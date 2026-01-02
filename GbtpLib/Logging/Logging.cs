using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
/*
사용 방법 (Quick start)
1) 전역 기본 옵션 구성 (먼저 호출: 로그 경로/롤링/보존기간/Debug 동시 출력 등)
    GbtpLib.Logging.LoggerFactory.ConfigureDefaults(new GbtpLib.Logging.LoggerOptions
    {
        LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs"),
        RollInterval = TimeSpan.FromDays(1),  // 일 단위 파일 교체
        Retention = TimeSpan.FromDays(120),   // 보존기간
        WriteToDebugAlso = true,
    });

2) 간편 퍼사드 사용 (접두사 또는 날짜 템플릿을 지정)
    // string.Format 스타일 지원: "Log_{yyyyMMdd}", "Log-{0:yyyyMMdd_HHmmss}"
    GbtpLib.Logging.AppLog.Use("Log_{yyMMdd}");
    GbtpLib.Logging.AppLog.Info("앱 시작");

3) (대안) 전역 로거 직접 사용
    var log = GbtpLib.Logging.LoggerFactory.GetGlobal("device_yyyyMMdd");
    log.Info("시작합니다");

4) 여러 전역 로거 동시 사용 (각각 다른 prefix/템플릿으로 별도 파일 생성)
    var deviceLog = GbtpLib.Logging.LoggerFactory.GetGlobal("device_{yyyyMMdd}");
    var networkLog = GbtpLib.Logging.LoggerFactory.GetGlobal("network_{0:yyyyMMdd_HHmmss}");
    deviceLog.Trace("장치 초기화");
    networkLog.Error("연결 실패", ex);

5) 바로 호출해서 단건 사용
    GbtpLib.Logging.LoggerFactory.GetGlobal("billing_yyMMdd").Warn("대기열 길이: " + queue.Count);

6) 앱 종료 시 정리
    GbtpLib.Logging.LoggerFactory.ShutdownGlobal("device_{yyyyMMdd}");
    // 또는 모두 정리
    GbtpLib.Logging.LoggerFactory.ShutdownAllGlobals();

파일명/롤링 동작
- 파일 prefix에 날짜/시간 토큰(yyMMdd, yyyyMMdd, yyyyMMdd_HHmmss, yyyyMMddHHmmss 등) 또는
  string.Format 플레이스홀더({yyyyMMdd}, {0:yyyyMMdd_HHmmss})를 사용할 수 있습니다.
- 토큰은 현재 시간으로 치환되며, 날짜 경계가 바뀌거나 롤링 간격이 경과하면 자동으로 새 파일이 열립니다.
- Windows 파일명에 허용되지 않는 문자는 자동으로 '-'로 대체됩니다.

출력 형식
- Debug 출력: [LEVEL] yyyy-MM-dd HH:mm:ss.fff [Class.Member] 메시지 Exception
- CSV 컬럼: Time,Lvl,Class,Member,Message,Exception (Time은 한국 시간 KST, ISO-8601 오프셋 +09:00 포함)

기타
- CallerMemberName/CallerFilePath 특성으로 멤버/클래스 정보가 자동으로 채워집니다.
- 기본 로그 경로는 실행 폴더의 Logs (또는 ConfigureDefaults.LogDirectory로 지정) 입니다.
- 기본 보존기간은 120일이며, 설정으로 변경 가능.
*/

namespace GbtpLib.Logging
{
    #region Public API

    /// <summary>
    /// 간단한 로깅 인터페이스. 호출자 정보는 컴파일러가 자동으로 채웁니다.
    /// </summary>
    public interface ILogger
    {
        void Trace(string message, Exception ex = null, [CallerMemberName] string memberName = null, [CallerFilePath] string sourceFilePath = null);
        void Info(string message, Exception ex = null, [CallerMemberName] string memberName = null, [CallerFilePath] string sourceFilePath = null);
        void Warn(string message, Exception ex = null, [CallerMemberName] string memberName = null, [CallerFilePath] string sourceFilePath = null);
        void Error(string message, Exception ex = null, [CallerMemberName] string memberName = null, [CallerFilePath] string sourceFilePath = null);
        void Critical(string message, Exception ex = null, [CallerMemberName] string memberName = null, [CallerFilePath] string sourceFilePath = null);
    }

    /// <summary>
    /// 로거 생성/구성 및 전역 레지스트리 관리.
    /// </summary>
    public static class LoggerFactory
    {
        #region Defaults & options
        private static LoggerOptions _defaults = new LoggerOptions();
        private static bool _defaultsConfigured; // ConfigureDefaults가 호출되었는지 여부

        /// <summary>
        /// 전역 기본 옵션을 설정합니다. null 을 전달하면 기본값으로 초기화됩니다.
        /// </summary>
        public static void ConfigureDefaults(LoggerOptions options)
        {
            _defaults = options ?? new LoggerOptions();
            _defaultsConfigured = true;
        }
        #endregion

        #region Factory
        /// <summary>
        /// 지정된 이름을 CSV 파일 prefix로 사용하는 기본 로거(MultiLogger)를 생성합니다.
        /// Debug 출력 중복 방지를 위해 WriteToDebugAlso=true 이면 DebugOutputLogger는 추가하지 않습니다.
        /// </summary>
        public static ILogger CreateInstance(string name)
        {
            if (_defaults.WriteToDebugAlso)
            {
                // CSV 로거가 파일과 Debug 양쪽에 기록. DebugOutputLogger는 추가하지 않음.
                var csv = new CsvRollingFileLogger(
                    logDirectory: _defaults.LogDirectory,
                    filePrefix: name,
                    rollInterval: _defaults.RollInterval,
                    writeToDebugAlso: true,
                    retention: _defaults.Retention);
                return new MultiLogger(csv);
            }
            else
            {
                // 파일 전용 CSV + DebugOutput
                var debug = new DebugOutputLogger();
                var csv = new CsvRollingFileLogger(
                    logDirectory: _defaults.LogDirectory,
                    filePrefix: name,
                    rollInterval: _defaults.RollInterval,
                    writeToDebugAlso: false,
                    retention: _defaults.Retention);
                return new MultiLogger(debug, csv);
            }
        }
        #endregion

        #region Global registry
        // 전역적으로 접근 가능한 로거 인스턴스들 (이름 기반, 지연 초기화)
        private static readonly ConcurrentDictionary<string, ILogger> _globals = new ConcurrentDictionary<string, ILogger>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 이름 기반 전역 로거를 가져옵니다. 없으면 생성합니다. 이름은 필수입니다.
        /// </summary>
        public static ILogger GetGlobal(string name)
        {
            if (!_defaultsConfigured)
                throw new InvalidOperationException("LoggerFactory.ConfigureDefaults(options)를 먼저 호출해 기본 옵션을 설정하세요.");

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Global logger name is required.", nameof(name));

            return _globals.GetOrAdd(name, n => CreateInstance(n));
        }

        /// <summary>
        /// 지정한 이름의 전역 로거를 종료하고 레지스트리에서 제거합니다.
        /// </summary>
        public static void ShutdownGlobal(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Global logger name is required.", nameof(name));

            if (_globals.TryRemove(name, out var old) && old is IDisposable d)
            {
                try { d.Dispose(); } catch { /* ignore */ }
            }
        }

        /// <summary>
        /// 모든 전역 로거를 종료하고 레지스트리를 초기화합니다.
        /// </summary>
        public static void ShutdownAllGlobals()
        {
            foreach (var kv in _globals)
            {
                if (_globals.TryRemove(kv.Key, out var value) && value is IDisposable d)
                {
                    try { d.Dispose(); } catch { /* ignore */ }
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// LoggerFactory 의 기본 생성 옵션.
    /// </summary>
    public sealed class LoggerOptions
    {
        /// <summary>로그 디렉터리. null 이면 기본 경로(실행폴더/logs).</summary>
        public string LogDirectory { get; set; }
        /// <summary>롤링 간격. null 이면 1일.</summary>
        public TimeSpan? RollInterval { get; set; }
        /// <summary>Debug 출력에도 동시에 쓰기.</summary>
        public bool WriteToDebugAlso { get; set; }
        /// <summary>보존기간. null 이면 120일.</summary>
        public TimeSpan? Retention { get; set; }
    }

    /// <summary>
    /// 간단 퍼사드. 한 번 이름을 설정하고 정적 메서드로 사용합니다.
    /// </summary>
    public static class AppLog
    {
        private static string _name;

        /// <summary>이 퍼사드가 사용할 전역 로거 이름을 설정합니다.</summary>
        public static void Use(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
            _name = name;
        }

        private static ILogger Current
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_name))
                    throw new InvalidOperationException("AppLog.Use(name) 을 먼저 호출해 이름을 설정하세요.");
                return LoggerFactory.GetGlobal(_name);
            }
        }

        public static void Trace(string message, Exception ex = null, [CallerMemberName] string memberName = null, [CallerFilePath] string sourceFilePath = null)
        {
            // Debug 출력도 항상 수행
            System.Diagnostics.Debug.WriteLine(LogFormatting.FormatStructuredLine("TRACE", message, ex, memberName, sourceFilePath));
            if (!string.IsNullOrWhiteSpace(_name))
                Current.Trace(message, ex, memberName, sourceFilePath);
        }
        public static void Info(string message, Exception ex = null, [CallerMemberName] string memberName = null, [CallerFilePath] string sourceFilePath = null)
        {
            // Debug 출력도 항상 수행
            System.Diagnostics.Debug.WriteLine(LogFormatting.FormatStructuredLine("INFO", message, ex, memberName, sourceFilePath));
            if (!string.IsNullOrWhiteSpace(_name))
                Current.Info(message, ex, memberName, sourceFilePath);
        }
        public static void Warn(string message, Exception ex = null, [CallerMemberName] string memberName = null, [CallerFilePath] string sourceFilePath = null)
        {
            // Debug 출력도 항상 수행
            System.Diagnostics.Debug.WriteLine(LogFormatting.FormatStructuredLine("WARN", message, ex, memberName, sourceFilePath));
            if (!string.IsNullOrWhiteSpace(_name))
                Current.Warn(message, ex, memberName, sourceFilePath);
        }
        public static void Error(string message, Exception ex = null, [CallerMemberName] string memberName = null, [CallerFilePath] string sourceFilePath = null)
        {
            // Debug 출력도 항상 수행
            System.Diagnostics.Debug.WriteLine(LogFormatting.FormatStructuredLine("ERROR", message, ex, memberName, sourceFilePath));
            if (!string.IsNullOrWhiteSpace(_name))
                Current.Error(message, ex, memberName, sourceFilePath);
        }
        public static void Critical(string message, Exception ex = null, [CallerMemberName] string memberName = null, [CallerFilePath] string sourceFilePath = null)
        {
            // Debug 출력도 항상 수행
            System.Diagnostics.Debug.WriteLine(LogFormatting.FormatStructuredLine("CRITICAL", message, ex, memberName, sourceFilePath));
            if (!string.IsNullOrWhiteSpace(_name))
                Current.Critical(message, ex, memberName, sourceFilePath);
        }
    }

    #endregion

    #region Formatting helpers

    internal static class LogFormatting
    {
        public static string ExtractTypeNameFromPath(string sourceFilePath)
        {
            if (string.IsNullOrEmpty(sourceFilePath)) return "?";
            try
            {
                // 사용자가 클래스명을 직접 전달하는 경우는 그대로 사용하고,
                // 경로처럼 보이는 경우만 파일명(확장자 제거)으로 변환
                bool looksLikePath =
                    sourceFilePath.IndexOf(Path.DirectorySeparatorChar) >= 0 ||
                    sourceFilePath.IndexOf(Path.AltDirectorySeparatorChar) >= 0 ||
                    sourceFilePath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) ||
                    sourceFilePath.IndexOf(':') >= 0;

                if (!looksLikePath)
                    return sourceFilePath; // 클래스명으로 간주

                return Path.GetFileNameWithoutExtension(sourceFilePath);
            }
            catch { return sourceFilePath; }
        }

        public static string GetCurrentTimestamp()
            => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);

        public static string FormatStructuredLine(string level, string message, Exception ex, string memberName, string sourceFilePath)
        {
            var cls = ExtractTypeNameFromPath(sourceFilePath);
            var time = GetCurrentTimestamp();
            var exText = ex == null ? string.Empty : " " + ex;
            return $"[{level}] {time} [{cls}.{memberName}] {message}{exText}";
        }
    }

    #endregion

    #region Logger implementations

    /// <summary>
    /// Debug 출력창으로 로그를 쓰는 로거.
    /// </summary>
    internal sealed class DebugOutputLogger : ILogger
    {
        private void WriteInternal(string level, string message, Exception ex, string memberName, string sourceFilePath)
        {
            System.Diagnostics.Debug.WriteLine(LogFormatting.FormatStructuredLine(level, message, ex, memberName, sourceFilePath));
        }

        public void Trace(string message, Exception ex = null, [CallerMemberName] string memberName = null, [CallerFilePath] string sourceFilePath = null)
        {
            WriteInternal("TRACE", message, ex, memberName, sourceFilePath);
        }

        public void Info(string message, Exception ex = null, [CallerMemberName] string memberName = null, [CallerFilePath] string sourceFilePath = null)
        {
            WriteInternal("INFO", message, ex, memberName, sourceFilePath);
        }

        public void Warn(string message, Exception ex = null, [CallerMemberName] string memberName = null, [CallerFilePath] string sourceFilePath = null)
        {
            WriteInternal("WARN", message, ex, memberName, sourceFilePath);
        }

        public void Error(string message, Exception ex = null, [CallerMemberName] string memberName = null, [CallerFilePath] string sourceFilePath = null)
        {
            WriteInternal("ERROR", message, ex, memberName, sourceFilePath);
        }

        public void Critical(string message, Exception ex = null, [CallerMemberName] string memberName = null, [CallerFilePath] string sourceFilePath = null)
        {
            WriteInternal("CRITICAL", message, ex, memberName, sourceFilePath);
        }
    }

    /// <summary>
    /// 여러 로거에 동시에 기록하는 복합 로거.
    /// </summary>
    internal sealed class MultiLogger : ILogger, IDisposable
    {
        private readonly ILogger[] _loggers;

        public MultiLogger(params ILogger[] loggers)
        {
            _loggers = loggers ?? Array.Empty<ILogger>();
        }

        public void Trace(string message, Exception ex = null, [CallerMemberName] string memberName = null, [CallerFilePath] string sourceFilePath = null)
        {
            foreach (var l in _loggers) l.Trace(message, ex, memberName, sourceFilePath);
        }

        public void Info(string message, Exception ex = null, [CallerMemberName] string memberName = null, [CallerFilePath] string sourceFilePath = null)
        {
            foreach (var l in _loggers) l.Info(message, ex, memberName, sourceFilePath);
        }

        public void Warn(string message, Exception ex = null, [CallerMemberName] string memberName = null, [CallerFilePath] string sourceFilePath = null)
        {
            foreach (var l in _loggers) l.Warn(message, ex, memberName, sourceFilePath);
        }

        public void Error(string message, Exception ex = null, [CallerMemberName] string memberName = null, [CallerFilePath] string sourceFilePath = null)
        {
            foreach (var l in _loggers) l.Error(message, ex, memberName, sourceFilePath);
        }

        public void Critical(string message, Exception ex = null, [CallerMemberName] string memberName = null, [CallerFilePath] string sourceFilePath = null)
        {
            foreach (var l in _loggers) l.Critical(message, ex, memberName, sourceFilePath);
        }

        public void Dispose()
        {
            foreach (var l in _loggers)
            {
                try
                {
                    if (l is IDisposable d) d.Dispose();
                }
                catch { /* ignore logger-dispose failures */ }
            }
        }
    }

    /// <summary>
    /// CSV 파일에 로그를 쓰고, 지정 간격으로 파일을 롤링합니다.
    /// </summary>
    internal sealed class CsvRollingFileLogger : ILogger, IDisposable
    {
        private const string CsvHeader = "Time,Lvl,Class,Member,Message,Exception";

        private static readonly Regex PlaceholderWithIndex = new Regex(@"\{0:([^}]+)\}", RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private static readonly Regex PlaceholderWithoutIndex = new Regex(@"\{([^}:]+(?:[^}]*)?)\}", RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private static readonly TimeZoneInfo KstTimeZone = InitKstTimeZone();

        private readonly object _lockObj = new object();
        private readonly string _logsDirectoryPath;
        private readonly string _fileNamePrefix;
        private readonly TimeSpan _rotationInterval;
        private readonly bool _writeToDebugAlso;
        private readonly TimeSpan _retention;

        private DateTime _currentRotationStart;
        private string _currentFilePath;
        private StreamWriter _logWriter;
        private bool _disposed;

        public CsvRollingFileLogger(string logDirectory = null, string filePrefix = "log", TimeSpan? rollInterval = null, bool writeToDebugAlso = false, TimeSpan? retention = null)
        {
            _logsDirectoryPath = string.IsNullOrWhiteSpace(logDirectory)
                ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs")
                : logDirectory;
            _fileNamePrefix = string.IsNullOrWhiteSpace(filePrefix) ? "Log" : filePrefix;
            _rotationInterval = rollInterval ?? TimeSpan.FromDays(1); // 기본 일 단위 교체
            _writeToDebugAlso = writeToDebugAlso;
            _retention = retention ?? TimeSpan.FromDays(120);

            Directory.CreateDirectory(_logsDirectoryPath);
            _currentRotationStart = DateTime.MinValue;
            _currentFilePath = null;
            _disposed = false;
        }

        private static TimeZoneInfo InitKstTimeZone()
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("Korea Standard Time"); }
            catch { return TimeZoneInfo.Local; }
        }

        private static DateTime GetRotationPeriodStart(DateTime dt, TimeSpan interval)
        {
            if (interval.Ticks <= 0) return dt;
            long ticks = (dt.Ticks / interval.Ticks) * interval.Ticks;
            return new DateTime(ticks);
        }

        private static string SanitizeForFileName(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            var invalid = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder(s.Length);
            foreach (var ch in s)
            {
                if (Array.IndexOf(invalid, ch) >= 0)
                {
                    sb.Append('-');
                }
                else
                {
                    sb.Append(ch);
                }
            }
            return sb.ToString();
        }

        private static string SanitizeForFilePattern(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            var invalid = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder(s.Length);
            foreach (var ch in s)
            {
                if (ch == '*') { sb.Append('*'); continue; }
                if (Array.IndexOf(invalid, ch) >= 0)
                {
                    sb.Append('-');
                }
                else
                {
                    sb.Append(ch);
                }
            }
            return sb.ToString();
        }

        private static string ReplaceDatePlaceholders(string template, DateTime dt)
        {
            if (string.IsNullOrEmpty(template)) return template;

            // {0:yyyyMMdd...} 형태
            template = PlaceholderWithIndex.Replace(template, m => SanitizeForFileName(dt.ToString(m.Groups[1].Value, CultureInfo.InvariantCulture)));

            // {yyyyMMdd...} 형태
            template = PlaceholderWithoutIndex.Replace(template, m => SanitizeForFileName(dt.ToString(m.Groups[1].Value, CultureInfo.InvariantCulture)));

            return template;
        }

        private static bool ContainsBareDateTokens(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            return s.Contains("yyyyMMddHHmmss") || s.Contains("yyyyMMdd_HHmmss") || s.Contains("yyyyMMdd") || s.Contains("yyMMdd");
        }

        private static string ReplaceBareDateTokens(string template, DateTime dt)
        {
            if (string.IsNullOrEmpty(template)) return template;
            // 긴 토큰부터 치환하여 오염 방지
            template = template.Replace("yyyyMMddHHmmss", dt.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture));
            template = template.Replace("yyyyMMdd_HHmmss", dt.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture));
            template = template.Replace("yyyyMMdd", dt.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
            template = template.Replace("yyMMdd", dt.ToString("yyMMdd", CultureInfo.InvariantCulture));
            return template;
        }

        private string BuildLogFilePath(DateTime periodStart)
        {
            // 날짜/시간 치환: 중괄호 플레이스홀더가 있으면 치환
            if (_fileNamePrefix.IndexOf('{') >= 0 && _fileNamePrefix.IndexOf('}') > _fileNamePrefix.IndexOf('{'))
            {
                var replaced = ReplaceDatePlaceholders(_fileNamePrefix, periodStart);
                var nameFromTemplate = string.IsNullOrWhiteSpace(replaced) ? "Log" : replaced;
                nameFromTemplate = SanitizeForFileName(nameFromTemplate);
                return Path.Combine(_logsDirectoryPath, nameFromTemplate + ".csv");
            }

            // 중괄호가 없고, 알려진 날짜 토큰이 포함되면 해당 토큰 치환
            if (ContainsBareDateTokens(_fileNamePrefix))
            {
                var replaced = ReplaceBareDateTokens(_fileNamePrefix, periodStart);
                var nameFromTemplate = string.IsNullOrWhiteSpace(replaced) ? "Log" : replaced;
                nameFromTemplate = SanitizeForFileName(nameFromTemplate);
                return Path.Combine(_logsDirectoryPath, nameFromTemplate + ".csv");
            }

            // 토큰이 없으면 literal 접두사 그대로 사용 (자동 접미사 추가 없음)
            var nameWithoutExt = SanitizeForFileName(_fileNamePrefix);
            return Path.Combine(_logsDirectoryPath, nameWithoutExt + ".csv");
        }

        private string BuildCleanupSearchPattern()
        {
            // 플레이스홀더가 있으면 와일드카드로 대체
            if (_fileNamePrefix.IndexOf('{') >= 0 && _fileNamePrefix.IndexOf('}') > _fileNamePrefix.IndexOf('{'))
            {
                var pattern = PlaceholderWithIndex.Replace(_fileNamePrefix, "*");
                pattern = PlaceholderWithoutIndex.Replace(pattern, "*");
                return SanitizeForFilePattern(pattern) + ".csv";
            }

            // 중괄호가 없지만 날짜 토큰이 있으면 해당 토큰을 와일드카드로 대체
            if (ContainsBareDateTokens(_fileNamePrefix))
            {
                var pattern = _fileNamePrefix
                    .Replace("yyyyMMddHHmmss", "*")
                    .Replace("yyyyMMdd_HHmmss", "*")
                    .Replace("yyyyMMdd", "*")
                    .Replace("yyMMdd", "*");
                return SanitizeForFilePattern(pattern) + ".csv";
            }

            // 플레이스홀더/토큰이 없으면 literal 파일만 정리 대상
            return SanitizeForFilePattern(_fileNamePrefix) + ".csv";
        }

        private void CleanupOldLogs()
        {
            try
            {
                if (!Directory.Exists(_logsDirectoryPath)) return;
                var cutoff = DateTime.Now - _retention;
                var pattern = BuildCleanupSearchPattern();
                var files = Directory.GetFiles(_logsDirectoryPath, pattern);
                foreach (var file in files)
                {
                    try
                    {
                        var info = new FileInfo(file);
                        var stamp = info.LastWriteTime;
                        if (stamp < cutoff)
                        {
                            info.Delete();
                        }
                    }
                    catch { /* ignore one-file failures */ }
                }
            }
            catch { /* ignore cleanup failures */ }
        }

        private void EnsureLogWriter()
        {
            if (_disposed) return;

            var now = DateTime.Now;
            var periodStart = GetRotationPeriodStart(now, _rotationInterval);
            var desiredPath = BuildLogFilePath(periodStart);

            // 파일명 토큰(날짜)이 바뀌었거나 회전 주기가 바뀐 경우 새 파일로 전환
            if (_logWriter != null && string.Equals(_currentFilePath, desiredPath, StringComparison.OrdinalIgnoreCase) && periodStart == _currentRotationStart)
            {
                return; // 현재 파일 계속 사용
            }

            // 롤링 필요: 기존 파일 닫기 후 새 파일 오픈
            if (_logWriter != null)
            {
                try { _logWriter.Flush(); _logWriter.Dispose(); } catch { /* ignore */ }
                _logWriter = null;
            }

            // 오래된 로그 정리 (보존기간 경과분 삭제)
            CleanupOldLogs();

            _currentRotationStart = periodStart;
            _currentFilePath = desiredPath;
            var exists = File.Exists(_currentFilePath);
            _logWriter = new StreamWriter(new FileStream(_currentFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite), Encoding.UTF8)
            {
                AutoFlush = true
            };
            if (!exists)
            {
                // 헤더 작성
                _logWriter.WriteLine(CsvHeader);
            }
        }

        private static string EscapeCsvField(string value)
        {
            if (value == null) return string.Empty;
            var s = value.Replace("\r", " ").Replace("\n", " ");
            if (s.IndexOf('"') >= 0 || s.IndexOf(',') >= 0)
            {
                s = '"' + s.Replace("\"", "\"\"") + '"';
            }
            return s;
        }

        private void WriteInternal(string level, string message, Exception ex, string memberName, string sourceFilePath)
        {
            try
            {
                if (_disposed) return;

                lock (_lockObj)
                {
                    if (_disposed) return;
                    EnsureLogWriter();

                    // 한국 시간(KST, +09:00)으로 타임스탬프 기록
                    var utcNow = DateTime.UtcNow;
                    var kstNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, KstTimeZone);
                    var kstOffset = KstTimeZone.GetUtcOffset(utcNow);
                    var kstNowOffset = new DateTimeOffset(kstNow, kstOffset);
                    var time = kstNowOffset.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", CultureInfo.InvariantCulture);

                    var line = string.Concat(
                        EscapeCsvField(time), ",",
                        EscapeCsvField(level), ",",
                        EscapeCsvField(LogFormatting.ExtractTypeNameFromPath(sourceFilePath)), ",",
                        EscapeCsvField(memberName ?? string.Empty), ",",
                        EscapeCsvField(message ?? string.Empty), ",",
                        EscapeCsvField(ex == null ? string.Empty : ex.ToString())
                    );
                    _logWriter.WriteLine(line);
                }

                if (_writeToDebugAlso)
                {
                    System.Diagnostics.Debug.WriteLine(LogFormatting.FormatStructuredLine(level, message, ex, memberName, sourceFilePath));
                }
            }
            catch
            {
                // 파일 문제가 있어도 앱이 중단되지 않도록 무시
            }
        }

        public void Trace(string message, Exception ex = null, [CallerMemberName] string memberName = null, [CallerFilePath] string sourceFilePath = null)
        {
            WriteInternal("TRACE", message, ex, memberName, sourceFilePath);
        }

        public void Info(string message, Exception ex = null, [CallerMemberName] string memberName = null, [CallerFilePath] string sourceFilePath = null)
        {
            WriteInternal("INFO", message, ex, memberName, sourceFilePath);
        }

        public void Warn(string message, Exception ex = null, [CallerMemberName] string memberName = null, [CallerFilePath] string sourceFilePath = null)
        {
            WriteInternal("WARN", message, ex, memberName, sourceFilePath);
        }

        public void Error(string message, Exception ex = null, [CallerMemberName] string memberName = null, [CallerFilePath] string sourceFilePath = null)
        {
            WriteInternal("ERROR", message, ex, memberName, sourceFilePath);
        }

        public void Critical(string message, Exception ex = null, [CallerMemberName] string memberName = null, [CallerFilePath] string sourceFilePath = null)
        {
            WriteInternal("CRITICAL", message, ex, memberName, sourceFilePath);
        }

        public void Dispose()
        {
            lock (_lockObj)
            {
                if (_disposed) return;
                _disposed = true;
                if (_logWriter != null)
                {
                    try { _logWriter.Flush(); _logWriter.Dispose(); } catch { /* ignore */ }
                    _logWriter = null;
                }
            }
        }
    }

    #endregion
}

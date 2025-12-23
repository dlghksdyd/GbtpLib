using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Data.SqlClient;
/* 사용 가이드 (순서대로)

0) 개요
- 이 라이브러리는 프로세스/환경 메트릭과 HealthCheck 결과를 수집해 HealthInfo 스냅샷을 제공합니다.
- 1회 초기화 값은 내부 캐시에 저장되고, 매 호출 시 실시간 항목만 갱신됩니다.

1) Reporter 선택
- 단순 메트릭만 필요: BasicHealthReporter
  ```csharp
  var reporter = new BasicHealthReporter();
  ```
- 여러 체크 종합: CompositeHealthReporter
  ```csharp
  var reporter = new CompositeHealthReporter(new IHealthCheck[] { [checks] }, TimeSpan.FromSeconds(2));
  ```
  perCheckTimeout 인자는 선택(각 체크 최대 대기시간), null이면 무제한.
- 또는, 헬퍼 API 한 번 호출로 시작: HealthMonitor.Start(options)
  아래 7번 섹션 참고(기본값 포함)

2) HealthCheck 준비
- SystemHealthCheck (프로세스/디스크 임계치)
  임계치 기본값: 모두 null(제한 없음)
  ```csharp
  var thresholds = new SystemHealthThresholds {
      MaxWorkingSetMB = 1024,       // 기본 null
      MaxPrivateBytesMB = 2048,     // 기본 null
      MaxAvgCpuPercent = 80,        // 기본 null
      MinDiskFreeGB = 1,            // 기본 null
      MinDiskFreePercent = 10       // 기본 null
  };
  ```
  driveRoot 기본값
  - SystemHealthCheck 생성자에서 null 전달 시: 현재 작업 디렉터리의 드라이브가 사용됩니다.
  - HealthMonitorOptions를 사용할 때: 기본값은 Path.GetPathRoot(Environment.CurrentDirectory)입니다.
  ```csharp
  var systemCheck = new SystemHealthCheck(thresholds, null);
  ```

- SqlServerHealthCheck (데이터베이스 연결 확인)
  기본값: commandText = "SELECT 1", commandTimeoutSeconds = 5
  ```csharp
  var sqlCheck = new SqlServerHealthCheck(
      connectionString: "Server=.;Database=master;Integrated Security=True;",
      commandText: "SELECT 1",
      commandTimeoutSeconds: 5
  );
  ```

- 사용자 정의 체크(IHealthCheck 구현)
  ```csharp
  public class MyCustomCheck : IHealthCheck {
      public string Name => "my-custom";
      public HealthCheckResult Check() {
          var sw = Stopwatch.StartNew();
          try {
              // 점검 로직 수행
              sw.Stop();
              return new HealthCheckResult { Name = Name, Status = HealthStatus.Healthy, Duration = sw.Elapsed, Description = "OK" };
          } catch (Exception ex) {
              sw.Stop();
              return new HealthCheckResult { Name = Name, Status = HealthStatus.Unhealthy, Duration = sw.Elapsed, Error = ex.Message, Description = "Failed" };
          }
      }
  }
  ```

3) Reporter 인스턴스 생성
- 종합 체크:
  ```csharp
  var checks = new IHealthCheck[] { systemCheck, sqlCheck, new MyCustomCheck() };
  var reporter = new CompositeHealthReporter(checks, TimeSpan.FromSeconds(2)); // perCheckTimeout 생략 시 null(무제한)
  ```
- 기본 메트릭만:
  ```csharp
  var reporter = new BasicHealthReporter();
  ```

4) 주기적으로 스냅샷 수집
- 타이머에서 주기 호출(예: 10초)
  ```csharp
  var timer = new System.Timers.Timer(10000);
  timer.Elapsed += (s, e) => {
      try {
          var info = reporter.GetHealth();
          // 직렬화/로그/전송 등 처리 (예: var json = Newtonsoft.Json.JsonConvert.SerializeObject(info));
      } catch { [logging] }
  };
  timer.AutoReset = true;
  timer.Start();
  ```

5) 필드 갱신 정책
- 1회 초기화(캐시): ApplicationName, Version, FrameworkVersion, EnvironmentName, InstanceId, StartTimeUtc, MachineName, ProcessId,
  정적 Metadata(OSVersion, ProcessorCount, Is64BitProcess, BaseDirectory, UserName, MachineName, CommandLine, AppDomain)
- 매 호출 갱신: Timestamp, Uptime, Status, IsHealthy, Details, Checks, Metrics,
  변동 Metadata(CurrentDirectory, Culture, UICulture, Disk.Root 등)

6) 주의사항(초기/기본 동작)
- BasicHealthReporter 초기 스냅샷: Status=Healthy, IsHealthy=true, Details="OK".
- CPU.Percent.AverageSinceStart는 시작 이후 평균값(순간값 아님).
- 디스크 메트릭은 현재 작업 디렉터리 드라이브 기준(SystemHealthCheck 생성 시 driveRoot로 변경 가능).
- CompositeHealthReporter perCheckTimeout은 동기 대기(Task.Run + Wait). 너무 짧으면 타임아웃 발생 가능.
- 프레임워크 버전 조회 실패 시 CLR Version으로 대체.

7) 원-콜(One-call) 헬퍼 사용: HealthMonitor
- HealthMonitorOptions 기본값(초기값)
  ```csharp
  EnableSystemCheck = true
  SystemThresholds = new SystemHealthThresholds()
  DriveRoot = Path.GetPathRoot(Environment.CurrentDirectory)
  SqlConnectionString = null // 미사용 시 null
  SqlCommandText = "SELECT 1"
  SqlCommandTimeoutSeconds = 5
  AdditionalChecks = new List<IHealthCheck>()
  PerCheckTimeout = null // 무제한
  Interval = TimeSpan.FromSeconds(10)
  RunImmediately = true
  ```

- 최소 예시
  ```csharp
  var stop = HealthMonitor.Start(new HealthMonitorOptions {
      // 필요 시 옵션 지정, 지정하지 않으면 위의 기본값 사용
      OnSnapshot = info => Console.WriteLine(info.Status),
      OnError = ex => Console.WriteLine(ex.Message)
  });
  // 중지
  stop.Dispose();
  ```

- 수동 최소 예시
```csharp
// Basic
var basic = new BasicHealthReporter();
var snap1 = basic.GetHealth();

// Composite
var th = new SystemHealthThresholds { MaxAvgCpuPercent = 85, MinDiskFreeGB = 1 };
var composite = new CompositeHealthReporter(new IHealthCheck[] {
    new SystemHealthCheck(th, null),
    new SqlServerHealthCheck("Server=.;Database=master;Integrated Security=True;", "SELECT 1", 3)
}, TimeSpan.FromSeconds(2));
var snap2 = composite.GetHealth();
```
*/

namespace GbtpLib.Monitoring
{
    public enum HealthStatus
    {
        Healthy = 0,
        Degraded = 1,
        Unhealthy = 2
    }

    public class HealthCheckResult
    {
        public string Name { get; set; }
        public HealthStatus Status { get; set; }
        public TimeSpan Duration { get; set; }
        public string Target { get; set; }
        public string Description { get; set; }
        public string Error { get; set; }
        public string[] Tags { get; set; }
    }

    public class HealthInfo
    {
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // App info
        public string ApplicationName { get; set; }
        public string Version { get; set; }
        public string FrameworkVersion { get; set; }
        public string EnvironmentName { get; set; }
        public string InstanceId { get; set; }
        public DateTime StartTimeUtc { get; set; }

        // Runtime/process info
        public string MachineName { get; set; }
        public int ProcessId { get; set; }
        public TimeSpan Uptime { get; set; }

        // Status
        public bool IsHealthy { get; set; }
        public HealthStatus Status { get; set; }
        public string Details { get; set; }

        // Extensibility
        public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
        public IDictionary<string, double> Metrics { get; set; } = new Dictionary<string, double>();
        public IList<HealthCheckResult> Checks { get; set; } = new List<HealthCheckResult>();
    }

    public interface IHealthReporter
    {
        HealthInfo GetHealth();
    }

    public interface IHealthCheck
    {
        string Name { get; }
        HealthCheckResult Check();
    }

    public class BasicHealthReporter : IHealthReporter
    {
        public HealthInfo GetHealth()
        {
            // Create a fresh snapshot from cached static/base info and populate dynamic parts
            var info = HealthUtils.CloneBaseInfo();
            info.Timestamp = DateTime.UtcNow;
            info.Uptime = info.StartTimeUtc == default(DateTime) ? TimeSpan.Zero : (DateTime.UtcNow - info.StartTimeUtc);
            info.Status = HealthStatus.Healthy;
            info.IsHealthy = true;
            info.Details = "OK";

            HealthUtils.PopulateProcessMetrics(info);
            HealthUtils.PopulateDynamicOsAndEnvironmentMetadata(info);

            return info;
        }
    }

    public class CompositeHealthReporter : IHealthReporter
    {
        private readonly IEnumerable<IHealthCheck> _healthChecks;
        private readonly TimeSpan? _perCheckTimeout;

        public CompositeHealthReporter(IEnumerable<IHealthCheck> checks)
            : this(checks, null)
        {
        }

        public CompositeHealthReporter(IEnumerable<IHealthCheck> checks, TimeSpan? perCheckTimeout)
        {
            _healthChecks = checks ?? Enumerable.Empty<IHealthCheck>();
            _perCheckTimeout = perCheckTimeout;
        }

        public HealthInfo GetHealth()
        {
            // Start with cached base info (static values) and update dynamic fields and checks
            var info = HealthUtils.CloneBaseInfo();
            info.Timestamp = DateTime.UtcNow;
            info.Uptime = info.StartTimeUtc == default(DateTime) ? TimeSpan.Zero : (DateTime.UtcNow - info.StartTimeUtc);
            info.Details = "Aggregated health checks";

            var checkResults = new List<HealthCheckResult>();
            foreach (var check in _healthChecks)
            {
                var stopwatch = Stopwatch.StartNew();
                HealthCheckResult result = null;
                try
                {
                    if (_perCheckTimeout.HasValue)
                    {
                        var checkTask = Task.Run(() => check.Check());
                        if (!checkTask.Wait(_perCheckTimeout.Value))
                        {
                            result = new HealthCheckResult
                            {
                                Name = check.Name,
                                Status = HealthStatus.Unhealthy,
                                Error = "Health check timed out after " + _perCheckTimeout.Value,
                                Description = "Timeout"
                            };
                        }
                        else
                        {
                            result = checkTask.Result ?? new HealthCheckResult { Name = check.Name, Status = HealthStatus.Healthy };
                        }
                    }
                    else
                    {
                        result = check.Check() ?? new HealthCheckResult { Name = check.Name, Status = HealthStatus.Healthy };
                    }
                }
                catch (Exception ex)
                {
                    result = new HealthCheckResult
                    {
                        Name = check.Name,
                        Status = HealthStatus.Unhealthy,
                        Error = ex.Message
                    };
                }
                finally
                {
                    stopwatch.Stop();
                }

                if (result.Duration == default(TimeSpan))
                {
                    result.Duration = stopwatch.Elapsed;
                }

                if (string.IsNullOrEmpty(result.Name))
                {
                    result.Name = check.Name;
                }

                checkResults.Add(result);
            }

            info.Checks = checkResults;

            var aggregate =
                checkResults.Any(r => r.Status == HealthStatus.Unhealthy) ? HealthStatus.Unhealthy :
                checkResults.Any(r => r.Status == HealthStatus.Degraded) ? HealthStatus.Degraded :
                HealthStatus.Healthy;

            info.Status = aggregate;
            info.IsHealthy = aggregate == HealthStatus.Healthy;

            HealthUtils.PopulateProcessMetrics(info);
            HealthUtils.PopulateDynamicOsAndEnvironmentMetadata(info);

            return info;
        }
    }

    public class SystemHealthThresholds
    {
        public double? MaxWorkingSetMB { get; set; }
        public double? MaxPrivateBytesMB { get; set; }
        public double? MaxAvgCpuPercent { get; set; }
        public double? MinDiskFreeGB { get; set; }
        public double? MinDiskFreePercent { get; set; }
    }

    public class SystemHealthCheck : IHealthCheck
    {
        public string Name { get { return "system"; } }

        private readonly SystemHealthThresholds _thresholds;
        private readonly string _driveRoot;

        public SystemHealthCheck()
            : this(null, null)
        {
        }

        public SystemHealthCheck(SystemHealthThresholds thresholds, string driveRoot)
        {
            _thresholds = thresholds ?? new SystemHealthThresholds();
            _driveRoot = string.IsNullOrEmpty(driveRoot) ? Path.GetPathRoot(Environment.CurrentDirectory) : driveRoot;
        }

        public HealthCheckResult Check()
        {
            var stopwatch = Stopwatch.StartNew();

            var process = Process.GetCurrentProcess();
            var workingSetMb = Math.Round(process.WorkingSet64 / 1024.0 / 1024.0, 2);
            var privateBytesMb = Math.Round(process.PrivateMemorySize64 / 1024.0 / 1024.0, 2);
            var uptime = DateTime.UtcNow - process.StartTime.ToUniversalTime();
            var averageCpu = HealthUtils.CalculateAverageCpuPercent(process, uptime);

            double? freeGb = null;
            double? freePercent = null;
            try
            {
                if (!string.IsNullOrEmpty(_driveRoot))
                {
                    var drive = new DriveInfo(_driveRoot);
                    if (drive.IsReady)
                    {
                        freeGb = Math.Round(drive.TotalFreeSpace / 1024.0 / 1024.0 / 1024.0, 2);
                        freePercent = Math.Round((drive.TotalFreeSpace / (double)drive.TotalSize) * 100.0, 2);
                    }
                }
            }
            catch
            {
                // ignore drive errors
            }

            var status = HealthStatus.Healthy;
            var description = "Process and system thresholds checked";

            if ((_thresholds.MaxWorkingSetMB.HasValue && workingSetMb > _thresholds.MaxWorkingSetMB.Value) ||
                (_thresholds.MaxPrivateBytesMB.HasValue && privateBytesMb > _thresholds.MaxPrivateBytesMB.Value) ||
                (_thresholds.MaxAvgCpuPercent.HasValue && averageCpu.HasValue && averageCpu.Value > _thresholds.MaxAvgCpuPercent.Value) ||
                (_thresholds.MinDiskFreeGB.HasValue && freeGb.HasValue && freeGb.Value < _thresholds.MinDiskFreeGB.Value) ||
                (_thresholds.MinDiskFreePercent.HasValue && freePercent.HasValue && freePercent.Value < _thresholds.MinDiskFreePercent.Value))
            {
                status = HealthStatus.Degraded;
                description = "One or more system metrics exceeded configured thresholds";
            }

            var result = new HealthCheckResult
            {
                Name = Name,
                Status = status,
                Description = description,
                Tags = new[] { "runtime", "process", "system" }
            };

            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;
            return result;
        }
    }

    // Database health check (SQL Server via ADO.NET)
    public class SqlServerHealthCheck : IHealthCheck
    {
        public string Name { get { return "sqlserver"; } }

        private readonly string _connectionString;
        private readonly string _commandText;
        private readonly int _commandTimeoutSeconds;

        public SqlServerHealthCheck(string connectionString)
            : this(connectionString, "SELECT 1", 5)
        {
        }

        public SqlServerHealthCheck(string connectionString, string commandText, int commandTimeoutSeconds)
        {
            _connectionString = connectionString;
            _commandText = string.IsNullOrEmpty(commandText) ? "SELECT 1" : commandText;
            _commandTimeoutSeconds = commandTimeoutSeconds > 0 ? commandTimeoutSeconds : 5;
        }

        public HealthCheckResult Check()
        {
            var stopwatch = Stopwatch.StartNew();
            var connectionStringBuilder = new SqlConnectionStringBuilder(_connectionString);
            var target = connectionStringBuilder.DataSource + (string.IsNullOrEmpty(connectionStringBuilder.InitialCatalog) ? string.Empty : "/" + connectionStringBuilder.InitialCatalog);
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = _commandText;
                    command.CommandTimeout = _commandTimeoutSeconds;
                    connection.Open();
                    var _ = command.ExecuteScalar();
                }

                stopwatch.Stop();
                return new HealthCheckResult
                {
                    Name = Name,
                    Status = HealthStatus.Healthy,
                    Duration = stopwatch.Elapsed,
                    Target = target,
                    Description = "SQL connectivity OK",
                    Tags = new[] { "db", "sqlserver" }
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return new HealthCheckResult
                {
                    Name = Name,
                    Status = HealthStatus.Unhealthy,
                    Duration = stopwatch.Elapsed,
                    Target = target,
                    Description = "SQL connectivity failed",
                    Error = ex.Message,
                    Tags = new[] { "db", "sqlserver" }
                };
            }
        }
    }

    internal static class HealthUtils
    {
        // Cache base/static info for the current process and reuse on each snapshot
        private static readonly Lazy<HealthInfo> _baseInfo = new Lazy<HealthInfo>(CreateBaseInfo, true);

        internal static HealthInfo ResolveBaseInfo()
        {
            return _baseInfo.Value;
        }

        internal static HealthInfo CloneBaseInfo()
        {
            var baseInfo = _baseInfo.Value;
            var info = new HealthInfo
            {
                ApplicationName = baseInfo.ApplicationName,
                Version = baseInfo.Version,
                FrameworkVersion = baseInfo.FrameworkVersion,
                EnvironmentName = baseInfo.EnvironmentName,
                InstanceId = baseInfo.InstanceId,
                StartTimeUtc = baseInfo.StartTimeUtc,
                MachineName = baseInfo.MachineName,
                ProcessId = baseInfo.ProcessId
            };

            // Copy static metadata
            if (baseInfo.Metadata != null)
            {
                foreach (var pair in baseInfo.Metadata)
                {
                    info.Metadata[pair.Key] = pair.Value;
                }
            }

            return info;
        }

        private static HealthInfo CreateBaseInfo()
        {
            var process = Process.GetCurrentProcess();
            var startUtc = process.StartTime.ToUniversalTime();
            var info = new HealthInfo
            {
                ApplicationName = AppDomain.CurrentDomain.FriendlyName,
                Version = ResolveVersion(),
                FrameworkVersion = ResolveFrameworkVersion(),
                EnvironmentName = ResolveEnvironmentName(),
                MachineName = Environment.MachineName,
                ProcessId = process.Id,
                StartTimeUtc = startUtc,
                InstanceId = ResolveInstanceId(process)
            };

            PopulateStaticOsAndEnvironmentMetadata(info);
            return info;
        }

        internal static string ResolveVersion()
        {
            var asm = Assembly.GetEntryAssembly() ?? typeof(BasicHealthReporter).Assembly;
            var infoVer = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (infoVer != null && !string.IsNullOrEmpty(infoVer.InformationalVersion))
                return infoVer.InformationalVersion;

            var fileVer = asm.GetCustomAttribute<AssemblyFileVersionAttribute>();
            if (fileVer != null && !string.IsNullOrEmpty(fileVer.Version))
                return fileVer.Version;

            var ver = asm.GetName().Version;
            return ver != null ? ver.ToString() : null;
        }

        internal static string ResolveFrameworkVersion()
        {
            try
            {
                using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
                    .OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full"))
                {
                    if (ndpKey != null)
                    {
                        var releaseObj = ndpKey.GetValue("Release");
                        if (releaseObj is int)
                        {
                            var release = (int)releaseObj;
                            return ".NET Framework " + MapReleaseToVersion(release);
                        }
                    }
                }
            }
            catch
            {
                // ignore and fallback
            }

            return ".NET Framework CLR " + Environment.Version;
        }

        private static string MapReleaseToVersion(int release)
        {
            // Simplified mapping covering 4.5 -> 4.8.1
            if (release >= 533320) return "4.8.1 or later";
            if (release >= 528040) return "4.8 or later";
            if (release >= 461808) return "4.7.2";
            if (release >= 461308) return "4.7.1";
            if (release >= 460798) return "4.7";
            if (release >= 394802) return "4.6.2";
            if (release >= 394254) return "4.6.1";
            if (release >= 393295) return "4.6";
            if (release >= 379893) return "4.5.2";
            if (release >= 378675) return "4.5.1";
            if (release >= 378389) return "4.5";
            return "4.x (unknown)";
        }

        internal static string ResolveEnvironmentName()
        {
            var env =
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ??
                Environment.GetEnvironmentVariable("ASPNET_ENV");
            return string.IsNullOrEmpty(env) ? "Production" : env;
        }

        internal static string ResolveInstanceId(Process p)
        {
            try
            {
                // Stable-ish instance id for this process instance
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}-{1}-{2}",
                    Environment.MachineName,
                    p.Id,
                    p.StartTime.ToUniversalTime().Ticks);
            }
            catch
            {
                return Environment.MachineName + "-" + Guid.NewGuid().ToString("N");
            }
        }

        internal static void PopulateProcessMetrics(HealthInfo info)
        {
            var p = Process.GetCurrentProcess();

            // Memory/handles/threads
            info.Metrics["Process.WorkingSetMB"] = Math.Round(p.WorkingSet64 / 1024.0 / 1024.0, 2);
            info.Metrics["Process.PrivateBytesMB"] = Math.Round(p.PrivateMemorySize64 / 1024.0 / 1024.0, 2);
            info.Metrics["Process.ThreadCount"] = p.Threads != null ? p.Threads.Count : 0;
            info.Metrics["Process.HandleCount"] = p.HandleCount;

            // GC
            info.Metrics["GC.HeapMB"] = Math.Round(GC.GetTotalMemory(false) / 1024.0 / 1024.0, 2);
            info.Metrics["GC.Gen0Collections"] = GC.CollectionCount(0);
            info.Metrics["GC.Gen1Collections"] = GC.CollectionCount(1);
            info.Metrics["GC.Gen2Collections"] = GC.CollectionCount(2);
            info.Metadata["GC.IsServerGC"] = GCSettings.IsServerGC.ToString();

            // CPU (average since process start)
            var uptime = DateTime.UtcNow - info.StartTimeUtc;
            var avgCpu = CalculateAverageCpuPercent(p, uptime);
            if (avgCpu.HasValue)
            {
                info.Metrics["CPU.Percent.AverageSinceStart"] = Math.Round(avgCpu.Value, 2);
            }

            // ThreadPool
            int aw, ai, mw, mi;
            ThreadPool.GetAvailableThreads(out aw, out ai);
            ThreadPool.GetMaxThreads(out mw, out mi);
            info.Metrics["ThreadPool.WorkersAvailable"] = aw;
            info.Metrics["ThreadPool.WorkersMax"] = mw;
            info.Metrics["ThreadPool.IOCPAvailable"] = ai;
            info.Metrics["ThreadPool.IOCPMax"] = mi;

            // Disk (drive of current directory)
            try
            {
                var root = Path.GetPathRoot(Environment.CurrentDirectory);
                if (!string.IsNullOrEmpty(root))
                {
                    var drive = new DriveInfo(root);
                    if (drive.IsReady)
                    {
                        var totalGB = drive.TotalSize / 1024.0 / 1024.0 / 1024.0;
                        var freeGB = drive.TotalFreeSpace / 1024.0 / 1024.0 / 1024.0;
                        info.Metrics["Disk.TotalGB"] = Math.Round(totalGB, 2);
                        info.Metrics["Disk.FreeGB"] = Math.Round(freeGB, 2);
                        info.Metrics["Disk.FreePercent"] = Math.Round((freeGB / totalGB) * 100.0, 2);
                        info.Metadata["Disk.Root"] = root;
                    }
                }
            }
            catch
            {
                // ignore disk metric errors
            }
        }

        internal static double? CalculateAverageCpuPercent(Process p, TimeSpan uptime)
        {
            try
            {
                if (uptime.TotalMilliseconds <= 0)
                    return null;

                var cpuMs = p.TotalProcessorTime.TotalMilliseconds;
                var wallMs = uptime.TotalMilliseconds * Environment.ProcessorCount;
                if (wallMs <= 0)
                    return null;

                var percent = (cpuMs / wallMs) * 100.0;
                if (percent < 0) percent = 0;
                if (percent > 100) percent = 100; // cap
                return percent;
            }
            catch
            {
                return null;
            }
        }

        // Split metadata population into static and dynamic parts to support 1-time init + periodic updates
        internal static void PopulateStaticOsAndEnvironmentMetadata(HealthInfo info)
        {
            info.Metadata["OSVersion"] = Environment.OSVersion.VersionString;
            info.Metadata["ProcessorCount"] = Environment.ProcessorCount.ToString(CultureInfo.InvariantCulture);
            info.Metadata["Is64BitProcess"] = Environment.Is64BitProcess.ToString();
            info.Metadata["BaseDirectory"] = AppDomain.CurrentDomain.BaseDirectory;
            info.Metadata["UserName"] = Environment.UserName;
            info.Metadata["MachineName"] = Environment.MachineName;
            info.Metadata["CommandLine"] = Environment.CommandLine;
            info.Metadata["AppDomain"] = AppDomain.CurrentDomain.FriendlyName;
        }

        internal static void PopulateDynamicOsAndEnvironmentMetadata(HealthInfo info)
        {
            info.Metadata["CurrentDirectory"] = Environment.CurrentDirectory;
            info.Metadata["Culture"] = CultureInfo.CurrentCulture.Name;
            info.Metadata["UICulture"] = CultureInfo.CurrentUICulture.Name;
        }

        internal static void PopulateOsAndEnvironmentMetadata(HealthInfo info)
        {
            // Backward-compatible method: populate both static and dynamic metadata
            PopulateStaticOsAndEnvironmentMetadata(info);
            PopulateDynamicOsAndEnvironmentMetadata(info);
        }
    }

    // Helper API to run the usage-guide flow in one call
    public class HealthMonitorOptions
    {
        public bool EnableSystemCheck { get; set; } = true;
        public SystemHealthThresholds SystemThresholds { get; set; } = new SystemHealthThresholds();
        public string DriveRoot { get; set; } = Path.GetPathRoot(Environment.CurrentDirectory);

        public string SqlConnectionString { get; set; }
        public string SqlCommandText { get; set; } = "SELECT 1";
        public int SqlCommandTimeoutSeconds { get; set; } = 5;

        public IEnumerable<IHealthCheck> AdditionalChecks { get; set; } = new List<IHealthCheck>();

        public TimeSpan? PerCheckTimeout { get; set; }
        public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(10);
        public bool RunImmediately { get; set; } = true;

        // Callback to receive each snapshot
        public Action<HealthInfo> OnSnapshot { get; set; }
        // Optional error callback for handler exceptions
        public Action<Exception> OnError { get; set; }
    }

    public static class HealthMonitor
    {
        private sealed class MonitorHandle : IDisposable
        {
            private System.Timers.Timer _timer;
            private bool _disposed;
            private readonly System.Timers.ElapsedEventHandler _handler;

            public MonitorHandle(System.Timers.Timer timer, System.Timers.ElapsedEventHandler handler)
            {
                _timer = timer;
                _handler = handler;
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                try
                {
                    if (_timer != null)
                    {
                        if (_handler != null)
                        {
                            _timer.Elapsed -= _handler;
                        }
                        _timer.Stop();
                        _timer.Dispose();
                    }
                }
                finally
                {
                    _timer = null;
                }
            }
        }

        public static IDisposable Start(HealthMonitorOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (options.OnSnapshot == null) throw new ArgumentNullException(nameof(options.OnSnapshot));
            if (options.Interval <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(options.Interval));

            // normalize defaults if consumer overrode with nulls
            var thresholds = options.SystemThresholds ?? new SystemHealthThresholds();
            var driveRoot = string.IsNullOrWhiteSpace(options.DriveRoot) ? Path.GetPathRoot(Environment.CurrentDirectory) : options.DriveRoot;
            var additionalChecks = options.AdditionalChecks ?? Enumerable.Empty<IHealthCheck>();
            var sqlText = string.IsNullOrEmpty(options.SqlCommandText) ? "SELECT 1" : options.SqlCommandText;

            var checks = new List<IHealthCheck>();
            if (options.EnableSystemCheck)
            {
                checks.Add(new SystemHealthCheck(thresholds, driveRoot));
            }

            if (!string.IsNullOrWhiteSpace(options.SqlConnectionString))
            {
                checks.Add(new SqlServerHealthCheck(options.SqlConnectionString, sqlText, options.SqlCommandTimeoutSeconds > 0 ? options.SqlCommandTimeoutSeconds : 5));
            }

            checks.AddRange(additionalChecks.Where(c => c != null));

            IHealthReporter reporter = checks.Count > 0
                ? (IHealthReporter)new CompositeHealthReporter(checks, options.PerCheckTimeout)
                : new BasicHealthReporter();

            var timer = new System.Timers.Timer(options.Interval.TotalMilliseconds)
            {
                AutoReset = true,
                Enabled = false
            };

            int running = 0;
            System.Timers.ElapsedEventHandler handler = (s, e) =>
            {
                if (Interlocked.CompareExchange(ref running, 1, 0) != 0)
                {
                    return; // skip overlapping ticks
                }
                try
                {
                    var snap = reporter.GetHealth();
                    options.OnSnapshot(snap);
                }
                catch (Exception ex)
                {
                    var cb = options.OnError;
                    if (cb != null)
                    {
                        try { cb(ex); } catch { /* swallow */ }
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref running, 0);
                }
            };

            timer.Elapsed += handler;
            timer.Start();

            if (options.RunImmediately)
            {
                try
                {
                    var first = reporter.GetHealth();
                    options.OnSnapshot(first);
                }
                catch (Exception ex)
                {
                    var cb = options.OnError;
                    if (cb != null)
                    {
                        try { cb(ex); } catch { /* swallow */ }
                    }
                }
            }

            return new MonitorHandle(timer, handler);
        }
    }
}

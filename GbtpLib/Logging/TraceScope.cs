using System;
using GbtpLib.Mssql.Application.Services;
using GbtpLib.Mssql.Application.UseCases;

namespace GbtpLib.Logging
{
    public interface ITraceScope : IDisposable { }

    internal class TraceScope : ITraceScope
    {
        private readonly ILogger _logger;
        private readonly string _scopeName;
        private readonly DateTime _start;
        public TraceScope(ILogger logger, string scopeName)
        {
            _logger = logger;
            _scopeName = scopeName;
            _start = DateTime.Now;
            _logger.Trace($"Enter {_scopeName}");
        }
        public void Dispose()
        {
            var elapsed = DateTime.Now - _start;
            _logger.Trace($"Exit {_scopeName} ({elapsed.TotalMilliseconds:F0} ms)");
        }
    }

    public static class TraceScopeFactory
    {
        public static ITraceScope Begin(ILogger logger, string scopeName) => new TraceScope(logger, scopeName);
    }
}

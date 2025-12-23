using System;
using System.Collections.Concurrent;

namespace GbtpLib.Configuration
{
    public interface IAppSettings
    {
        string Get(string key, string defaultValue = null);
        void Set(string key, string value);
    }

    // Framework-agnostic settings provider (no System.Configuration dependency)
    public class InMemoryAppSettings : IAppSettings
    {
        private readonly ConcurrentDictionary<string, string> _settings = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public string Get(string key, string defaultValue = null)
        {
            if (string.IsNullOrEmpty(key)) return defaultValue;
            if (_settings.TryGetValue(key, out var v)) return v;
            var env = Environment.GetEnvironmentVariable(key);
            return string.IsNullOrEmpty(env) ? defaultValue : env;
        }

        public void Set(string key, string value)
        {
            if (string.IsNullOrEmpty(key)) return;
            _settings[key] = value;
        }
    }
}

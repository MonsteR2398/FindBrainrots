using System;
using System.Collections.Generic;

namespace Treasures.Services
{
    public interface IAnalyticsService
    {
        bool IsAvailable { get; }
        bool IsInitialized { get; }
        void Initialize(Action<bool> onInitialized);

        // Analytics
        void LogEvent(string eventName);
        void LogEvent(string eventName, IDictionary<string, object> parameters);

        // Remote Config
        bool GetBool(string key, bool defaultValue = false);
        long GetLong(string key, long defaultValue = 0);
        string GetString(string key, string defaultValue = "");
    }
}

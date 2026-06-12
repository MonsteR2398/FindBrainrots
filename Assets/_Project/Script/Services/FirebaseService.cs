using System;
using System.Collections.Generic;
using UnityEngine;

namespace Treasures.Services
{
    /// <summary>
    /// Firebase implementation of <see cref="IAnalyticsService"/>.
    /// </summary>
    public class FirebaseService : IAnalyticsService
    {
#if FIREBASE_ENABLED
        public bool IsAvailable => true;
#else
        public bool IsAvailable => false;
#endif

        public bool IsInitialized { get; private set; }

#if FIREBASE_ENABLED
        public void Initialize(Action<bool> onInitialized)
        {
           
        }

        public void LogEvent(string eventName)
        {
            
        }

        public void LogEvent(string eventName, IDictionary<string, object> parameters)
        {
           
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            return true;
        }

        public long GetLong(string key, long defaultValue = 0)
        {
            return 1;
        }

        public string GetString(string key, string defaultValue = "")
        {
            return "1";
        }
#else
        public void Initialize(Action<bool> onInitialized) => onInitialized?.Invoke(false);
        public void LogEvent(string eventName) { }
        public void LogEvent(string eventName, IDictionary<string, object> parameters) { }
        public bool GetBool(string key, bool defaultValue = false) => defaultValue;
        public long GetLong(string key, long defaultValue = 0) => defaultValue;
        public string GetString(string key, string defaultValue = "") => defaultValue;
#endif
    }
}

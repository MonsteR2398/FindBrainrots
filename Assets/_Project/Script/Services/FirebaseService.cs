using System;
using System.Collections.Generic;
using UnityEngine;

#if FIREBASE_ENABLED
using Firebase;
using Firebase.Analytics;
using Firebase.RemoteConfig;
using Firebase.Extensions;
#endif

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

        public void Initialize(Action<bool> onInitialized)
        {
#if FIREBASE_ENABLED
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    // Initialize Analytics
                    FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                    
                    // Initialize Remote Config with default values or just fetch
                    FetchRemoteConfig(onInitialized);
                }
                else
                {
                    Debug.LogError($"[Firebase] Could not resolve dependencies: {dependencyStatus}");
                    IsInitialized = true; // Mark as initialized but failed
                    onInitialized?.Invoke(false);
                }
            });
#else
            IsInitialized = true;
            onInitialized?.Invoke(false);
#endif
        }

#if FIREBASE_ENABLED
        private void FetchRemoteConfig(Action<bool> onInitialized)
        {
            var defaults = new Dictionary<string, object>
            {
                { "banner_enabled", true },
                { "initial_ad_delay_sec", 180L },
                { "interstitial_interval_sec", 90L }
            };

            FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults).ContinueWithOnMainThread(setDefaultsTask =>
            {
                FirebaseRemoteConfig.DefaultInstance.FetchAndActivateAsync().ContinueWithOnMainThread(task =>
                {
                    if (task.IsFaulted || task.IsCanceled)
                    {
                        Debug.LogWarning("[Firebase] Remote Config fetch failed or timed out.");
                    }
                    else
                    {
                        Debug.Log("[Firebase] Remote Config fetched and activated.");
                    }

                    IsInitialized = true;
                    onInitialized?.Invoke(true);
                });
            });
        }

        public void LogEvent(string eventName)
        {
            if (!IsInitialized) return;
            FirebaseAnalytics.LogEvent(eventName);
        }

        public void LogEvent(string eventName, IDictionary<string, object> parameters)
        {
            if (!IsInitialized) return;

            if (parameters == null)
            {
                FirebaseAnalytics.LogEvent(eventName);
                return;
            }

            var firebaseParams = new Parameter[parameters.Count];
            int i = 0;
            foreach (var kvp in parameters)
            {
                if (kvp.Value is string s) firebaseParams[i] = new Parameter(kvp.Key, s);
                else if (kvp.Value is long l) firebaseParams[i] = new Parameter(kvp.Key, l);
                else if (kvp.Value is int intVal) firebaseParams[i] = new Parameter(kvp.Key, intVal);
                else if (kvp.Value is double d) firebaseParams[i] = new Parameter(kvp.Key, d);
                else if (kvp.Value is float f) firebaseParams[i] = new Parameter(kvp.Key, f);
                else firebaseParams[i] = new Parameter(kvp.Key, kvp.Value?.ToString() ?? "");
                i++;
            }

            FirebaseAnalytics.LogEvent(eventName, firebaseParams);
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            if (!IsInitialized) return defaultValue;
            var val = FirebaseRemoteConfig.DefaultInstance.GetValue(key);
            return val.Source != ValueSource.StaticValue ? val.BooleanValue : defaultValue;
        }

        public long GetLong(string key, long defaultValue = 0)
        {
            if (!IsInitialized) return defaultValue;
            var val = FirebaseRemoteConfig.DefaultInstance.GetValue(key);
            return val.Source != ValueSource.StaticValue ? val.LongValue : defaultValue;
        }

        public string GetString(string key, string defaultValue = "")
        {
            if (!IsInitialized) return defaultValue;
            var val = FirebaseRemoteConfig.DefaultInstance.GetValue(key);
            return val.Source != ValueSource.StaticValue ? val.StringValue : defaultValue;
        }
#else
        public void LogEvent(string eventName) { }
        public void LogEvent(string eventName, IDictionary<string, object> parameters) { }
        public bool GetBool(string key, bool defaultValue = false) => defaultValue;
        public long GetLong(string key, long defaultValue = 0) => defaultValue;
        public string GetString(string key, string defaultValue = "") => defaultValue;
#endif
    }
}

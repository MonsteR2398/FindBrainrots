using System;
using System.Collections.Generic;
using UnityEngine;
#if FIREBASE_ENABLED
using Firebase;
using Firebase.Extensions;
using Firebase.RemoteConfig;
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

#if FIREBASE_ENABLED
        public void Initialize(Action<bool> onInitialized)
        {
            if (IsInitialized)
            {
                onInitialized?.Invoke(true);
                return;
            }

            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("[Firebase] Dependency check failed: " + task.Exception);
                    IsInitialized = false;
                    onInitialized?.Invoke(false);
                    return;
                }

                var status = task.Result;
                if (status != DependencyStatus.Available)
                {
                    Debug.LogError("[Firebase] Dependencies not available: " + status);
                    IsInitialized = false;
                    onInitialized?.Invoke(false);
                    return;
                }

                try
                {
                    var _ = FirebaseApp.DefaultInstance;

                    Firebase.Analytics.FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                    Firebase.Crashlytics.Crashlytics.IsCrashlyticsCollectionEnabled = true;

                    // Set default Remote Config values
                    var defaults = new Dictionary<string, object>
                    {
                        { "banner_enabled", true },
                        { "interstitial_interval_sec", 60L }
                    };
                    FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults);

                    IsInitialized = true;
                    Debug.Log("[Firebase] Initialized.");

                    FirebaseRemoteConfig.DefaultInstance
                        .FetchAndActivateAsync()
                        .ContinueWithOnMainThread(rcTask =>
                        {
                            if (rcTask.IsFaulted || rcTask.IsCanceled)
                                Debug.LogWarning("[Firebase] Remote Config fetch failed.");
                            else
                                Debug.Log("[Firebase] Remote Config ready: " + rcTask.Result);
                        });

                    onInitialized?.Invoke(true);
                }
                catch (Exception e)
                {
                    Debug.LogError("[Firebase] Initialization error: " + e);
                    IsInitialized = false;
                    onInitialized?.Invoke(false);
                }
            });
        }

        public void LogEvent(string eventName)
        {
            if (!IsInitialized) return;
            Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName);
        }

        public void LogEvent(string eventName, IDictionary<string, object> parameters)
        {
            if (!IsInitialized) return;
            if (parameters == null || parameters.Count == 0)
            {
                Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName);
                return;
            }

            var list = new List<Firebase.Analytics.Parameter>(parameters.Count);
            foreach (var kv in parameters)
            {
                switch (kv.Value)
                {
                    case null:
                        list.Add(new Firebase.Analytics.Parameter(kv.Key, string.Empty));
                        break;
                    case int i:
                        list.Add(new Firebase.Analytics.Parameter(kv.Key, (long)i));
                        break;
                    case long l:
                        list.Add(new Firebase.Analytics.Parameter(kv.Key, l));
                        break;
                    case float f:
                        list.Add(new Firebase.Analytics.Parameter(kv.Key, (double)f));
                        break;
                    case double d:
                        list.Add(new Firebase.Analytics.Parameter(kv.Key, d));
                        break;
                    default:
                        list.Add(new Firebase.Analytics.Parameter(kv.Key, kv.Value.ToString()));
                        break;
                }
            }

            Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName, list.ToArray());
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            if (!IsInitialized) return defaultValue;
            return FirebaseRemoteConfig.DefaultInstance.GetValue(key).BooleanValue;
        }

        public long GetLong(string key, long defaultValue = 0)
        {
            if (!IsInitialized) return defaultValue;
            return FirebaseRemoteConfig.DefaultInstance.GetValue(key).LongValue;
        }

        public string GetString(string key, string defaultValue = "")
        {
            if (!IsInitialized) return defaultValue;
            return FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue;
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

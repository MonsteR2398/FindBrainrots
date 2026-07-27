using System.Collections.Generic;
using UnityEngine;

namespace Treasures.WorldSystem
{
    /// <summary>
    /// Central registry for UI windows that can be opened by UITrigger.
    /// UI components register themselves on Awake/Start and can be found by name.
    /// </summary>
    public static class UIRegistry
    {
        private static readonly Dictionary<string, IUIOpenable> _uiWindows = new Dictionary<string, IUIOpenable>();
        private static readonly Dictionary<IUIOpenable, string> _reverseLookup = new Dictionary<IUIOpenable, string>();

        public static void Register(string name, IUIOpenable uiComponent)
        {
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogWarning("[UIRegistry] Cannot register UI with null or empty name.");
                return;
            }

            if (uiComponent == null)
            {
                Debug.LogWarning("[UIRegistry] Cannot register null UI component.");
                return;
            }

            // If already registered, update the entry
            if (_uiWindows.ContainsKey(name))
            {
                Debug.LogWarning($"[UIRegistry] UI window '{name}' is already registered. Updating reference.");
                _uiWindows[name] = uiComponent;
            }
            else
            {
                _uiWindows.Add(name, uiComponent);
                _reverseLookup.Add(uiComponent, name);
            }
        }

        /// <summary>
        /// Unregister a UI window by name.
        /// </summary>
        public static void Unregister(string name)
        {
            if (_uiWindows.TryGetValue(name, out IUIOpenable uiComponent))
            {
                _uiWindows.Remove(name);
                _reverseLookup.Remove(uiComponent);
            }
        }

        /// <summary>
        /// Unregister a UI window by component reference.
        /// </summary>
        public static void Unregister(IUIOpenable uiComponent)
        {
            if (_reverseLookup.TryGetValue(uiComponent, out string name))
            {
                _uiWindows.Remove(name);
                _reverseLookup.Remove(uiComponent);
            }
        }

        /// <summary>
        /// Get a UI window by its registered name.
        /// </summary>
        public static IUIOpenable Get(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogWarning("[UIRegistry] Cannot get UI with null or empty name.");
                return null;
            }

            _uiWindows.TryGetValue(name, out IUIOpenable uiComponent);
            return uiComponent;
        }

        /// <summary>
        /// Check if a UI window is registered.
        /// </summary>
        public static bool IsRegistered(string name)
        {
            return !string.IsNullOrEmpty(name) && _uiWindows.ContainsKey(name);
        }

        /// <summary>
        /// Clear all registered UI windows (useful for testing).
        /// </summary>
        public static void Clear()
        {
            _uiWindows.Clear();
            _reverseLookup.Clear();
        }
    }
}
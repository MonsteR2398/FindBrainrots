using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Treasures.Services.EditorTools
{
    /// <summary>
    /// Keeps the <c>FIREBASE_ENABLED</c> scripting define in sync with the actual presence of the
    /// Firebase SDK in the project. The Firebase SDK is git-ignored, so after a fresh clone the
    /// folder may be missing — this removes the define automatically so the project still compiles.
    /// When Firebase is re-imported, the define is added back.
    ///
    /// Runs automatically on every editor load and asset import.
    /// </summary>
    [InitializeOnLoad]
    public static class FirebaseDefineSetup
    {
        private const string Define = "FIREBASE_ENABLED";

        private static readonly NamedBuildTarget[] Targets =
        {
            NamedBuildTarget.Android,
            NamedBuildTarget.Standalone,
            NamedBuildTarget.iOS,
        };

        static FirebaseDefineSetup()
        {
            // Defer to avoid running during a domain reload import step.
            EditorApplication.delayCall += Sync;
        }

        [MenuItem("Tools/Services/Resync Firebase Define")]
        public static void Sync()
        {
            bool present = IsFirebasePresent();
            foreach (var target in Targets)
                SetDefine(target, present);
        }

        private static bool IsFirebasePresent()
        {
            string fbDir = Path.Combine(Application.dataPath, "Firebase");
            if (!Directory.Exists(fbDir)) return false;

            // The managed editor assembly is the most reliable marker of a usable Firebase SDK.
            return Directory.GetFiles(fbDir, "Firebase.App.dll", SearchOption.AllDirectories).Length > 0;
        }

        private static void SetDefine(NamedBuildTarget target, bool enabled)
        {
            string current;
            try { current = PlayerSettings.GetScriptingDefineSymbols(target); }
            catch { return; }

            var defines = current
                .Split(';')
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .ToList();

            bool has = defines.Contains(Define);

            if (enabled && !has)
            {
                defines.Add(Define);
            }
            else if (!enabled && has)
            {
                defines.RemoveAll(d => d == Define);
            }
            else
            {
                return; // already in the desired state
            }

            PlayerSettings.SetScriptingDefineSymbols(target, string.Join(";", defines));
            Debug.Log($"[FirebaseDefineSetup] {target.TargetName}: FIREBASE_ENABLED {(enabled ? "added" : "removed")}.");
        }
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Treasures.WorldSystem
{
    /// <summary>
    /// Lives in the persistent core scene. Loads/unloads world scenes additively,
    /// places the player at the world's spawn point and applies the mode's rules.
    /// </summary>
    public class GameModeManager : MonoBehaviour
    {
        public static GameModeManager Instance { get; private set; }

        [Header("Config")]
        [SerializeField] private GameModeCatalog catalog;
        [Tooltip("World loaded automatically when the game starts. Defaults to the first catalog entry.")]
        [SerializeField] private GameModeDefinition startMode;
        [SerializeField] private bool loadStartModeOnPlay = true;

        public GameModeDefinition CurrentMode { get; private set; }
        public bool IsSwitching { get; private set; }
        public GameModeCatalog Catalog => catalog;

        public event System.Action<GameModeDefinition> SwitchStarted;
        public event System.Action<GameModeDefinition> WorldChanged;

        private Scene _worldScene;
        private PlayerController _player;
        private GameModeContext _ctx;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            _player = FindFirstObjectByType<PlayerController>();

            if (loadStartModeOnPlay)
            {
                GameModeDefinition first = startMode != null
                    ? startMode
                    : (catalog != null && catalog.Modes != null && catalog.Modes.Length > 0 ? catalog.Modes[0] : null);

                if (first != null) SwitchTo(first);
                else Debug.LogWarning("[GameModeManager] No start mode and empty catalog - nothing to load.");
            }
        }

        private void Update()
        {
            if (!IsSwitching && _ctx != null && CurrentMode != null && CurrentMode.Rules != null)
                CurrentMode.Rules.Tick(_ctx, Time.deltaTime);
        }

        public void SwitchTo(GameModeDefinition target)
        {
            if (target == null) { Debug.LogWarning("[GameModeManager] SwitchTo called with null."); return; }
            if (IsSwitching) { Debug.LogWarning("[GameModeManager] Already switching - ignored."); return; }
            if (CurrentMode == target && _worldScene.IsValid() && _worldScene.isLoaded) return;
            StartCoroutine(SwitchRoutine(target));
        }

        private IEnumerator SwitchRoutine(GameModeDefinition target)
        {
            IsSwitching = true;
            SwitchStarted?.Invoke(target);

            // Exit current rules.
            if (CurrentMode != null && CurrentMode.Rules != null && _ctx != null)
                CurrentMode.Rules.OnExit(_ctx);

            // Unload current world.
            if (_worldScene.IsValid() && _worldScene.isLoaded)
            {
                AsyncOperation unload = SceneManager.UnloadSceneAsync(_worldScene);
                while (unload != null && !unload.isDone) yield return null;
            }

            if (string.IsNullOrEmpty(target.SceneName))
            {
                Debug.LogError($"[GameModeManager] '{target.name}' has no SceneName set.");
                IsSwitching = false;
                yield break;
            }

            // Adopt an already-loaded scene (e.g. left open in the editor) instead of duplicating it.
            Scene existing = SceneManager.GetSceneByName(target.SceneName);
            if (existing.IsValid() && existing.isLoaded)
            {
                _worldScene = existing;
            }
            else
            {
                AsyncOperation load = SceneManager.LoadSceneAsync(target.SceneName, LoadSceneMode.Additive);
                while (load != null && !load.isDone) yield return null;
                _worldScene = SceneManager.GetSceneByName(target.SceneName);
            }

            // Place the player.
            if (_player == null) _player = FindFirstObjectByType<PlayerController>();
            Transform spawn = FindSpawn(_worldScene);
            if (_player != null)
            {
                if (spawn != null) _player.Teleport(spawn.position, spawn.rotation);
                else Debug.LogWarning($"[GameModeManager] No WorldSpawnPoint found in '{target.SceneName}'.");
            }

            CurrentMode = target;
            _ctx = new GameModeContext(_player, _worldScene, target);
            if (target.Rules != null) target.Rules.OnEnter(_ctx);

            WorldChanged?.Invoke(target);
            IsSwitching = false;
        }

        private static Transform FindSpawn(Scene scene)
        {
            if (!scene.IsValid()) return null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                WorldSpawnPoint sp = root.GetComponentInChildren<WorldSpawnPoint>(true);
                if (sp != null) return sp.transform;
            }
            return null;
        }
    }
}

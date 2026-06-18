using System.Collections.Generic;
using UnityEngine;
using Treasures.WorldSystem;

namespace Treasures.Bots
{
    /// <summary>
    /// Lives in the persistent core scene. When a world loads (GameModeManager.WorldChanged),
    /// spawns parkour bots on the world's ParkourGraph and despawns them when the world changes.
    /// </summary>
    public class BotSpawner : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private GameObject[] botPrefabs;
        [SerializeField, Min(0)] private int botCount = 3;
        [Tooltip("Vertical offset above a graph node when spawning, so the bot settles onto the platform.")]
        [SerializeField] private float spawnHeightOffset = 0.5f;

        [SerializeField] private ParkourGraph currentGraph;

        private readonly List<GameObject> _bots = new List<GameObject>();

        private void OnEnable()
        {
            var mgr = GameModeManager.Instance;
            if (mgr != null)
            {
                mgr.WorldChanged += HandleWorldChanged;
                if (mgr.CurrentMode != null) HandleWorldChanged(mgr.CurrentMode);
            }
        }

        private void Start()
        {
            var mgr = GameModeManager.Instance;
            if (mgr != null)
            {
                mgr.WorldChanged -= HandleWorldChanged;
                mgr.WorldChanged += HandleWorldChanged;
            }
        }

        private void OnDisable()
        {
            var mgr = GameModeManager.Instance;
            if (mgr != null) mgr.WorldChanged -= HandleWorldChanged;
        }

        private void HandleWorldChanged(GameModeDefinition def)
        {
            DespawnAll();
            SpawnBots();
        }

        private void SpawnBots()
        {
            if (botPrefabs == null)
            {
                Debug.LogWarning("[BotSpawner] No bot prefab assigned.");
                return;
            }

            if (currentGraph == null)
            {
                Debug.LogWarning("[BotSpawner] No ParkourGraph in the loaded world - no bots spawned.");
                return;
            }

            IReadOnlyList<ParkourNode> nodes = currentGraph.Nodes;
            if (nodes == null || nodes.Count == 0)
            {
                Debug.LogWarning("[BotSpawner] ParkourGraph has no nodes - no bots spawned.");
                return;
            }

            for (int i = 0; i < botCount; i++)
            {
                ParkourNode node = currentGraph.RandomNode();
                if (node == null) continue;

                Vector3 pos = node.Position + Vector3.up * spawnHeightOffset;
                GameObject bot = Instantiate(botPrefabs[Random.Range(0, botPrefabs.Length)], pos, Quaternion.identity);
                bot.name = $"Bot_{i}";

                var brain = bot.GetComponent<BotBrain>();
                if (brain != null) brain.Initialize(currentGraph);

                _bots.Add(bot);
            }

            Debug.Log($"[BotSpawner] Spawned {_bots.Count} bots.");
        }

        private void DespawnAll()
        {
            for (int i = 0; i < _bots.Count; i++)
                if (_bots[i] != null) Destroy(_bots[i]);
            _bots.Clear();
        }
    }
}

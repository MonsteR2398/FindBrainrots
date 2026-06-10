using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Treasures.Bots.Editor
{
    [CustomEditor(typeof(ParkourGraph))]
    public class ParkourGraphEditor : UnityEditor.Editor
    {
        private float _autoConnectRadius = 12f;
        private float _jumpHeightThreshold = 0.75f;
        private float _jumpGapThreshold = 2.5f;
        private float _doubleJumpDistance = 7f;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var graph = (ParkourGraph)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Authoring Tools", EditorStyles.boldLabel);

            if (GUILayout.Button("Add Node (at graph center)"))
                AddNode(graph, graph.transform.position + Vector3.up * 0.5f);

            EditorGUILayout.HelpBox(
                "Select 2+ ParkourNodes in the hierarchy, then use the buttons below. " +
                "Connections are bidirectional.", MessageType.Info);

            using (new EditorGUI.DisabledScope(SelectedNodes().Count < 2))
            {
                if (GUILayout.Button("Connect Selected (walk)"))
                    ConnectSelected(false);
                if (GUILayout.Button("Connect Selected (jump)"))
                    ConnectSelected(true);
            }
            using (new EditorGUI.DisabledScope(SelectedNodes().Count < 1))
            {
                if (GUILayout.Button("Disconnect Selected (remove all their links)"))
                    DisconnectSelected();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Auto-Connect", EditorStyles.boldLabel);
            _autoConnectRadius = EditorGUILayout.FloatField("Radius", _autoConnectRadius);
            _jumpHeightThreshold = EditorGUILayout.FloatField("Jump if height diff >", _jumpHeightThreshold);
            _jumpGapThreshold = EditorGUILayout.FloatField("Jump if XZ gap >", _jumpGapThreshold);
            _doubleJumpDistance = EditorGUILayout.FloatField("Double-jump if dist >", _doubleJumpDistance);

            if (GUILayout.Button("Auto-Connect All Nodes By Radius"))
                AutoConnect(graph);

            if (GUILayout.Button("Clear All Links"))
                ClearAllLinks(graph);
        }

        private static List<ParkourNode> SelectedNodes()
        {
            var result = new List<ParkourNode>();
            foreach (GameObject go in Selection.gameObjects)
            {
                var node = go.GetComponent<ParkourNode>();
                if (node != null) result.Add(node);
            }
            return result;
        }

        private void AddNode(ParkourGraph graph, Vector3 position)
        {
            var go = new GameObject("Node_" + graph.transform.childCount);
            Undo.RegisterCreatedObjectUndo(go, "Add Parkour Node");
            go.transform.SetParent(graph.transform);
            go.transform.position = position;
            go.AddComponent<ParkourNode>();
            graph.Collect();
            Selection.activeGameObject = go;
        }

        private void ConnectSelected(bool asJump)
        {
            List<ParkourNode> nodes = SelectedNodes();
            for (int i = 0; i < nodes.Count; i++)
            for (int j = 0; j < nodes.Count; j++)
            {
                if (i == j) continue;
                AddOrUpdateLink(nodes[i], nodes[j], asJump,
                    asJump && Vector3.Distance(nodes[i].Position, nodes[j].Position) > _doubleJumpDistance);
            }
        }

        private void DisconnectSelected()
        {
            foreach (ParkourNode node in SelectedNodes())
            {
                Undo.RecordObject(node, "Disconnect Node");
                node.links.Clear();
                EditorUtility.SetDirty(node);
            }
        }

        private void AutoConnect(ParkourGraph graph)
        {
            graph.Collect();
            IReadOnlyList<ParkourNode> nodes = graph.Nodes;
            int count = 0;
            for (int i = 0; i < nodes.Count; i++)
            for (int j = 0; j < nodes.Count; j++)
            {
                if (i == j) continue;
                ParkourNode a = nodes[i];
                ParkourNode b = nodes[j];
                float dist = Vector3.Distance(a.Position, b.Position);
                if (dist > _autoConnectRadius) continue;

                float heightDiff = Mathf.Abs(a.Position.y - b.Position.y);
                Vector3 fa = a.Position; fa.y = 0;
                Vector3 fb = b.Position; fb.y = 0;
                float gap = Vector3.Distance(fa, fb);

                bool isJump = heightDiff > _jumpHeightThreshold || gap > _jumpGapThreshold;
                bool doubleJump = isJump && dist > _doubleJumpDistance;
                AddOrUpdateLink(a, b, isJump, doubleJump);
                count++;
            }
            Debug.Log($"[ParkourGraphEditor] Auto-connected {count} directed edges across {nodes.Count} nodes.");
        }

        private void ClearAllLinks(ParkourGraph graph)
        {
            graph.Collect();
            foreach (ParkourNode node in graph.Nodes)
            {
                if (node == null) continue;
                Undo.RecordObject(node, "Clear Links");
                node.links.Clear();
                EditorUtility.SetDirty(node);
            }
        }

        private static void AddOrUpdateLink(ParkourNode from, ParkourNode to, bool isJump, bool doubleJump)
        {
            if (from == null || to == null || from == to) return;
            Undo.RecordObject(from, "Add Parkour Link");
            if (from.TryGetLinkTo(to, out ParkourLink existing))
            {
                existing.isJump = isJump;
                existing.requiresDoubleJump = doubleJump;
            }
            else
            {
                from.links.Add(new ParkourLink { target = to, isJump = isJump, requiresDoubleJump = doubleJump });
            }
            EditorUtility.SetDirty(from);
        }
    }
}

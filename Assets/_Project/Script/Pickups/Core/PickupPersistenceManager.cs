using System.Collections.Generic;
using UnityEngine;
using PlayerPrefs = RedefineYG.PlayerPrefs;

namespace Treasures.Pickups
{
    public class PickupPersistenceManager : MonoBehaviour
    {
        private static PickupPersistenceManager _instance;
        public static PickupPersistenceManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("PickupPersistenceManager");
                    _instance = go.AddComponent<PickupPersistenceManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private const string SaveKey = "CollectedPickups";
        private HashSet<string> _collectedIds = new HashSet<string>();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }

        public bool IsCollected(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            return _collectedIds.Contains(id);
        }

        public void MarkAsCollected(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            if (_collectedIds.Add(id))
            {
                Save();
            }
        }

        private void Save()
        {
            string data = string.Join(",", _collectedIds);
            PlayerPrefs.SetString(SaveKey, data);
            PlayerPrefs.Save();
        }

        private void Load()
        {
            string data = PlayerPrefs.GetString(SaveKey, "");
            if (!string.IsNullOrEmpty(data))
            {
                string[] ids = data.Split(',');
                _collectedIds = new HashSet<string>(ids);
            }
        }

        [ContextMenu("Clear Persistence")]
        public void ClearData()
        {
            _collectedIds.Clear();
            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.Save();
            Debug.Log("Pickup Persistence Cleared");
        }
    }
}

using UnityEngine;

namespace ModularTreasures.Quests
{
    [CreateAssetMenu(fileName = "New Location Condition", menuName = "Quests/Conditions/Location Condition")]
    public class LocationConditionSO : QuestConditionSO
    {
        [SerializeField] private Vector3 targetPosition;
        [SerializeField] private float reachDistance = 2f;
        [SerializeField] private string playerTag = "Player";

        private Transform _playerTransform;

        protected override void OnEnableCondition()
        {
            GameObject player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null)
                _playerTransform = player.transform;
        }

        protected override void OnDisableCondition()
        {
            _playerTransform = null;
        }

        public void Update()
        {
            if (_playerTransform != null)
            {
                float dist = Vector3.Distance(_playerTransform.position, targetPosition);
                if (dist <= reachDistance)
                    AddProgress(Manager.CurrentQuest.TargetValue);
            }
        }
    }
}

using UnityEngine;

namespace ModularTreasures.Quests
{
    [CreateAssetMenu(fileName = "HUD Disable Reward", menuName = "Quests/Rewards/HUD Disable")]
    public class HUDDisableRewardSO : QuestRewardSO
    {
        public override void GiveReward(Transform targetPos)
        {
            var hud = Object.FindAnyObjectByType<BrainrotTaskHUD>();
            if (hud != null)
            {
                hud.DisableHUD();
            }
        }
    }
}
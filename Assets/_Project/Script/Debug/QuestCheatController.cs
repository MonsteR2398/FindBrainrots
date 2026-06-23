using UnityEngine;
using UnityEngine.InputSystem;
using ModularTreasures.Quests;

public class QuestCheatController : MonoBehaviour
{
    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            CompleteCurrentQuest();
        }
    }

    private void CompleteCurrentQuest()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("[QuestCheat] QuestManager instance not found!");
            return;
        }

        var currentQuest = QuestManager.Instance.CurrentQuest;
        if (currentQuest == null)
        {
            Debug.LogWarning("[QuestCheat] No active quest to complete!");
            return;
        }

        Debug.Log($"[QuestCheat] Completing quest: {currentQuest.Title}");
        
        // Add enough progress to complete it
        float remaining = currentQuest.TargetValue - QuestManager.Instance.CurrentProgress;
        if (remaining <= 0) remaining = 1f; // Just in case it's already "complete" but not claimed
        
        QuestManager.Instance.AddProgress(remaining + 1000f);
    }
}
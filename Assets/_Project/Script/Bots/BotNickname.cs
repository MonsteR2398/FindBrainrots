using TMPro;
using UnityEngine;
using Treasures.Localization;

namespace Treasures.Bots
{
    /// <summary>
    /// Assigns a random localized nickname to a bot from the bot.name.N range.
    /// </summary>
    public class BotNickname : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private LocalizedText localizedText;

        private void Start()
        {
            if (nameText == null) nameText = GetComponentInChildren<TMP_Text>();
            if (localizedText == null) localizedText = GetComponentInChildren<LocalizedText>();

            int randomIndex = Random.Range(0, 100);
            string key = "bot.name." + randomIndex;

            if (localizedText != null)
            {
                localizedText.SetKey(key);
            }
            else if (nameText != null)
            {
                nameText.text = Treasures.Localization.Localization.Get(key);
            }
        }
    }
}

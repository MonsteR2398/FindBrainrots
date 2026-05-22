using UnityEngine;

namespace Treasures.CurrencySystem
{
    [CreateAssetMenu(fileName = "New Currency", menuName = "Treasures/Currency/Definition")]
    public class CurrencyDefinition : ScriptableObject
    {
        [SerializeField] private CurrencyType type;
        [SerializeField] private Sprite icon;

        public CurrencyType Type => type;
        public Sprite Icon => icon;

#if UNITY_EDITOR
        public void Initialize(CurrencyType type, Sprite icon = null)
        {
            this.type = type;
            this.icon = icon;
        }
#endif
    }
}

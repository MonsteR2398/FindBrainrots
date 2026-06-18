using System.Collections.Generic;
using UnityEngine;

namespace Treasures.IAP
{
    [CreateAssetMenu(fileName = "OfferCatalog", menuName = "Treasures/IAP/OfferCatalog")]
    public class OfferCatalogSO : ScriptableObject
    {
        [SerializeField] private List<OfferSO> offers = new List<OfferSO>();

        public List<OfferSO> Offers => offers;
    }
}
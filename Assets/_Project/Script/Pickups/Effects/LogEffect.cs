using UnityEngine;

namespace Treasures.Pickups
{
    [CreateAssetMenu(fileName = "New Log Effect", menuName = "Treasures/Pickups/Effects/Log")]
    public class LogEffect : BasePickupEffect
    {
        [SerializeField] private string message = "Item picked up!";

        public override void Apply(GameObject picker)
        {
            Debug.Log($"[LogEffect] {message} (Picked up by: {picker.name})");
        }
    }
}

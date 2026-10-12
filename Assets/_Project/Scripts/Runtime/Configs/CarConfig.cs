using UnityEngine;

namespace RoadTurret.Configs
{
    [CreateAssetMenu(fileName = "CarConfig", menuName = "RoadTurret/Configs/Car")]
    public sealed class CarConfig : ScriptableObject
    {
        [field: SerializeField, Min(1f)]
        public float MaxHealth { get; private set; } = 100f;

        [field: SerializeField, Min(0.1f), Tooltip("Meters per second.")]
        public float CruiseSpeed { get; private set; } = 8f;

        [field: SerializeField, Min(0.1f), Tooltip("Meters per second squared.")]
        public float Acceleration { get; private set; } = 5f;

        [field: SerializeField, Min(0.1f), Tooltip("Meters per second squared.")]
        public float BrakeDeceleration { get; private set; } = 9f;
    }
}

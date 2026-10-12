using UnityEngine;

namespace RoadTurret.Configs
{
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "RoadTurret/Configs/Enemy")]
    public sealed class EnemyConfig : ScriptableObject
    {
        [field: Header("Stats")]
        [field: SerializeField, Min(1f)]
        public float MaxHealth { get; private set; } = 15f;

        [field: SerializeField, Min(0.1f)]
        public float RunSpeed { get; private set; } = 5.5f;

        [field: SerializeField, Min(1f), Tooltip("Degrees per second.")]
        public float TurnSpeed { get; private set; } = 720f;

        [field: Header("Behaviour")]
        [field: SerializeField, Min(1f), Tooltip("Distance to the car at which an idle enemy starts running.")]
        public float AggroRadius { get; private set; } = 20f;

        [field: SerializeField, Min(0.1f), Tooltip("Distance to the car body at which the enemy crashes into it.")]
        public float AttackRange { get; private set; } = 0.45f;

        [field: SerializeField, Min(0f), Tooltip("Damage dealt to the car when the enemy crashes into it.")]
        public float ImpactDamage { get; private set; } = 6f;

        [field: SerializeField, Min(0f), Tooltip("Speed at which the smashed enemy is thrown away from the car.")]
        public float ImpactKnockback { get; private set; } = 9f;

        [field: SerializeField, Min(1f), Tooltip("Enemies left this far behind the car are returned to the pool.")]
        public float DespawnDistanceBehind { get; private set; } = 25f;

        [field: Header("Feedback")]
        [field: SerializeField, Min(0f)]
        public float KnockbackSpeed { get; private set; } = 5f;

        [field: SerializeField, Min(0f)]
        public float KnockbackDamping { get; private set; } = 5f;

        [field: SerializeField, Min(0f)]
        public float CorpseDuration { get; private set; } = 1.6f;

        [field: SerializeField, Min(0f)]
        public float CorpseSinkDuration { get; private set; } = 0.6f;
    }
}

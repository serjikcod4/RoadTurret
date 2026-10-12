using UnityEngine;

namespace RoadTurret.Configs
{
    [CreateAssetMenu(fileName = "TurretConfig", menuName = "RoadTurret/Configs/Turret")]
    public sealed class TurretConfig : ScriptableObject
    {
        [field: Header("Aiming")]
        [field: SerializeField, Range(10f, 180f), Tooltip("Max turret angle to each side of the car's forward direction.")]
        public float MaxYaw { get; private set; } = 60f;

        [field: SerializeField, Min(0f)]
        public float AimSmoothTime { get; private set; } = 0.05f;

        [field: Header("Firing")]
        [field: SerializeField, Min(0.1f), Tooltip("Shots per second.")]
        public float FireRate { get; private set; } = 2.4f;

        [field: SerializeField, Min(0f), Tooltip("Random horizontal spread in degrees.")]
        public float Spread { get; private set; } = 1.5f;

        [field: SerializeField, Min(0f)]
        public float RecoilDistance { get; private set; } = 0.12f;

        [field: SerializeField, Min(0.1f)]
        public float RecoilRecovery { get; private set; } = 18f;

        [field: Header("Bullet")]
        [field: SerializeField, Min(1f)]
        public float BulletSpeed { get; private set; } = 40f;

        [field: SerializeField, Min(0f), Tooltip("Each shot deals a random damage in this range.")]
        public Vector2Int DamageRange { get; private set; } = new(10, 20);

        [field: SerializeField, Min(1f)]
        public float BulletRange { get; private set; } = 50f;

        [field: SerializeField, Min(0.01f)]
        public float BulletRadius { get; private set; } = 0.45f;
    }
}

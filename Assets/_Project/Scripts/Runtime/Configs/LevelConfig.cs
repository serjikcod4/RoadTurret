using UnityEngine;

namespace RoadTurret.Configs
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "RoadTurret/Configs/Level")]
    public sealed class LevelConfig : ScriptableObject
    {
        [field: SerializeField, Min(10f), Tooltip("Distance from the start to the finish line in meters.")]
        public float Length { get; private set; } = 260f;

        [field: Header("Enemies")]
        [field: SerializeField, Min(0)]
        public int EnemyCount { get; private set; } = 30;

        [field: SerializeField, Min(0f), Tooltip("No enemies are placed closer than this to the start.")]
        public float SafeStartDistance { get; private set; } = 30f;

        [field: SerializeField, Min(0f), Tooltip("No enemies are placed closer than this to the finish.")]
        public float SafeFinishDistance { get; private set; } = 10f;

        [field: SerializeField, Min(1f), Tooltip("Enemies are placed across the road, within this distance from its center.")]
        public float SpawnHalfWidth { get; private set; } = 7f;

        [field: SerializeField, Min(0f)]
        public float MinEnemySpacing { get; private set; } = 3f;
    }
}

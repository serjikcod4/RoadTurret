using RoadTurret.Configs;
using UnityEngine;
using VContainer;

namespace RoadTurret.Level
{
    /// <summary>
    /// Straight track along the local Z axis: builds the ground for the configured length and places the finish line.
    /// </summary>
    public sealed class Track : MonoBehaviour
    {
        [SerializeField] private Transform _startPoint;
        [SerializeField] private Transform _finishLine;
        [SerializeField] private Transform _groundRoot;
        [SerializeField] private GameObject _groundTilePrefab;
        [SerializeField, Min(1f)] private float _groundTileLength = 75f;
        [SerializeField, Min(0f), Tooltip("Extra ground beyond the finish so the camera never sees the edge.")]
        private float _groundMargin = 120f;

        private LevelConfig _config;

        public float Length => _config.Length;
        public Pose StartPose => new(_startPoint.position, _startPoint.rotation);

        [Inject]
        public void Construct(LevelConfig config) => _config = config;

        private void Start()
        {
            BuildGround();
            _finishLine.localPosition = new Vector3(0f, 0f, Length);
        }

        public Vector3 GetWorldPoint(float sideOffset, float distance) =>
            transform.TransformPoint(new Vector3(sideOffset, 0f, distance));

        public float GetDistance(Vector3 worldPosition) => transform.InverseTransformPoint(worldPosition).z;

        public float GetProgress(Vector3 worldPosition) => Mathf.Clamp01(GetDistance(worldPosition) / Length);

        public bool IsFinished(Vector3 worldPosition) => GetDistance(worldPosition) >= Length;

        private void BuildGround()
        {
            // The ground model is copied along the whole level, starting one tile behind the start point.
            int tileCount = Mathf.CeilToInt((Length + _groundMargin) / _groundTileLength) + 1;

            for (int i = 0; i < tileCount; i++)
            {
                GameObject tile = Instantiate(_groundTilePrefab, _groundRoot);
                tile.name = $"{_groundTilePrefab.name}_{i:00}";
                tile.transform.SetLocalPositionAndRotation(new Vector3(0f, 0f, (i - 1) * _groundTileLength), Quaternion.identity);
            }
        }
    }
}

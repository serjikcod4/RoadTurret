using System;
using System.Collections.Generic;
using RoadTurret.Common;
using RoadTurret.Configs;
using RoadTurret.Level;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RoadTurret.Enemies
{
    /// <summary>
    /// Places the level's enemies along the track and returns them to the pool when they are gone.
    /// </summary>
    public sealed class EnemySpawner
    {
        private const int PlacementAttempts = 10;

        private readonly PrefabPool<Enemy> _pool;
        private readonly LevelConfig _config;
        private readonly Track _track;
        private readonly List<Enemy> _activeEnemies = new();
        private readonly List<Vector2> _placedPoints = new();

        public EnemySpawner(PrefabPool<Enemy> pool, LevelConfig config, Track track)
        {
            _pool = pool;
            _config = config;
            _track = track;
        }

        public event Action<Enemy> EnemyKilled;

        public int SpawnedCount { get; private set; }
        public int KilledCount { get; private set; }

        public void SpawnLevel()
        {
            DespawnAll();
            SpawnedCount = 0;
            KilledCount = 0;
            _placedPoints.Clear();

            float start = _config.SafeStartDistance;
            float end = _track.Length - _config.SafeFinishDistance;
            float segment = (end - start) / Mathf.Max(1, _config.EnemyCount);

            // Stratified placement: one enemy per segment keeps the pressure even along the whole level.
            for (int i = 0; i < _config.EnemyCount; i++)
            {
                Vector2 point = FindFreePoint(start + segment * i, segment);
                _placedPoints.Add(point);
                Spawn(point);
            }
        }

        public void FreezeAll()
        {
            foreach (Enemy enemy in _activeEnemies)
                enemy.Freeze();
        }

        public void DespawnAll()
        {
            for (int i = _activeEnemies.Count - 1; i >= 0; i--)
                _activeEnemies[i].Despawn();
        }

        private Vector2 FindFreePoint(float segmentStart, float segmentLength)
        {
            Vector2 point = default;

            for (int attempt = 0; attempt < PlacementAttempts; attempt++)
            {
                point = new Vector2(
                    Random.Range(-_config.SpawnHalfWidth, _config.SpawnHalfWidth),
                    segmentStart + Random.value * segmentLength);

                if (IsFarFromOthers(point))
                    break;
            }

            return point;
        }

        private bool IsFarFromOthers(Vector2 point)
        {
            float minDistanceSqr = _config.MinEnemySpacing * _config.MinEnemySpacing;

            foreach (Vector2 placed in _placedPoints)
            {
                if ((placed - point).sqrMagnitude < minDistanceSqr)
                    return false;
            }

            return true;
        }

        private void Spawn(Vector2 trackPoint)
        {
            Enemy enemy = _pool.Get();
            enemy.Killed += OnEnemyKilled;
            enemy.Despawned += OnEnemyDespawned;
            _activeEnemies.Add(enemy);

            Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            enemy.Spawn(_track.GetWorldPoint(trackPoint.x, trackPoint.y), rotation);
            SpawnedCount++;
        }

        private void OnEnemyKilled(Enemy enemy)
        {
            KilledCount++;
            EnemyKilled?.Invoke(enemy);
        }

        private void OnEnemyDespawned(Enemy enemy)
        {
            enemy.Killed -= OnEnemyKilled;
            enemy.Despawned -= OnEnemyDespawned;
            _activeEnemies.Remove(enemy);
            _pool.Release(enemy);
        }
    }
}

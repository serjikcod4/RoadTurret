using System;
using System.Collections.Generic;
using RoadTurret.Common;
using UnityEngine;
using VContainer;

namespace RoadTurret.Effects
{
    /// <summary>
    /// Plays one-shot effects, keeping a separate pool for every effect prefab.
    /// </summary>
    public sealed class EffectsPlayer : IDisposable
    {
        private readonly IObjectResolver _resolver;
        private readonly Transform _root;
        private readonly Dictionary<PooledEffect, (PrefabPool<PooledEffect> Pool, Action<PooledEffect> Release)> _pools = new();

        public EffectsPlayer(IObjectResolver resolver, Transform root)
        {
            _resolver = resolver;
            _root = root;
        }

        public void Play(PooledEffect prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null)
                return;

            if (!_pools.TryGetValue(prefab, out var entry))
            {
                var pool = new PrefabPool<PooledEffect>(_resolver, prefab, _root);
                entry = (pool, pool.Release);
                _pools.Add(prefab, entry);
            }

            PooledEffect effect = entry.Pool.Get();
            effect.transform.SetPositionAndRotation(position, rotation);
            effect.Play(entry.Release);
        }

        public void Dispose()
        {
            foreach (var entry in _pools.Values)
                entry.Pool.Dispose();

            _pools.Clear();
        }
    }
}

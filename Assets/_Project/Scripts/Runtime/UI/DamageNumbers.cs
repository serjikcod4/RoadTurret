using System;
using RoadTurret.Common;
using UnityEngine;

namespace RoadTurret.UI
{
    /// <summary>
    /// Spawns pooled floating damage values.
    /// </summary>
    public sealed class DamageNumbers
    {
        private readonly PrefabPool<DamageNumber> _pool;
        private readonly Action<DamageNumber> _release;

        public DamageNumbers(PrefabPool<DamageNumber> pool)
        {
            _pool = pool;
            _release = pool.Release;
        }

        public void Show(Vector3 position, float amount, Color color) =>
            _pool.Get().Show(position, Mathf.RoundToInt(amount), color, _release);
    }
}

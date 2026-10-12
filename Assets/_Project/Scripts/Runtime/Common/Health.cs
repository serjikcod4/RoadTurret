using System;
using UnityEngine;

namespace RoadTurret.Common
{
    public sealed class Health
    {
        public Health(float max)
        {
            Max = max;
            Current = max;
        }

        public event Action Changed;
        public event Action Died;

        public float Max { get; }
        public float Current { get; private set; }
        public float Normalized => Current / Max;
        public bool IsDead => Current <= 0f;

        public void TakeDamage(float amount)
        {
            if (IsDead || amount <= 0f)
                return;

            Current = Mathf.Max(0f, Current - amount);
            Changed?.Invoke();

            if (IsDead)
                Died?.Invoke();
        }

        public void Restore()
        {
            Current = Max;
            Changed?.Invoke();
        }
    }
}

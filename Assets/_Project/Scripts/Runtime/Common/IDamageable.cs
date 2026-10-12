using UnityEngine;

namespace RoadTurret.Common
{
    public interface IDamageable
    {
        bool IsAlive { get; }

        void TakeDamage(float amount, Vector3 hitDirection);
    }
}

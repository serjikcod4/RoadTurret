using System;
using RoadTurret.Common;
using RoadTurret.Configs;
using RoadTurret.Effects;
using UnityEngine;
using VContainer;

namespace RoadTurret.Weapons
{
    public sealed class Bullet : MonoBehaviour
    {
        [SerializeField] private TrailRenderer _trail;
        [SerializeField] private LayerMask _hitMask;
        [SerializeField] private PooledEffect _impactEffect;

        private EffectsPlayer _effects;
        private Action<Bullet> _release;
        private Vector3 _previousPosition;
        private Vector3 _direction;
        private float _speed;
        private float _damage;
        private float _radius;
        private float _distanceLeft;

        [Inject]
        public void Construct(EffectsPlayer effects) => _effects = effects;

        private void Update()
        {
            float step = _speed * Time.deltaTime;
            Vector3 nextPosition = transform.position + _direction * step;
            Vector3 path = nextPosition - _previousPosition;

            if (Physics.SphereCast(_previousPosition, _radius, path.normalized, out RaycastHit hit, path.magnitude,
                    _hitMask, QueryTriggerInteraction.Collide))
            {
                OnHit(hit);
                return;
            }

            transform.position = nextPosition;
            _previousPosition = nextPosition;
            _distanceLeft -= step;

            if (_distanceLeft <= 0f)
                Despawn();
        }

        public void Launch(Vector3 origin, Vector3 position, Vector3 direction, float damage, TurretConfig config,
            Action<Bullet> release)
        {
            _previousPosition = origin;
            _direction = direction;
            _damage = damage;
            _speed = config.BulletSpeed;
            _radius = config.BulletRadius;
            _distanceLeft = config.BulletRange;
            _release = release;

            transform.SetPositionAndRotation(position, Quaternion.LookRotation(direction));
            _trail.Clear();
        }

        private void OnHit(RaycastHit hit)
        {
            if (hit.collider.TryGetComponent(out IDamageable target) && target.IsAlive)
                target.TakeDamage(_damage, _direction);

            _effects.Play(_impactEffect, hit.point, Quaternion.LookRotation(-_direction));
            Despawn();
        }

        private void Despawn()
        {
            Action<Bullet> release = _release;
            _release = null;
            release?.Invoke(this);
        }
    }
}

using System;
using RoadTurret.Common;
using RoadTurret.Configs;
using RoadTurret.Effects;
using RoadTurret.UI;
using UnityEngine;
using VContainer;

namespace RoadTurret.Vehicle
{
    public sealed class Car : MonoBehaviour, IDamageable
    {
        [SerializeField] private BoxCollider _body;
        [SerializeField] private Transform[] _wheels;
        [SerializeField, Min(0.01f)] private float _wheelRadius = 0.36f;
        [SerializeField] private ParticleSystem _exhaust;
        [SerializeField] private HitFlash _hitFlash;
        [SerializeField] private CarHitReaction _hitReaction;
        [SerializeField, Min(0f)] private float _destroyedKick = 1.8f;
        [SerializeField] private WorldHealthBar _healthBar;
        [SerializeField] private PooledEffect _hitEffect;
        [SerializeField] private PooledEffect _destroyedEffect;

        private CarConfig _config;
        private EffectsPlayer _effects;
        private float _speed;
        private float _targetSpeed;

        public event Action<float> Damaged;

        public Health Health { get; private set; }
        public bool IsAlive => !Health.IsDead;
        public Vector3 Position => transform.position;

        [Inject]
        public void Construct(CarConfig config, EffectsPlayer effects)
        {
            _config = config;
            _effects = effects;
            Health = new Health(config.MaxHealth);
            _healthBar.Bind(Health);
        }

        private void Update()
        {
            float rate = _targetSpeed > _speed ? _config.Acceleration : _config.BrakeDeceleration;
            _speed = Mathf.MoveTowards(_speed, _targetSpeed, rate * Time.deltaTime);

            if (_speed <= 0f)
                return;

            float distance = _speed * Time.deltaTime;
            transform.position += transform.forward * distance;
            SpinWheels(distance);
        }

        public void PlaceAt(Pose pose)
        {
            transform.SetPositionAndRotation(pose.position, pose.rotation);
            _speed = 0f;
            _targetSpeed = 0f;
            Health.Restore();
            _hitReaction.ResetPose();
            SetExhaust(true);
        }

        public void Drive() => _targetSpeed = _config.CruiseSpeed;

        public void Stop() => _targetSpeed = 0f;

        public void TakeDamage(float amount, Vector3 hitDirection)
        {
            if (!IsAlive)
                return;

            Health.TakeDamage(amount);
            Damaged?.Invoke(amount);
            _hitFlash.Play();
            _hitReaction.Play(hitDirection, IsAlive ? 1f : _destroyedKick);

            // The attacker stands on the opposite side of the hit direction: show sparks on that side of the body.
            Vector3 hitPoint = ClosestPointOnBody(transform.position - hitDirection * 10f);
            _effects.Play(_hitEffect, hitPoint, Quaternion.LookRotation(-hitDirection));

            if (!IsAlive)
            {
                _effects.Play(_destroyedEffect, _body.transform.TransformPoint(_body.center), Quaternion.identity);
                SetExhaust(false);
            }
        }

        /// <summary>Closest point on the car body box to the given world point.</summary>
        public Vector3 ClosestPointOnBody(Vector3 worldPoint)
        {
            Transform bodyTransform = _body.transform;
            Vector3 extents = _body.size * 0.5f;
            Vector3 local = bodyTransform.InverseTransformPoint(worldPoint) - _body.center;

            local.x = Mathf.Clamp(local.x, -extents.x, extents.x);
            local.y = Mathf.Clamp(local.y, -extents.y, extents.y);
            local.z = Mathf.Clamp(local.z, -extents.z, extents.z);

            return bodyTransform.TransformPoint(local + _body.center);
        }

        private void SpinWheels(float distance)
        {
            float angle = distance / _wheelRadius * Mathf.Rad2Deg;

            foreach (Transform wheel in _wheels)
                wheel.Rotate(angle, 0f, 0f, Space.Self);
        }

        private void SetExhaust(bool isRunning)
        {
            if (_exhaust == null)
                return;

            if (isRunning)
                _exhaust.Play(true);
            else
                _exhaust.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}

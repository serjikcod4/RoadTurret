using System;
using RoadTurret.Common;
using RoadTurret.Configs;
using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;

namespace RoadTurret.Weapons
{
    /// <summary>
    /// Rotates smoothly towards the requested yaw and fires automatically while armed.
    /// </summary>
    public sealed class Turret : MonoBehaviour
    {
        [SerializeField] private Transform _pivot;
        [SerializeField] private Transform _muzzle;
        [SerializeField] private Transform _recoilRoot;
        [SerializeField] private ParticleSystem _muzzleFlash;
        [SerializeField] private LineRenderer _aimLine;

        private TurretConfig _config;
        private PrefabPool<Bullet> _bullets;
        private Action<Bullet> _releaseBullet;
        private Vector3 _recoilRestPosition;
        private float _targetYaw;
        private float _currentYaw;
        private float _yawVelocity;
        private float _recoilOffset;
        private float _fireCooldown;

        public bool IsArmed { get; private set; }

        [Inject]
        public void Construct(TurretConfig config, PrefabPool<Bullet> bullets)
        {
            _config = config;
            _bullets = bullets;
            _releaseBullet = bullets.Release;
        }

        private void Awake()
        {
            _recoilRestPosition = _recoilRoot.localPosition;
            _aimLine.enabled = false;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            UpdateRotation();
            UpdateRecoil(deltaTime);

            if (IsArmed)
                UpdateFiring(deltaTime);
        }

        public void Arm()
        {
            IsArmed = true;
            _fireCooldown = 0f;
            _aimLine.enabled = true;
        }

        public void Disarm()
        {
            IsArmed = false;
            _aimLine.enabled = false;
        }

        public void ResetAim()
        {
            _targetYaw = 0f;
            _currentYaw = 0f;
            _yawVelocity = 0f;
            _pivot.localRotation = Quaternion.identity;
        }

        /// <summary>Turns the turret towards a world point, within the allowed angle.</summary>
        public void AimAt(Vector3 worldPoint)
        {
            Vector3 localDirection = transform.InverseTransformDirection(worldPoint - _pivot.position);
            float yaw = Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg;
            _targetYaw = Mathf.Clamp(yaw, -_config.MaxYaw, _config.MaxYaw);
        }

        private void UpdateRotation()
        {
            _currentYaw = Mathf.SmoothDamp(_currentYaw, _targetYaw, ref _yawVelocity, _config.AimSmoothTime);
            _pivot.localRotation = Quaternion.Euler(0f, _currentYaw, 0f);
        }

        private void UpdateRecoil(float deltaTime)
        {
            _recoilOffset = Mathf.Lerp(_recoilOffset, 0f, 1f - Mathf.Exp(-_config.RecoilRecovery * deltaTime));
            _recoilRoot.localPosition = _recoilRestPosition + Vector3.back * _recoilOffset;
        }

        private void UpdateFiring(float deltaTime)
        {
            _fireCooldown -= deltaTime;

            if (_fireCooldown > 0f)
                return;

            _fireCooldown += 1f / _config.FireRate;
            Fire();
        }

        private void Fire()
        {
            Vector3 forward = Vector3.ProjectOnPlane(_muzzle.forward, Vector3.up).normalized;
            Vector3 direction = Quaternion.AngleAxis(Random.Range(-_config.Spread, _config.Spread), Vector3.up) * forward;

            // Hit detection starts at the turret axis, so enemies standing closer than the barrel length are still hit.
            Vector3 origin = _pivot.position;
            origin.y = _muzzle.position.y;

            int damage = Random.Range(_config.DamageRange.x, _config.DamageRange.y + 1);
            _bullets.Get().Launch(origin, _muzzle.position, direction, damage, _config, _releaseBullet);

            _muzzleFlash.Play(true);
            _recoilOffset = _config.RecoilDistance;
        }
    }
}

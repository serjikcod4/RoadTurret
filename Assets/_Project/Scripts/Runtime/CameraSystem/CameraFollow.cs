using RoadTurret.Vehicle;
using UnityEngine;
using VContainer;

namespace RoadTurret.CameraSystem
{
    /// <summary>
    /// Smoothly follows the car from behind and shakes when the car takes damage.
    /// </summary>
    public sealed class CameraFollow : MonoBehaviour
    {
        [Header("Follow")]
        [SerializeField] private Vector3 _offset = new(0f, 14f, -10f);
        [SerializeField] private Vector3 _lookOffset = new(0f, 0f, 8f);
        [SerializeField, Min(0f)] private float _smoothTime = 0.2f;

        [Header("Shake")]
        [SerializeField, Min(0f)] private float _maxShake = 0.5f;
        [SerializeField, Min(0f)] private float _shakeFrequency = 22f;
        [SerializeField, Min(0f)] private float _traumaDecay = 2.5f;
        [SerializeField, Min(0f)] private float _traumaPerDamage = 0.06f;

        private Car _car;
        private Vector3 _position;
        private Vector3 _velocity;
        private float _trauma;

        [Inject]
        public void Construct(Car car)
        {
            _car = car;
            _car.Damaged += OnCarDamaged;
        }

        private void OnDestroy()
        {
            if (_car != null)
                _car.Damaged -= OnCarDamaged;
        }

        private void LateUpdate()
        {
            _position = Vector3.SmoothDamp(_position, GetTargetPosition(), ref _velocity, _smoothTime);
            transform.SetPositionAndRotation(_position, GetLookRotation(_position));
            ApplyShake();
        }

        public void SnapToTarget()
        {
            _position = GetTargetPosition();
            _velocity = Vector3.zero;
            _trauma = 0f;
            transform.SetPositionAndRotation(_position, GetLookRotation(_position));
        }

        public void AddTrauma(float amount) => _trauma = Mathf.Clamp01(_trauma + amount);

        private Vector3 GetTargetPosition() => _car.transform.TransformPoint(_offset);

        private Quaternion GetLookRotation(Vector3 from) =>
            Quaternion.LookRotation(_car.transform.TransformPoint(_lookOffset) - from);

        private void OnCarDamaged(float damage) => AddTrauma(damage * _traumaPerDamage);

        private void ApplyShake()
        {
            if (_trauma <= 0f)
                return;

            float strength = _trauma * _trauma * _maxShake;
            float time = Time.time * _shakeFrequency;
            Vector3 shake = new(Mathf.PerlinNoise(time, 0f) - 0.5f, Mathf.PerlinNoise(0f, time) - 0.5f, 0f);

            transform.position += transform.rotation * shake * (2f * strength);
            _trauma = Mathf.Max(0f, _trauma - _traumaDecay * Time.deltaTime);
        }
    }
}

using UnityEngine;

namespace RoadTurret.Vehicle
{
    /// <summary>
    /// Body reaction to impacts: a quick scale punch with a barely noticeable nudge towards the hit direction.
    /// Only the visual moves, physics and movement are untouched.
    /// </summary>
    public sealed class CarHitReaction : MonoBehaviour
    {
        [SerializeField] private Transform _visual;

        [Header("Punch")]
        [SerializeField, Range(0f, 0.5f)] private float _punchScale = 0.08f;
        [SerializeField, Min(0.01f)] private float _punchDuration = 0.2f;

        [Header("Nudge")]
        [SerializeField, Min(0f)] private float _tiltAngle = 2f;
        [SerializeField, Min(0f)] private float _shoveDistance = 0.04f;
        [SerializeField, Min(0f)] private float _stiffness = 300f;
        [SerializeField, Min(0f)] private float _damping = 28f;

        private float _punchTime = float.MaxValue;
        private float _punchStrength;
        private Vector3 _displacement;
        private Vector3 _velocity;

        /// <summary>Plays the reaction for a hit along a world-space direction. Strength 1 is a regular hit.</summary>
        public void Play(Vector3 worldDirection, float strength = 1f)
        {
            _punchTime = 0f;
            _punchStrength = strength;

            Vector3 local = transform.InverseTransformDirection(worldDirection);
            local.y = 0f;

            if (local.sqrMagnitude > 0.0001f)
                _velocity += local.normalized * (Mathf.Sqrt(_stiffness) * strength);
        }

        public void ResetPose()
        {
            _punchTime = float.MaxValue;
            _displacement = Vector3.zero;
            _velocity = Vector3.zero;
            _visual.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            _visual.localScale = Vector3.one;
        }

        private void LateUpdate()
        {
            bool isPunching = _punchTime < _punchDuration;
            bool isNudged = _displacement != Vector3.zero || _velocity != Vector3.zero;

            if (!isPunching && !isNudged)
                return;

            float deltaTime = Time.deltaTime;
            _punchTime += deltaTime;
            UpdateNudge(deltaTime);
            ApplyPose();
        }

        private void UpdateNudge(float deltaTime)
        {
            Vector3 acceleration = -_stiffness * _displacement - _damping * _velocity;
            _velocity += acceleration * deltaTime;
            _displacement += _velocity * deltaTime;

            if (_displacement.sqrMagnitude < 0.00001f && _velocity.sqrMagnitude < 0.0001f)
            {
                _displacement = Vector3.zero;
                _velocity = Vector3.zero;
            }
        }

        private void ApplyPose()
        {
            // Fast attack, soft release: the square root shifts the sine peak to the first quarter of the punch.
            float progress = Mathf.Clamp01(_punchTime / _punchDuration);
            float punch = Mathf.Sin(Mathf.PI * Mathf.Sqrt(progress)) * _punchScale * _punchStrength;
            _visual.localScale = Vector3.one * (1f + punch);

            // Rotating around (up x direction) tips the roof slightly towards the push direction.
            Quaternion tilt = _displacement == Vector3.zero
                ? Quaternion.identity
                : Quaternion.AngleAxis(_displacement.magnitude * _tiltAngle, Vector3.Cross(Vector3.up, _displacement));

            _visual.SetLocalPositionAndRotation(_displacement * _shoveDistance, tilt);
        }
    }
}

using UnityEngine;

namespace RoadTurret.Effects
{
    /// <summary>
    /// Short emissive flash on the given renderers, used as hit feedback.
    /// </summary>
    public sealed class HitFlash : MonoBehaviour
    {
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        [SerializeField] private Renderer[] _renderers;
        [SerializeField, ColorUsage(false, true)] private Color _color = new(1.5f, 1.5f, 1.5f);
        [SerializeField, Min(0.01f)] private float _duration = 0.1f;

        private MaterialPropertyBlock _propertyBlock;
        private float _timeLeft;

        private void Awake() => _propertyBlock = new MaterialPropertyBlock();

        private void Update()
        {
            if (_timeLeft <= 0f)
                return;

            _timeLeft = Mathf.Max(0f, _timeLeft - Time.deltaTime);
            Apply(_timeLeft / _duration);
        }

        private void OnDisable()
        {
            _timeLeft = 0f;
            Apply(0f);
        }

        public void Play()
        {
            _timeLeft = _duration;
            Apply(1f);
        }

        private void Apply(float strength)
        {
            foreach (Renderer target in _renderers)
            {
                target.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor(EmissionColorId, _color * strength);
                target.SetPropertyBlock(_propertyBlock);
            }
        }
    }
}

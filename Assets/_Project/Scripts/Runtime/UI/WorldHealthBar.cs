using RoadTurret.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoadTurret.UI
{
    /// <summary>
    /// Health bar drawn in the world and turned to the camera.
    /// Fills are resized through anchors so rounded sliced sprites keep their shape at any value.
    /// A trailing fill shows the recently lost health, and the bar punches on every hit.
    /// </summary>
    public sealed class WorldHealthBar : MonoBehaviour
    {
        [Header("Parts")]
        [SerializeField] private RectTransform _fill;
        [SerializeField] private Graphic _fillGraphic;
        [SerializeField] private RectTransform _trail;
        [SerializeField] private TMP_Text _label;

        [Header("Look")]
        [SerializeField] private Gradient _fillColor = new();

        [Header("Animation")]
        [SerializeField, Min(0f)] private float _trailDelay = 0.25f;
        [SerializeField, Min(0f)] private float _trailSpeed = 0.9f;
        [SerializeField, Range(0f, 0.5f)] private float _punchScale = 0.12f;
        [SerializeField, Min(0.01f)] private float _punchDuration = 0.2f;

        private Health _health;
        private Transform _camera;
        private Vector3 _baseScale;
        private bool _hasBaseScale;
        private float _trailValue;
        private float _trailHoldTime;
        private float _punchTime = float.MaxValue;
        private int _shownValue = -1;

        private void Awake()
        {
            _camera = Camera.main != null ? Camera.main.transform : null;
            CacheBaseScale();
        }

        private void OnDestroy()
        {
            if (_health != null)
                _health.Changed -= OnHealthChanged;
        }

        private void LateUpdate()
        {
            if (_health == null)
                return;

            UpdateTrail(Time.deltaTime);
            UpdatePunch(Time.deltaTime);

            if (_camera != null)
                transform.rotation = _camera.rotation;
        }

        public void Bind(Health health)
        {
            if (_health != null)
                _health.Changed -= OnHealthChanged;

            // Scene objects can be injected before their own Awake, so the scale is cached here as well.
            CacheBaseScale();
            _health = health;
            _health.Changed += OnHealthChanged;
            ShowInstantly();
        }

        public void SetVisible(bool isVisible) => gameObject.SetActive(isVisible);

        private void OnHealthChanged()
        {
            float value = _health.Normalized;

            if (value >= _trailValue)
            {
                ShowInstantly();
                return;
            }

            SetFill(value);
            _trailHoldTime = _trailDelay;
            _punchTime = 0f;
        }

        private void ShowInstantly()
        {
            float value = _health.Normalized;
            SetFill(value);
            _trailValue = value;
            SetWidth(_trail, value);
            _punchTime = float.MaxValue;
            transform.localScale = _baseScale;
        }

        private void SetFill(float value)
        {
            SetWidth(_fill, value);
            _fillGraphic.color = _fillColor.Evaluate(value);

            int shownValue = Mathf.CeilToInt(_health.Current);

            if (_label == null || shownValue == _shownValue)
                return;

            _shownValue = shownValue;
            _label.text = shownValue.ToString();
        }

        private void UpdateTrail(float deltaTime)
        {
            float value = _health.Normalized;

            if (_trailValue <= value)
                return;

            if (_trailHoldTime > 0f)
            {
                _trailHoldTime -= deltaTime;
                return;
            }

            _trailValue = Mathf.MoveTowards(_trailValue, value, _trailSpeed * deltaTime);
            SetWidth(_trail, _trailValue);
        }

        private void UpdatePunch(float deltaTime)
        {
            if (_punchTime >= _punchDuration)
                return;

            _punchTime += deltaTime;

            // Fast attack, soft release: the square root shifts the sine peak to the first quarter of the punch.
            float progress = Mathf.Clamp01(_punchTime / _punchDuration);
            float punch = Mathf.Sin(Mathf.PI * Mathf.Sqrt(progress)) * _punchScale;
            transform.localScale = _baseScale * (1f + punch);
        }

        private void CacheBaseScale()
        {
            if (_hasBaseScale)
                return;

            _baseScale = transform.localScale;
            _hasBaseScale = true;
        }

        private static void SetWidth(RectTransform rect, float value) =>
            rect.anchorMax = new Vector2(Mathf.Clamp01(value), 1f);
    }
}

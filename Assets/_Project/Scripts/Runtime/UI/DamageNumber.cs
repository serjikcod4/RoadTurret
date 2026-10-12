using System;
using TMPro;
using UnityEngine;

namespace RoadTurret.UI
{
    /// <summary>
    /// Floating damage value: pops up, rises and fades out, then reports back to its pool.
    /// </summary>
    public sealed class DamageNumber : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField, Min(0.05f)] private float _lifetime = 0.75f;
        [SerializeField, Min(0f)] private float _riseHeight = 1.6f;
        [SerializeField, Min(0f)] private float _sideDrift = 0.6f;
        [SerializeField] private AnimationCurve _scale = new(
            new Keyframe(0f, 0.4f), new Keyframe(0.15f, 1.25f), new Keyframe(0.3f, 1f), new Keyframe(1f, 0.85f));

        private Transform _camera;
        private Action<DamageNumber> _onFinished;
        private Vector3 _origin;
        private Vector3 _drift;
        private Color _color;
        private float _age;

        private void Awake() => _camera = Camera.main != null ? Camera.main.transform : null;

        private void LateUpdate()
        {
            _age += Time.deltaTime;
            float progress = Mathf.Clamp01(_age / _lifetime);
            float rise = 1f - (1f - progress) * (1f - progress);

            transform.position = _origin + Vector3.up * (_riseHeight * rise) + _drift * progress;
            transform.localScale = Vector3.one * _scale.Evaluate(progress);

            if (_camera != null)
                transform.rotation = _camera.rotation;

            _color.a = 1f - progress * progress * progress;
            _label.color = _color;

            if (progress >= 1f)
                Finish();
        }

        public void Show(Vector3 position, int amount, Color color, Action<DamageNumber> onFinished)
        {
            _origin = position;
            _drift = UnityEngine.Random.Range(-1f, 1f) * _sideDrift * Vector3.right;
            _color = color;
            _age = 0f;
            _onFinished = onFinished;
            _label.SetText("{0}", amount);
            LateUpdate();
        }

        private void Finish()
        {
            Action<DamageNumber> onFinished = _onFinished;
            _onFinished = null;
            onFinished?.Invoke(this);
        }
    }
}

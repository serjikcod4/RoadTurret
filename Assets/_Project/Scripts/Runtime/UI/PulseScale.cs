using UnityEngine;

namespace RoadTurret.UI
{
    /// <summary>
    /// Gentle looping scale pulse, used to draw attention to "tap" hints.
    /// </summary>
    public sealed class PulseScale : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _amplitude = 0.06f;
        [SerializeField, Min(0f)] private float _frequency = 1.5f;

        private void Update()
        {
            float scale = 1f + Mathf.Sin(Time.unscaledTime * _frequency * Mathf.PI * 2f) * _amplitude;
            transform.localScale = new Vector3(scale, scale, 1f);
        }
    }
}

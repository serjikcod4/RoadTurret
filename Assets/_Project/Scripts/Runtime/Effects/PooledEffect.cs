using System;
using UnityEngine;

namespace RoadTurret.Effects
{
    /// <summary>
    /// One-shot particle effect that reports back when all of its particles have finished.
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public sealed class PooledEffect : MonoBehaviour
    {
        private ParticleSystem _particles;
        private Action<PooledEffect> _onFinished;

        private void Awake()
        {
            _particles = GetComponent<ParticleSystem>();

            ParticleSystem.MainModule main = _particles.main;
            main.playOnAwake = false;
            main.stopAction = ParticleSystemStopAction.Callback;
        }

        public void Play(Action<PooledEffect> onFinished)
        {
            _onFinished = onFinished;
            _particles.Clear(true);
            _particles.Play(true);
        }

        private void OnParticleSystemStopped()
        {
            Action<PooledEffect> onFinished = _onFinished;
            _onFinished = null;
            onFinished?.Invoke(this);
        }
    }
}

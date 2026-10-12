using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using RoadTurret.Common;
using RoadTurret.Configs;
using RoadTurret.Effects;
using RoadTurret.UI;
using RoadTurret.Vehicle;
using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;

namespace RoadTurret.Enemies
{
    /// <summary>
    /// Stands idle until the car comes close, then runs at it. On contact it damages the car and is smashed by it.
    /// </summary>
    [RequireComponent(typeof(CapsuleCollider))]
    public sealed class Enemy : MonoBehaviour, IDamageable
    {
        private enum State
        {
            Idle,
            Chase,
            Dead
        }

        private static readonly int IdleAnimation = Animator.StringToHash("Idle");
        private static readonly int RunAnimation = Animator.StringToHash("Run");
        private static readonly int DeathAnimation = Animator.StringToHash("Death");

        [SerializeField] private CapsuleCollider _collider;
        [SerializeField] private Animator _animator;
        [SerializeField] private HitFlash _hitFlash;
        [SerializeField] private WorldHealthBar _healthBar;
        [SerializeField] private Color _damageNumberColor = new(1f, 0.95f, 0.6f);
        [SerializeField] private PooledEffect _hitEffect;
        [SerializeField] private PooledEffect _deathEffect;

        private EnemyConfig _config;
        private Car _car;
        private EffectsPlayer _effects;
        private DamageNumbers _damageNumbers;
        private Health _health;
        private CancellationTokenSource _lifetime;
        private State _state;
        private bool _isFrozen;
        private Vector3 _knockbackVelocity;

        /// <summary>Raised when the turret kills the enemy; crashing into the car is not a kill.</summary>
        public event Action<Enemy> Killed;
        public event Action<Enemy> Despawned;

        public bool IsAlive => _state != State.Dead;
        private Vector3 BodyCenter => transform.TransformPoint(_collider.center);
        private Vector3 HeadTop => transform.TransformPoint(_collider.center + Vector3.up * (_collider.height * 0.5f));
        private bool HasAnimator => _animator != null && _animator.runtimeAnimatorController != null;

        [Inject]
        public void Construct(EnemyConfig config, Car car, EffectsPlayer effects, DamageNumbers damageNumbers)
        {
            _config = config;
            _car = car;
            _effects = effects;
            _damageNumbers = damageNumbers;
            _health = new Health(config.MaxHealth);
            _healthBar.Bind(_health);
        }

        private void Update()
        {
            if (_state != State.Dead && IsLeftBehind())
            {
                Despawn();
                return;
            }

            float deltaTime = Time.deltaTime;

            switch (_state)
            {
                case State.Idle:
                    UpdateIdle();
                    break;
                case State.Chase:
                    UpdateChase(deltaTime);
                    break;
                case State.Dead:
                    MoveWithKnockback(Vector3.zero, deltaTime);
                    break;
            }
        }

        private void OnDisable() => CancelLifetime();

        public void Spawn(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);

            _lifetime = new CancellationTokenSource();
            _health.Restore();
            _collider.enabled = true;
            _isFrozen = false;
            _knockbackVelocity = Vector3.zero;

            EnterState(State.Idle);

            if (HasAnimator)
                _animator.Play(IdleAnimation, 0, Random.value);
        }

        /// <summary>Stops all activity, used when the round is over.</summary>
        public void Freeze()
        {
            _isFrozen = true;

            if (_state == State.Chase)
                EnterState(State.Idle);
        }

        public void Despawn()
        {
            CancelLifetime();
            Despawned?.Invoke(this);
        }

        public void TakeDamage(float amount, Vector3 hitDirection)
        {
            if (!IsAlive)
                return;

            _health.TakeDamage(amount);
            _hitFlash.Play();
            _effects.Play(_hitEffect, BodyCenter, Quaternion.LookRotation(hitDirection));
            _damageNumbers.Show(HeadTop, amount, _damageNumberColor);

            _knockbackVelocity += Vector3.ProjectOnPlane(hitDirection, Vector3.up) * _config.KnockbackSpeed;

            if (_health.IsDead)
            {
                Die(hitDirection, isKilledByPlayer: true);
                return;
            }

            if (_state == State.Idle && !_isFrozen)
                EnterState(State.Chase);
        }

        private void UpdateIdle()
        {
            if (_isFrozen)
                return;

            if ((_car.Position - transform.position).sqrMagnitude <= _config.AggroRadius * _config.AggroRadius)
                EnterState(State.Chase);
        }

        private void UpdateChase(float deltaTime)
        {
            Vector3 position = transform.position;
            Vector3 toCar = Vector3.ProjectOnPlane(_car.ClosestPointOnBody(position) - position, Vector3.up);
            float distance = toCar.magnitude;

            if (distance <= _config.AttackRange)
            {
                CrashIntoCar(toCar / Mathf.Max(distance, 0.001f));
                return;
            }

            Vector3 direction = toCar / distance;
            MoveWithKnockback(direction * _config.RunSpeed, deltaTime);
            TurnTowards(direction, deltaTime);
        }

        private void CrashIntoCar(Vector3 directionToCar)
        {
            _car.TakeDamage(_config.ImpactDamage, directionToCar);

            // Smashed by the moving car: thrown aside and back.
            Transform car = _car.transform;
            float side = car.InverseTransformPoint(transform.position).x >= 0f ? 1f : -1f;
            Vector3 away = (car.right * side - car.forward * 0.5f).normalized;
            _knockbackVelocity = away * _config.ImpactKnockback;

            Die(away, isKilledByPlayer: false);
        }

        private void MoveWithKnockback(Vector3 velocity, float deltaTime)
        {
            transform.position += (velocity + _knockbackVelocity) * deltaTime;
            _knockbackVelocity = Vector3.Lerp(_knockbackVelocity, Vector3.zero, 1f - Mathf.Exp(-_config.KnockbackDamping * deltaTime));
        }

        private void Die(Vector3 hitDirection, bool isKilledByPlayer)
        {
            EnterState(State.Dead);
            _collider.enabled = false;

            Vector3 fallDirection = Vector3.ProjectOnPlane(hitDirection, Vector3.up);

            if (fallDirection.sqrMagnitude > 0f)
                transform.rotation = Quaternion.LookRotation(-fallDirection);

            _effects.Play(_deathEffect, BodyCenter, Quaternion.LookRotation(hitDirection));

            if (isKilledByPlayer)
                Killed?.Invoke(this);

            RemoveCorpseAsync(_lifetime.Token).Forget();
        }

        private async UniTaskVoid RemoveCorpseAsync(CancellationToken cancellationToken)
        {
            bool isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(_config.CorpseDuration), cancellationToken: cancellationToken)
                .SuppressCancellationThrow();

            if (isCanceled)
                return;

            Vector3 start = transform.position;
            float duration = _config.CorpseSinkDuration;

            for (float time = 0f; time < duration; time += Time.deltaTime)
            {
                transform.position = start + Vector3.down * (time / duration * _collider.height);

                if (await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken).SuppressCancellationThrow())
                    return;
            }

            Despawn();
        }

        private bool IsLeftBehind()
        {
            Vector3 fromEnemyToCar = _car.Position - transform.position;
            return Vector3.Dot(fromEnemyToCar, _car.transform.forward) > _config.DespawnDistanceBehind;
        }

        private void EnterState(State state)
        {
            _state = state;

            _healthBar.SetVisible(state != State.Dead);

            switch (state)
            {
                case State.Idle:
                    PlayAnimation(IdleAnimation);
                    break;
                case State.Chase:
                    PlayAnimation(RunAnimation);
                    break;
                case State.Dead:
                    PlayAnimation(DeathAnimation);
                    break;
            }
        }

        private void TurnTowards(Vector3 direction, float deltaTime)
        {
            if (direction.sqrMagnitude < 0.0001f)
                return;

            Quaternion target = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, _config.TurnSpeed * deltaTime);
        }

        private void PlayAnimation(int stateHash)
        {
            if (HasAnimator)
                _animator.CrossFadeInFixedTime(stateHash, 0.15f);
        }

        private void CancelLifetime()
        {
            if (_lifetime == null)
                return;

            _lifetime.Cancel();
            _lifetime.Dispose();
            _lifetime = null;
        }
    }
}

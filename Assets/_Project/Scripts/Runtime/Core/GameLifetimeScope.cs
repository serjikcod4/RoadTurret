using RoadTurret.CameraSystem;
using RoadTurret.Common;
using RoadTurret.Configs;
using RoadTurret.Effects;
using RoadTurret.Enemies;
using RoadTurret.Input;
using RoadTurret.Level;
using RoadTurret.UI;
using RoadTurret.Vehicle;
using RoadTurret.Weapons;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RoadTurret.Core
{
    public sealed class GameLifetimeScope : LifetimeScope
    {
        [Header("Configs")]
        [SerializeField] private CarConfig _carConfig;
        [SerializeField] private TurretConfig _turretConfig;
        [SerializeField] private EnemyConfig _enemyConfig;
        [SerializeField] private LevelConfig _levelConfig;

        [Header("Scene")]
        [SerializeField] private Car _car;
        [SerializeField] private Turret _turret;
        [SerializeField] private Track _track;
        [SerializeField] private Camera _camera;
        [SerializeField] private CameraFollow _cameraFollow;
        [SerializeField] private HudView _hud;
        [SerializeField] private OverlayView _overlay;

        [Header("Pooling")]
        [SerializeField] private Bullet _bulletPrefab;
        [SerializeField] private Enemy _enemyPrefab;
        [SerializeField] private DamageNumber _damageNumberPrefab;
        [SerializeField] private Transform _bulletsRoot;
        [SerializeField] private Transform _enemiesRoot;
        [SerializeField] private Transform _effectsRoot;
        [SerializeField] private Transform _damageNumbersRoot;
        [SerializeField, Min(0)] private int _bulletsPrewarm = 16;
        [SerializeField, Min(0)] private int _enemiesPrewarm = 40;
        [SerializeField, Min(0)] private int _damageNumbersPrewarm = 16;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_carConfig);
            builder.RegisterInstance(_turretConfig);
            builder.RegisterInstance(_enemyConfig);
            builder.RegisterInstance(_levelConfig);

            builder.RegisterComponent(_car);
            builder.RegisterComponent(_turret);
            builder.RegisterComponent(_track);
            builder.RegisterComponent(_camera);
            builder.RegisterComponent(_cameraFollow);
            builder.RegisterComponent(_hud);
            builder.RegisterComponent(_overlay);

            builder.Register(resolver => new EffectsPlayer(resolver, _effectsRoot), Lifetime.Singleton);
            builder.Register(resolver => new PrefabPool<Bullet>(resolver, _bulletPrefab, _bulletsRoot, _bulletsPrewarm), Lifetime.Singleton);
            builder.Register(resolver => new PrefabPool<Enemy>(resolver, _enemyPrefab, _enemiesRoot, _enemiesPrewarm), Lifetime.Singleton);
            builder.Register(resolver => new PrefabPool<DamageNumber>(resolver, _damageNumberPrefab, _damageNumbersRoot, _damageNumbersPrewarm), Lifetime.Singleton);
            builder.Register<DamageNumbers>(Lifetime.Singleton);
            builder.Register<EnemySpawner>(Lifetime.Singleton);

            builder.Register<PointerPlayerInput>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.RegisterEntryPoint<TurretAimController>();
            builder.RegisterEntryPoint<GameFlow>();
        }
    }
}

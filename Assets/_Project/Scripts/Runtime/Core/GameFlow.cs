using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using RoadTurret.CameraSystem;
using RoadTurret.Enemies;
using RoadTurret.Input;
using RoadTurret.Level;
using RoadTurret.UI;
using RoadTurret.Vehicle;
using RoadTurret.Weapons;
using VContainer.Unity;

namespace RoadTurret.Core
{
    /// <summary>
    /// Game loop: wait for a tap, drive to the finish or until the car is destroyed, show the result, repeat.
    /// </summary>
    public sealed class GameFlow : IAsyncStartable
    {
        private const float DestroyedCameraTrauma = 0.9f;
        private static readonly TimeSpan ResultDelay = TimeSpan.FromSeconds(0.8);

        private readonly Car _car;
        private readonly Turret _turret;
        private readonly Track _track;
        private readonly EnemySpawner _enemies;
        private readonly CameraFollow _camera;
        private readonly OverlayView _overlay;
        private readonly IPlayerInput _input;

        public GameFlow(Car car, Turret turret, Track track, EnemySpawner enemies, CameraFollow camera,
            OverlayView overlay, IPlayerInput input)
        {
            _car = car;
            _turret = turret;
            _track = track;
            _enemies = enemies;
            _camera = camera;
            _overlay = overlay;
            _input = input;
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            try
            {
                while (!cancellation.IsCancellationRequested)
                    await PlayRoundAsync(cancellation);
            }
            catch (OperationCanceledException)
            {
                // The scope is being destroyed.
            }
        }

        private async UniTask PlayRoundAsync(CancellationToken cancellation)
        {
            PrepareRound();

            await _overlay.ShowStartAsync(cancellation);
            await _input.WaitForTapAsync(cancellation);
            _overlay.HideStartAsync(cancellation).Forget();

            GameResult result = await DriveAsync(cancellation);

            await UniTask.Delay(ResultDelay, cancellationToken: cancellation);
            await _overlay.ShowResultAsync(result, _enemies.KilledCount, _enemies.SpawnedCount, cancellation);
            await _input.WaitForTapAsync(cancellation);
            await _overlay.HideResultAsync(cancellation);
        }

        private void PrepareRound()
        {
            _car.PlaceAt(_track.StartPose);
            _turret.Disarm();
            _turret.ResetAim();
            _enemies.SpawnLevel();
            _camera.SnapToTarget();
        }

        private async UniTask<GameResult> DriveAsync(CancellationToken cancellation)
        {
            _car.Drive();
            _turret.Arm();

            await UniTask.WaitUntil(this, flow => !flow._car.IsAlive || flow._track.IsFinished(flow._car.Position),
                cancellationToken: cancellation);

            _turret.Disarm();
            _car.Stop();
            _enemies.FreezeAll();

            if (_car.IsAlive)
                return GameResult.Win;

            _camera.AddTrauma(DestroyedCameraTrauma);
            return GameResult.Lose;
        }
    }
}

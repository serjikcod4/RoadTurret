using RoadTurret.Input;
using UnityEngine;
using VContainer.Unity;

namespace RoadTurret.Weapons
{
    /// <summary>
    /// Points the turret at the spot on the ground under the cursor or finger.
    /// </summary>
    public sealed class TurretAimController : ITickable
    {
        private static readonly Plane Ground = new(Vector3.up, Vector3.zero);

        private readonly Turret _turret;
        private readonly IPlayerInput _input;
        private readonly Camera _camera;

        public TurretAimController(Turret turret, IPlayerInput input, Camera camera)
        {
            _turret = turret;
            _input = input;
            _camera = camera;
        }

        public void Tick()
        {
            if (!_turret.IsArmed || !_input.TryGetAimPoint(out Vector2 screenPoint))
                return;

            Ray ray = _camera.ScreenPointToRay(screenPoint);

            if (Ground.Raycast(ray, out float distance))
                _turret.AimAt(ray.GetPoint(distance));
        }
    }
}

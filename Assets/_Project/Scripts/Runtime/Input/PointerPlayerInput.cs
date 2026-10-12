using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace RoadTurret.Input
{
    /// <summary>
    /// The mouse aims wherever the cursor is, a touch aims while the finger is on the screen.
    /// A tap or click continues the game.
    /// </summary>
    public sealed class PointerPlayerInput : IPlayerInput, IInitializable, IDisposable
    {
        private readonly InputAction _press = new("Press", InputActionType.Button, "<Pointer>/press");
        private readonly InputAction _position = new("Position", InputActionType.Value, "<Pointer>/position", expectedControlType: "Vector2");

        public void Initialize()
        {
            _press.Enable();
            _position.Enable();
        }

        public void Dispose()
        {
            _press.Dispose();
            _position.Dispose();
        }

        public bool TryGetAimPoint(out Vector2 screenPoint)
        {
            screenPoint = _position.ReadValue<Vector2>();
            bool isTouch = _position.activeControl?.device is Touchscreen;
            return !isTouch || _press.IsPressed();
        }

        public UniTask WaitForTapAsync(CancellationToken cancellationToken) =>
            UniTask.WaitUntil(_press, action => action.WasPressedThisFrame(), cancellationToken: cancellationToken);
    }
}

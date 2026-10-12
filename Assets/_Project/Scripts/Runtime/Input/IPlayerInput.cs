using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace RoadTurret.Input
{
    public interface IPlayerInput
    {
        /// <summary>Screen point the player is aiming at, if any.</summary>
        bool TryGetAimPoint(out Vector2 screenPoint);

        UniTask WaitForTapAsync(CancellationToken cancellationToken);
    }
}

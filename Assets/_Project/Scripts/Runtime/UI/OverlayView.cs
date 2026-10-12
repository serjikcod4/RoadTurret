using System.Threading;
using Cysharp.Threading.Tasks;
using RoadTurret.Core;
using TMPro;
using UnityEngine;

namespace RoadTurret.UI
{
    /// <summary>
    /// Full-screen messages: "tap to start" and the round result.
    /// </summary>
    public sealed class OverlayView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _startScreen;
        [SerializeField] private CanvasGroup _resultScreen;
        [SerializeField] private TMP_Text _resultTitle;
        [SerializeField] private TMP_Text _resultDetails;
        [SerializeField] private Color _winColor = new(1f, 0.85f, 0.2f);
        [SerializeField] private Color _loseColor = new(1f, 0.3f, 0.25f);
        [SerializeField, Min(0f)] private float _fadeDuration = 0.25f;

        private void Awake()
        {
            SetVisible(_startScreen, false);
            SetVisible(_resultScreen, false);
        }

        public UniTask ShowStartAsync(CancellationToken cancellationToken) =>
            FadeAsync(_startScreen, 1f, cancellationToken);

        public UniTask HideStartAsync(CancellationToken cancellationToken) =>
            FadeAsync(_startScreen, 0f, cancellationToken);

        public UniTask ShowResultAsync(GameResult result, int kills, int enemies, CancellationToken cancellationToken)
        {
            bool isWin = result == GameResult.Win;
            _resultTitle.text = isWin ? "You win" : "You lose";
            _resultTitle.color = isWin ? _winColor : _loseColor;
            _resultDetails.text = $"Enemies killed: {kills} / {enemies}";

            return FadeAsync(_resultScreen, 1f, cancellationToken);
        }

        public UniTask HideResultAsync(CancellationToken cancellationToken) =>
            FadeAsync(_resultScreen, 0f, cancellationToken);

        private async UniTask FadeAsync(CanvasGroup group, float targetAlpha, CancellationToken cancellationToken)
        {
            group.gameObject.SetActive(true);
            float startAlpha = group.alpha;

            for (float time = 0f; time < _fadeDuration; time += Time.unscaledDeltaTime)
            {
                group.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / _fadeDuration);
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            SetVisible(group, targetAlpha > 0f);
        }

        private static void SetVisible(CanvasGroup group, bool isVisible)
        {
            group.alpha = isVisible ? 1f : 0f;
            group.gameObject.SetActive(isVisible);
        }
    }
}

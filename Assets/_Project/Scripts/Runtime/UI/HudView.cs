using RoadTurret.Enemies;
using RoadTurret.Level;
using RoadTurret.Vehicle;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace RoadTurret.UI
{
    /// <summary>
    /// Level progress and kill counter.
    /// </summary>
    public sealed class HudView : MonoBehaviour
    {
        [SerializeField] private Image _progressFill;
        [SerializeField] private RectTransform _progressMarker;
        [SerializeField] private TMP_Text _distanceLabel;
        [SerializeField] private TMP_Text _killsLabel;
        [SerializeField] private RectTransform _killsPanel;
        [SerializeField, Min(0f)] private float _killPunchScale = 0.35f;
        [SerializeField, Min(0.1f)] private float _killPunchRecovery = 10f;

        private Car _car;
        private Track _track;
        private EnemySpawner _enemies;
        private int _shownDistance = -1;
        private int _shownKills = -1;
        private float _killPunch;

        [Inject]
        public void Construct(Car car, Track track, EnemySpawner enemies)
        {
            _car = car;
            _track = track;
            _enemies = enemies;
        }

        private void LateUpdate()
        {
            UpdateProgress();
            UpdateKills();
        }

        private void UpdateProgress()
        {
            float progress = _track.GetProgress(_car.Position);
            _progressFill.fillAmount = progress;

            Vector2 anchor = _progressMarker.anchorMin;
            anchor.y = progress;
            _progressMarker.anchorMin = anchor;
            _progressMarker.anchorMax = anchor;

            int distance = Mathf.CeilToInt(Mathf.Max(0f, _track.Length - _track.GetDistance(_car.Position)));

            if (distance == _shownDistance)
                return;

            _shownDistance = distance;
            _distanceLabel.SetText("{0}m", distance);
        }

        private void UpdateKills()
        {
            int kills = _enemies.KilledCount;

            if (kills != _shownKills)
            {
                if (kills > _shownKills && _shownKills >= 0)
                    _killPunch = _killPunchScale;

                _shownKills = kills;
                _killsLabel.SetText("{0}", kills);
            }

            _killPunch = Mathf.Lerp(_killPunch, 0f, 1f - Mathf.Exp(-_killPunchRecovery * Time.deltaTime));
            _killsPanel.localScale = Vector3.one * (1f + _killPunch);
        }
    }
}

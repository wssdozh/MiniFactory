using MiniFactory.Feedback;
using MiniFactory.Gameplay;
using MiniFactory.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniFactory.UI
{
    public class BoostUI : MonoBehaviour
    {
        [SerializeField] private Button boostButton;
        [SerializeField] private TextMeshProUGUI boostButtonText;
        [SerializeField] private TextMeshProUGUI timerText;

        private BoostState _boostState;
        private IConfigService _config;
        private FeedbackRoot _feedback;
        private float _boostDuration = 30f;
        private float _boostMultiplier = 2f;
        private int _lastDisplayedSeconds = -1;

        public void Initialize(BoostState boostState, IConfigService config = null, FeedbackRoot feedback = null)
        {
            _boostState = boostState;
            _config = config;
            _feedback = feedback;

            if (_config != null)
            {
                _boostDuration = _config.BoostDuration;
                _boostMultiplier = _config.BoostMultiplier;

                if (_config.IsBoostEnabled == false)
                {
                    gameObject.SetActive(false);
                    return;
                }
            }

            if (_boostState != null)
                _boostState.StateChanged += OnBoostStateChanged;

            if (boostButton != null)
                boostButton.onClick.AddListener(OnBoostClicked);

            UpdateDisplay();
        }

        private void Update()
        {
            if (_boostState == null || _boostState.IsActive == false)
                return;

            int remainingSeconds = Mathf.CeilToInt(_boostState.RemainingSeconds);
            if (remainingSeconds != _lastDisplayedSeconds)
            {
                _lastDisplayedSeconds = remainingSeconds;
                if (boostButtonText != null)
                    boostButtonText.SetText("ACTIVE\n{0}s", remainingSeconds);
            }
        }

        private void OnBoostClicked()
        {
            if (_feedback != null)
                _feedback.Play(FeedbackType.ButtonClick);

            if (_boostState != null && _boostState.IsActive == false)
                _boostState.Activate(_boostDuration, _boostMultiplier);
        }

        private void OnBoostStateChanged()
        {
            _lastDisplayedSeconds = -1;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (_boostState == null)
                return;

            if (_boostState.IsActive)
            {
                if (boostButton != null)
                    boostButton.interactable = false;

                if (boostButtonText != null)
                    boostButtonText.text = $"ACTIVE\n{_boostState.RemainingSeconds:F0}s";

                if (timerText != null)
                    timerText.text = string.Empty;
            }
            else
            {
                if (boostButton != null)
                    boostButton.interactable = true;

                if (boostButtonText != null)
                    boostButtonText.text = $"BOOST\n{_boostMultiplier:0.#}X";

                if (timerText != null)
                    timerText.text = string.Empty;
            }
        }

        private void OnDisable()
        {
            if (_boostState != null)
                _boostState.StateChanged -= OnBoostStateChanged;

            if (boostButton != null)
                boostButton.onClick.RemoveListener(OnBoostClicked);
        }
    }
}

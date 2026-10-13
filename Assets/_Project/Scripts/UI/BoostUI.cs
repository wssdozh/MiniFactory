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
        private float _boostDuration = 30f;
        private float _boostMultiplier = 2f;

        public void Initialize(BoostState boostState, IConfigService config = null)
        {
            _boostState = boostState;
            _config = config;

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
            {
                _boostState.StateChanged += OnBoostStateChanged;
            }

            if (boostButton != null)
            {
                boostButton.onClick.AddListener(OnBoostClicked);
            }

            UpdateDisplay();
        }

        private void Update()
        {
            if (_boostState != null && _boostState.IsActive)
            {
                if (timerText != null)
                {
                    timerText.text = $"{_boostState.RemainingSeconds:F0}s";
                }
            }
        }

        private void OnBoostClicked()
        {
            if (_boostState != null && _boostState.IsActive == false)
            {
                _boostState.Activate(_boostDuration, _boostMultiplier);
            }
        }

        private void OnBoostStateChanged()
        {
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (_boostState == null)
            {
                return;
            }

            if (_boostState.IsActive)
            {
                if (boostButton != null)
                {
                    boostButton.interactable = false;
                }

                if (boostButtonText != null)
                {
                    boostButtonText.text = "Active";
                }

                if (timerText != null)
                {
                    timerText.text = $"{_boostState.RemainingSeconds:F0}s";
                }
            }
            else
            {
                if (boostButton != null)
                {
                    boostButton.interactable = true;
                }

                if (boostButtonText != null)
                {
                    boostButtonText.text = $"Boost {_boostMultiplier:0.#}x";
                }

                if (timerText != null)
                {
                    timerText.text = "";
                }
            }
        }

        private void OnDisable()
        {
            if (_boostState != null)
            {
                _boostState.StateChanged -= OnBoostStateChanged;
            }

            if (boostButton != null)
            {
                boostButton.onClick.RemoveListener(OnBoostClicked);
            }
        }
    }
}

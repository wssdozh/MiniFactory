using MiniFactory.Gameplay;
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
        [SerializeField] private float boostDuration = 30f;
        [SerializeField] private float boostMultiplier = 2f;

        private BoostState _boostState;

        public void Initialize(BoostState boostState)
        {
            _boostState = boostState;
            _boostState.StateChanged += OnBoostStateChanged;

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
                _boostState.Activate(boostDuration, boostMultiplier);
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
                    boostButtonText.text = "Boost Active";
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
                    boostButtonText.text = "Boost 2x";
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

using MiniFactory.Feedback;
using MiniFactory.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniFactory.UI
{
    public class MachineCardUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI incomeText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Button unlockButton;
        [SerializeField] private TextMeshProUGUI unlockCostText;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private TextMeshProUGUI upgradeCostText;

        private MachineState _state;
        private Wallet _wallet;
        private MachineCollection _machines;
        private FeedbackRoot _feedback;

        public void Initialize(MachineState state, Wallet wallet, MachineCollection machines, FeedbackRoot feedback = null)
        {
            _state = state;
            _wallet = wallet;
            _machines = machines;
            _feedback = feedback;

            if (_state != null)
            {
                _state.Changed += UpdateDisplay;
            }

            if (_wallet != null)
            {
                _wallet.CoinsChanged += OnCoinsChanged;
            }

            if (unlockButton != null)
            {
                unlockButton.onClick.AddListener(OnUnlockClicked);
            }

            if (upgradeButton != null)
            {
                upgradeButton.onClick.AddListener(OnUpgradeClicked);
            }

            UpdateDisplay();
        }

        private void OnUnlockClicked()
        {
            if (_feedback != null)
            {
                _feedback.Play(FeedbackType.ButtonClick);
            }

            if (_machines != null && _state != null && _wallet != null)
            {
                _machines.TryUnlock(_state.MachineId, _wallet);
            }
        }

        private void OnUpgradeClicked()
        {
            if (_feedback != null)
            {
                _feedback.Play(FeedbackType.ButtonClick);
            }

            if (_machines != null && _state != null && _wallet != null)
            {
                _machines.TryUpgrade(_state.MachineId, _wallet);
            }
        }

        private void OnCoinsChanged(int coins)
        {
            UpdateButtonsInteractable();
        }

        private void UpdateDisplay()
        {
            if (_state == null)
            {
                return;
            }

            if (nameText != null)
            {
                nameText.text = _state.DisplayName;
            }

            if (iconImage != null && _state.Icon != null)
            {
                iconImage.sprite = _state.Icon;
            }

            bool isUnlocked = _state.IsUnlocked;

            if (levelText != null)
            {
                levelText.text = isUnlocked ? $"Lvl {_state.Level}" : "Locked";
            }

            if (incomeText != null)
            {
                incomeText.text = $"+{(isUnlocked ? _state.CurrentIncomePerSecond : _state.BaseIncomePerSecond):F1}/s";
            }

            if (unlockButton != null)
            {
                unlockButton.gameObject.SetActive(isUnlocked == false);
            }

            if (upgradeButton != null)
            {
                upgradeButton.gameObject.SetActive(isUnlocked);
            }

            if (unlockCostText != null)
            {
                unlockCostText.text = $"UNLOCK\n{_state.UnlockCost}";
            }

            if (upgradeCostText != null)
            {
                upgradeCostText.text = $"UPGRADE\n{_state.CurrentUpgradeCost}";
            }

            UpdateButtonsInteractable();
        }

        private void UpdateButtonsInteractable()
        {
            if (_state == null || _wallet == null)
            {
                return;
            }

            if (_state.IsUnlocked == false)
            {
                if (unlockButton != null)
                {
                    unlockButton.interactable = _wallet.Coins >= _state.UnlockCost;
                }
            }
            else
            {
                if (upgradeButton != null)
                {
                    upgradeButton.interactable = _wallet.Coins >= _state.CurrentUpgradeCost;
                }
            }
        }

        private void OnDisable()
        {
            if (_state != null)
            {
                _state.Changed -= UpdateDisplay;
            }

            if (_wallet != null)
            {
                _wallet.CoinsChanged -= OnCoinsChanged;
            }

            if (unlockButton != null)
            {
                unlockButton.onClick.RemoveListener(OnUnlockClicked);
            }

            if (upgradeButton != null)
            {
                upgradeButton.onClick.RemoveListener(OnUpgradeClicked);
            }
        }
    }
}

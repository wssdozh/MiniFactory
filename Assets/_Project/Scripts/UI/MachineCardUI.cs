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

        public void Initialize(MachineState state, Wallet wallet, MachineCollection machines)
        {
            _state = state;
            _wallet = wallet;
            _machines = machines;

            GameplayEvents.MachineUnlocked += OnMachineUnlocked;
            GameplayEvents.MachineUpgraded += OnMachineUpgraded;

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
            if (_machines != null && _state != null && _wallet != null)
            {
                _machines.TryUnlock(_state.MachineId, _wallet);
            }
        }

        private void OnUpgradeClicked()
        {
            if (_machines != null && _state != null && _wallet != null)
            {
                _machines.TryUpgrade(_state.MachineId, _wallet);
            }
        }

        private void OnMachineUnlocked(string id)
        {
            if (_state != null && id == _state.MachineId)
            {
                UpdateDisplay();
            }
        }

        private void OnMachineUpgraded(string id, int level)
        {
            if (_state != null && id == _state.MachineId)
            {
                UpdateDisplay();
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

            if (isUnlocked == false)
            {
                if (levelText != null)
                {
                    levelText.text = "Locked";
                }

                if (incomeText != null)
                {
                    incomeText.text = $"+{_state.BaseIncomePerSecond:F1}/s";
                }

                if (unlockButton != null)
                {
                    unlockButton.gameObject.SetActive(true);
                }

                if (upgradeButton != null)
                {
                    upgradeButton.gameObject.SetActive(false);
                }

                if (unlockCostText != null)
                {
                    unlockCostText.text = $"Unlock: {_state.UnlockCost}";
                }
            }
            else
            {
                if (levelText != null)
                {
                    levelText.text = $"Lvl {_state.Level}";
                }

                if (incomeText != null)
                {
                    incomeText.text = $"+{_state.CurrentIncomePerSecond:F1}/s";
                }

                if (unlockButton != null)
                {
                    unlockButton.gameObject.SetActive(false);
                }

                if (upgradeButton != null)
                {
                    upgradeButton.gameObject.SetActive(true);
                }

                if (upgradeCostText != null)
                {
                    upgradeCostText.text = $"Upgrade: {_state.CurrentUpgradeCost}";
                }
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
            GameplayEvents.MachineUnlocked -= OnMachineUnlocked;
            GameplayEvents.MachineUpgraded -= OnMachineUpgraded;

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

using MiniFactory.Gameplay;
using TMPro;
using UnityEngine;

namespace MiniFactory.UI
{
    public class WalletUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI coinsText;
        [SerializeField] private TextMeshProUGUI incomeRateText;

        private Wallet _wallet;

        public void Initialize(Wallet wallet)
        {
            _wallet = wallet;
            _wallet.CoinsChanged += OnCoinsChanged;
            OnCoinsChanged(_wallet.Coins);
        }

        public void SetIncomeRate(float rate, bool isBoosted)
        {
            if (incomeRateText != null)
            {
                incomeRateText.text = isBoosted ? $"+{rate:F1}/s (x2)" : $"+{rate:F1}/s";
            }
        }

        private void OnCoinsChanged(int newCoins)
        {
            if (coinsText != null)
            {
                coinsText.text = $"Coins: {newCoins}";
            }
        }

        private void OnDisable()
        {
            if (_wallet != null)
            {
                _wallet.CoinsChanged -= OnCoinsChanged;
            }
        }
    }
}

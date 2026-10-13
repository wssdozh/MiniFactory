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
                incomeRateText.SetText(isBoosted ? "+{0:F1}/s (x2)" : "+{0:F1}/s", rate);
        }

        private void OnCoinsChanged(int newCoins)
        {
            if (coinsText != null)
                coinsText.SetText("Coins: {0}", newCoins);
        }

        private void OnDisable()
        {
            if (_wallet != null)
                _wallet.CoinsChanged -= OnCoinsChanged;
        }
    }
}

using MiniFactory.Feedback;
using MiniFactory.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniFactory.UI
{
    public class ShopUI : MonoBehaviour
    {
        [SerializeField] private Button buyButton;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private TextMeshProUGUI statusText;

        private IStoreService _storeService;
        private string _productId;
        private int _coinReward;
        private FeedbackRoot _feedback;

        public void Initialize(IStoreService storeService, string productId, int coinReward, FeedbackRoot feedback = null)
        {
            _storeService = storeService;
            _productId = productId;
            _coinReward = coinReward;
            _feedback = feedback;

            if (_storeService != null)
            {
                _storeService.Initialized += OnStoreInitialized;
                _storeService.PurchaseSucceeded += OnPurchaseSucceeded;
                _storeService.PurchaseFailed += OnPurchaseFailed;
            }

            if (buyButton != null)
                buyButton.onClick.AddListener(OnBuyClicked);

            UpdateDisplay();
        }

        private void OnDisable()
        {
            if (_storeService != null)
            {
                _storeService.Initialized -= OnStoreInitialized;
                _storeService.PurchaseSucceeded -= OnPurchaseSucceeded;
                _storeService.PurchaseFailed -= OnPurchaseFailed;
            }

            if (buyButton != null)
                buyButton.onClick.RemoveListener(OnBuyClicked);
        }

        private void OnBuyClicked()
        {
            if (_feedback != null)
                _feedback.Play(FeedbackType.ButtonClick);

            if (_storeService != null && string.IsNullOrEmpty(_productId) == false)
            {
                if (statusText != null)
                    statusText.text = "Processing...";
                _storeService.Purchase(_productId);
            }
        }

        private void OnStoreInitialized()
        {
            UpdateDisplay();
        }

        private void OnPurchaseSucceeded(string productId)
        {
            if (productId == _productId && statusText != null)
                statusText.text = $"+{_coinReward} Coins!";
        }

        private void OnPurchaseFailed(string productId, string reason)
        {
            if (productId == _productId && statusText != null)
                statusText.text = "Failed";
        }

        private void UpdateDisplay()
        {
            bool ready = _storeService != null && _storeService.IsInitialized;

            if (buyButton != null)
                buyButton.interactable = ready;

            if (priceText != null)
            {
                string price = ready ? _storeService.GetProductPrice(_productId) : "$0.99";
                priceText.text = $"+{_coinReward}\n{price}";
            }

            if (statusText != null && ready == false)
                statusText.text = "Store Init...";
            else if (statusText != null && statusText.text == "Store Init...")
                statusText.text = string.Empty;
        }
    }
}

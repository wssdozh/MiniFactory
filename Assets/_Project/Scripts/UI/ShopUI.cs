using MiniFactory.Feedback;
using MiniFactory.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniFactory.UI
{
    public class ShopUI : MonoBehaviour
    {
        public enum ShopStatus { Initializing, Ready, Processing, Success, Failed }

        [SerializeField] private Button buyButton;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private TextMeshProUGUI statusText;

        private IStoreService _storeService;
        private string _productId;
        private int _coinReward;
        private FeedbackRoot _feedback;
        private ShopStatus _status = ShopStatus.Initializing;

        public ShopStatus Status => _status;

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

            SetStatus(_storeService != null && _storeService.IsInitialized ? ShopStatus.Ready : ShopStatus.Initializing);
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
                SetStatus(ShopStatus.Processing);
                _storeService.Purchase(_productId);
            }
        }

        private void OnStoreInitialized()
        {
            if (_status == ShopStatus.Initializing)
                SetStatus(ShopStatus.Ready);

            UpdateDisplay();
        }

        private void OnPurchaseSucceeded(string productId)
        {
            if (productId == _productId)
                SetStatus(ShopStatus.Success);
        }

        private void OnPurchaseFailed(string productId, string reason)
        {
            if (productId == _productId)
                SetStatus(ShopStatus.Failed);
        }

        private void SetStatus(ShopStatus status)
        {
            _status = status;
            if (statusText == null)
                return;

            statusText.text = status switch
            {
                ShopStatus.Initializing => "Store Init...",
                ShopStatus.Processing => "Processing...",
                ShopStatus.Success => $"+{_coinReward} Coins!",
                ShopStatus.Failed => "Failed",
                _ => string.Empty
            };
        }

        private void UpdateDisplay()
        {
            bool ready = _storeService != null && _storeService.IsInitialized;

            if (buyButton != null)
                buyButton.interactable = ready && _status != ShopStatus.Processing;

            if (priceText != null)
            {
                string price = ready ? _storeService.GetProductPrice(_productId) : "$0.99";
                priceText.text = $"+{_coinReward}\n{price}";
            }

            if (!ready && _status != ShopStatus.Processing)
                SetStatus(ShopStatus.Initializing);
            else if (ready && _status == ShopStatus.Initializing)
                SetStatus(ShopStatus.Ready);
        }
    }
}

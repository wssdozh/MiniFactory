using System.Collections;
using MiniFactory.Feedback;
using MiniFactory.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniFactory.UI
{
    public class ShopUI : MonoBehaviour
    {
        public enum ShopStatus { Initializing, Ready, Processing }

        private static readonly WaitForSeconds FeedbackWait = new WaitForSeconds(2f);

        [SerializeField] private Button buyButton;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private TextMeshProUGUI statusText;

        private IStoreService _storeService;
        private string _productId;
        private int _coinReward;
        private FeedbackRoot _feedback;
        private ShopStatus _status = ShopStatus.Initializing;
        private Coroutine _feedbackRoutine;

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
        }

        private void OnDisable()
        {
            StopFeedback();

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

        private void OnStoreInitialized() => SetStatus(ShopStatus.Ready);

        private void OnPurchaseSucceeded(string productId)
        {
            if (productId != _productId)
                return;

            SetStatus(ShopStatus.Ready);
            ShowFeedback($"+{_coinReward} Coins!");
        }

        private void OnPurchaseFailed(string productId, string reason)
        {
            if (productId != _productId)
                return;

            SetStatus(ShopStatus.Ready);
            ShowFeedback("Failed");
        }

        private void SetStatus(ShopStatus status)
        {
            _status = status;
            StopFeedback();

            bool ready = _storeService != null && _storeService.IsInitialized;

            if (buyButton != null)
                buyButton.interactable = ready && _status != ShopStatus.Processing;

            if (priceText != null)
                priceText.text = $"+{_coinReward}\n{(ready ? _storeService.GetProductPrice(_productId) : "$0.99")}";

            if (statusText != null)
            {
                statusText.text = status switch
                {
                    ShopStatus.Initializing => "Store Init...",
                    ShopStatus.Processing => "Processing...",
                    _ => string.Empty
                };
            }
        }

        private void ShowFeedback(string message)
        {
            if (statusText == null)
                return;

            StopFeedback();
            _feedbackRoutine = StartCoroutine(FeedbackRoutine(message));
        }

        private void StopFeedback()
        {
            if (_feedbackRoutine == null)
                return;

            StopCoroutine(_feedbackRoutine);
            _feedbackRoutine = null;
        }

        private IEnumerator FeedbackRoutine(string message)
        {
            statusText.text = message;
            yield return FeedbackWait;
            statusText.text = string.Empty;
            _feedbackRoutine = null;
        }
    }
}

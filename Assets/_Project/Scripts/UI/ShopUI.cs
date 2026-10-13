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
        public enum ShopStatus
        {
            Initializing,
            Ready,
            Processing
        }

        private const float FeedbackDisplayDuration = 2f;

        [SerializeField] private Button buyButton;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private TextMeshProUGUI statusText;

        private IStoreService _storeService;
        private string _productId;
        private int _coinReward;
        private FeedbackRoot _feedback;
        private ShopStatus _status = ShopStatus.Initializing;
        private Coroutine _feedbackCoroutine;

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
            {
                buyButton.onClick.AddListener(OnBuyClicked);
            }

            bool isStoreReady = _storeService != null && _storeService.IsInitialized;
            SetStatus(isStoreReady ? ShopStatus.Ready : ShopStatus.Initializing);
            UpdateDisplay();
        }

        private void OnDisable()
        {
            if (_feedbackCoroutine != null)
            {
                StopCoroutine(_feedbackCoroutine);
                _feedbackCoroutine = null;
            }

            if (_storeService != null)
            {
                _storeService.Initialized -= OnStoreInitialized;
                _storeService.PurchaseSucceeded -= OnPurchaseSucceeded;
                _storeService.PurchaseFailed -= OnPurchaseFailed;
            }

            if (buyButton != null)
            {
                buyButton.onClick.RemoveListener(OnBuyClicked);
            }
        }

        private void OnBuyClicked()
        {
            if (_feedback != null)
            {
                _feedback.Play(FeedbackType.ButtonClick);
            }

            if (_storeService != null && string.IsNullOrEmpty(_productId) == false)
            {
                SetStatus(ShopStatus.Processing);
                _storeService.Purchase(_productId);
            }
        }

        private void OnStoreInitialized()
        {
            if (_status == ShopStatus.Initializing)
            {
                SetStatus(ShopStatus.Ready);
            }

            UpdateDisplay();
        }

        private void OnPurchaseSucceeded(string productId)
        {
            if (productId != _productId)
            {
                return;
            }

            SetStatus(ShopStatus.Ready);
            ShowTemporaryMessage($"+{_coinReward} Coins!");
        }

        private void OnPurchaseFailed(string productId, string reason)
        {
            if (productId != _productId)
            {
                return;
            }

            SetStatus(ShopStatus.Ready);
            ShowTemporaryMessage("Failed");
        }

        private void SetStatus(ShopStatus status)
        {
            _status = status;

            if (_feedbackCoroutine != null)
            {
                StopCoroutine(_feedbackCoroutine);
                _feedbackCoroutine = null;
            }

            if (statusText != null)
            {
                statusText.text = status switch
                {
                    ShopStatus.Initializing => "Store Init...",
                    ShopStatus.Processing => "Processing...",
                    _ => string.Empty
                };
            }

            if (buyButton != null)
            {
                bool isStoreReady = _storeService != null && _storeService.IsInitialized;
                buyButton.interactable = isStoreReady && _status != ShopStatus.Processing;
            }
        }

        private void ShowTemporaryMessage(string message)
        {
            if (statusText == null)
            {
                return;
            }

            if (_feedbackCoroutine != null)
            {
                StopCoroutine(_feedbackCoroutine);
            }

            _feedbackCoroutine = StartCoroutine(ClearMessageRoutine(message));
        }

        private IEnumerator ClearMessageRoutine(string message)
        {
            statusText.text = message;
            yield return new WaitForSeconds(FeedbackDisplayDuration);
            statusText.text = string.Empty;
            _feedbackCoroutine = null;
        }

        private void UpdateDisplay()
        {
            bool isStoreReady = _storeService != null && _storeService.IsInitialized;

            if (buyButton != null)
            {
                buyButton.interactable = isStoreReady && _status != ShopStatus.Processing;
            }

            if (priceText != null)
            {
                string price = isStoreReady ? _storeService.GetProductPrice(_productId) : "$0.99";
                priceText.text = $"+{_coinReward}\n{price}";
            }

            if (isStoreReady == false && _status != ShopStatus.Processing)
            {
                SetStatus(ShopStatus.Initializing);
            }
            else if (isStoreReady && _status == ShopStatus.Initializing)
            {
                SetStatus(ShopStatus.Ready);
            }
        }
    }
}

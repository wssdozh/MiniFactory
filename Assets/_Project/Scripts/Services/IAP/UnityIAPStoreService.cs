using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;

namespace MiniFactory.Services
{
    public class UnityIAPStoreService : IStoreService
    {
        private readonly string[] _productIds;
        private readonly bool _forceFakeStore;
        private StoreController _storeController;

        public bool IsInitialized { get; private set; }
        public event Action Initialized;
        public event Action<string> PurchaseSucceeded;
        public event Action<string, string> PurchaseFailed;

        public UnityIAPStoreService(string[] productIds, bool forceFakeStore = true)
        {
            _productIds = productIds ?? Array.Empty<string>();
            _forceFakeStore = forceFakeStore;
        }

        public async void Initialize()
        {
            if (IsInitialized || _storeController != null)
                return;

            if (_forceFakeStore)
                DefaultStoreHelper.OverrideDefaultStoreName("fake");

            _storeController = UnityIAPServices.StoreController();

            _storeController.OnStoreConnected += OnStoreConnected;
            _storeController.OnStoreDisconnected += OnStoreDisconnected;
            _storeController.OnPurchasePending += OnPurchasePending;
            _storeController.OnPurchaseConfirmed += OnPurchaseConfirmed;
            _storeController.OnPurchaseFailed += OnPurchaseFailed;
            _storeController.OnPurchaseDeferred += OnPurchaseDeferred;

            try
            {
                await _storeController.Connect();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IAP] Failed to connect store: {ex.Message}");
                PurchaseFailed?.Invoke(string.Empty, ex.Message);
            }
        }

        private void OnStoreConnected()
        {
            List<ProductDefinition> definitions = _productIds
                .Select(id => new ProductDefinition(id, ProductType.Consumable))
                .ToList();

            _storeController.OnProductsFetched += OnProductsFetched;
            _storeController.OnProductsFetchFailed += OnProductsFetchFailed;
            _storeController.FetchProducts(definitions);
        }

        private void OnStoreDisconnected(StoreConnectionFailureDescription description)
        {
            Debug.LogWarning($"[IAP] Store disconnected: {description?.Message}");
            IsInitialized = false;
        }

        private void OnProductsFetched(List<Product> products)
        {
            IsInitialized = true;
            Debug.Log($"[IAP] Successfully initialized with {products.Count} products.");
            Initialized?.Invoke();
        }

        private void OnProductsFetchFailed(ProductFetchFailed failed)
        {
            Debug.LogError($"[IAP] Product fetch failed: {failed.FailureReason}");
            IsInitialized = false;
        }

        private void OnPurchasePending(PendingOrder pendingOrder)
        {
            string productId = pendingOrder.CartOrdered.Items().FirstOrDefault()?.Product.definition.id;
            _storeController.ConfirmPurchase(pendingOrder);

            if (string.IsNullOrEmpty(productId) == false)
            {
                Debug.Log($"[IAP] Purchase pending resolved and confirmed: {productId}");
                PurchaseSucceeded?.Invoke(productId);
            }
        }

        private void OnPurchaseConfirmed(Order order)
        {
            if (order is FailedOrder failedOrder)
                Debug.LogError($"[IAP] Order confirmation failed: {failedOrder.FailureReason} - {failedOrder.Details}");
        }

        private void OnPurchaseFailed(FailedOrder failedOrder)
        {
            string productId = failedOrder.CartOrdered?.Items()?.FirstOrDefault()?.Product?.definition?.id ?? string.Empty;
            string reason = $"{failedOrder.FailureReason}: {failedOrder.Details}";
            if (reason.Contains("UserCancelled"))
                Debug.LogWarning($"[IAP] Purchase cancelled by user: {productId} ({reason})");
            else
                Debug.LogError($"[IAP] Purchase failed for {productId}: {reason}");

            PurchaseFailed?.Invoke(productId, reason);
        }

        private void OnPurchaseDeferred(DeferredOrder deferredOrder)
        {
            Debug.Log("[IAP] Purchase is deferred (Ask-to-Buy / parental approval).");
        }

        public void Purchase(string productId)
        {
            if (IsInitialized == false || _storeController == null)
            {
                Debug.LogWarning($"[IAP] Cannot purchase {productId}: store not initialized.");
                PurchaseFailed?.Invoke(productId, "Store not initialized");
                return;
            }

            Product product = _storeController.GetProductById(productId);
            if (product != null)
                _storeController.PurchaseProduct(product);
            else
                _storeController.PurchaseProduct(productId);
        }

        public string GetProductPrice(string productId)
        {
            if (IsInitialized == false || _storeController == null)
                return "$0.99";

            Product product = _storeController.GetProductById(productId);
            return product?.metadata?.localizedPriceString ?? "$0.99";
        }
    }
}

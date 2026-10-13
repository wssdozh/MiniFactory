using System;

namespace MiniFactory.Services
{
    public interface IStoreService
    {
        bool IsInitialized { get; }
        event Action Initialized;
        event Action<string> PurchaseSucceeded;
        event Action<string, string> PurchaseFailed;

        void Initialize();
        void Purchase(string productId);
        string GetProductPrice(string productId);
    }
}

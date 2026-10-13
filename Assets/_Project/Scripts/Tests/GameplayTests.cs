using System.Collections.Generic;
using NUnit.Framework;
using MiniFactory.Gameplay;
using MiniFactory.Services;
using MiniFactory.UI;
using UnityEngine;

namespace MiniFactory.Tests
{
    [TestFixture]
    public class GameplayTests
    {
        private MachineConfig CreateDummyConfig(string id, int unlockCost, float baseIncome, int baseUpgradeCost)
        {
            MachineConfig config = ScriptableObject.CreateInstance<MachineConfig>();
            config.MachineId = id;
            config.DisplayName = id;
            config.UnlockCost = unlockCost;
            config.BaseIncomePerSecond = baseIncome;
            config.BaseUpgradeCost = baseUpgradeCost;
            config.UpgradeCostMultiplier = 1.5f;
            config.IncomePerLevelMultiplier = 1.2f;
            return config;
        }

        [Test]
        public void Wallet_TrySpend_RejectsWhenInsufficientCoins()
        {
            Wallet wallet = new Wallet(10);
            bool spent = wallet.TrySpend(50);

            Assert.IsFalse(spent);
            Assert.AreEqual(10, wallet.Coins);
        }

        [Test]
        public void Wallet_TrySpend_SucceedsAndDeductsCoins()
        {
            Wallet wallet = new Wallet(50);
            bool spent = wallet.TrySpend(30);

            Assert.IsTrue(spent);
            Assert.AreEqual(20, wallet.Coins);
        }

        [Test]
        public void MachineState_Unlock_StartsAtLevel1WithBaseIncome()
        {
            MachineConfig config = CreateDummyConfig("TestMachine", 50, 10f, 20);
            MachineState machine = new MachineState(config, unlocked: false, level: 0);

            Assert.IsFalse(machine.IsUnlocked);
            Assert.AreEqual(0, machine.Level);
            Assert.AreEqual(0f, machine.CurrentIncomePerSecond);

            machine.Unlock();

            Assert.IsTrue(machine.IsUnlocked);
            Assert.AreEqual(1, machine.Level);
            Assert.AreEqual(10f, machine.CurrentIncomePerSecond);
        }

        [Test]
        public void MachineState_Upgrade_IncreasesIncomeAndCost()
        {
            MachineConfig config = CreateDummyConfig("TestMachine", 0, 10f, 20);
            MachineState machine = new MachineState(config, unlocked: true, level: 1);

            int initialCost = machine.CurrentUpgradeCost;
            float initialIncome = machine.CurrentIncomePerSecond;

            machine.Upgrade();

            Assert.AreEqual(2, machine.Level);
            Assert.Greater(machine.CurrentUpgradeCost, initialCost);
            Assert.Greater(machine.CurrentIncomePerSecond, initialIncome);
        }

        [Test]
        public void OfflineProgressCalculator_CapsAtMaxSeconds()
        {
            float incomePerSec = 5f;
            float elapsedSeconds = 10000f;
            float maxOfflineSeconds = 7200f;

            float offlineIncome = OfflineProgressCalculator.CalculateOfflineIncome(
                incomePerSec, elapsedSeconds, maxOfflineSeconds, null);

            Assert.AreEqual(incomePerSec * maxOfflineSeconds, offlineIncome);
        }

        [Test]
        public void OfflineProgressCalculator_AppliesBoostMultiplier()
        {
            float incomePerSec = 10f;
            float elapsedSeconds = 100f;
            float maxOfflineSeconds = 7200f;

            BoostState boost = new BoostState();
            boost.Activate(duration: 30f, multiplier: 2f);

            float offlineIncome = OfflineProgressCalculator.CalculateOfflineIncome(
                incomePerSec, elapsedSeconds, maxOfflineSeconds, boost);

            Assert.AreEqual(1300f, offlineIncome);
        }

        [Test]
        public void BoostState_TicksAndExpiresProperly()
        {
            BoostState boost = new BoostState();
            boost.Activate(duration: 10f, multiplier: 2f);

            Assert.IsTrue(boost.IsActive);
            Assert.AreEqual(2f, boost.Multiplier);

            boost.Tick(5f);
            Assert.IsTrue(boost.IsActive);
            Assert.AreEqual(5f, boost.RemainingSeconds);

            boost.Tick(6f);
            Assert.IsFalse(boost.IsActive);
            Assert.AreEqual(1f, boost.Multiplier);
            Assert.AreEqual(0f, boost.RemainingSeconds);
        }

        [Test]
        public void BoostState_FiresStartedAndFinishedEvents()
        {
            BoostState boost = new BoostState();
            bool startedFired = false;
            float startedDuration = 0f;
            float startedMultiplier = 0f;
            bool finishedFired = false;

            boost.BoostStarted += (dur, mul) =>
            {
                startedFired = true;
                startedDuration = dur;
                startedMultiplier = mul;
            };

            boost.BoostFinished += () =>
            {
                finishedFired = true;
            };

            boost.Activate(15f, 3f);
            Assert.IsTrue(startedFired);
            Assert.AreEqual(15f, startedDuration);
            Assert.AreEqual(3f, startedMultiplier);
            Assert.IsFalse(finishedFired);

            boost.Tick(16f);
            Assert.IsTrue(finishedFired);
        }

        [Test]
        public void AnalyticsService_DispatchesToMultipleProviders()
        {
            MockAnalyticsProvider providerA = new MockAnalyticsProvider();
            MockAnalyticsProvider providerB = new MockAnalyticsProvider();
            AnalyticsService service = new AnalyticsService(providerA, providerB);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "machine_id", "drill" },
                { "level", 2 }
            };

            service.LogEvent("machine_upgraded", parameters);

            Assert.AreEqual(1, providerA.LoggedEvents.Count);
            Assert.AreEqual("machine_upgraded", providerA.LoggedEvents[0].Name);
            Assert.AreEqual("drill", providerA.LoggedEvents[0].Params["machine_id"]);

            Assert.AreEqual(1, providerB.LoggedEvents.Count);
            Assert.AreEqual("machine_upgraded", providerB.LoggedEvents[0].Name);
            Assert.AreEqual(2, providerB.LoggedEvents[0].Params["level"]);
        }

        [Test]
        public void Store_SimulatedPurchaseSucceeded_CreditsWallet()
        {
            Wallet wallet = new Wallet(100);
            int rewardAmount = 500;

            wallet.Add(rewardAmount);

            Assert.AreEqual(600, wallet.Coins);
        }

        [Test]
        public void ShopUI_StatusReflectsStoreLifecycle()
        {
            GameObject go = new GameObject();
            ShopUI shop = go.AddComponent<ShopUI>();
            MockStoreService store = new MockStoreService(isInitialized: false);

            shop.Initialize(store, "coins_pack", 500);
            Assert.AreEqual(ShopUI.ShopStatus.Initializing, shop.Status);

            store.SimulateInitialized();
            Assert.AreEqual(ShopUI.ShopStatus.Ready, shop.Status);

            store.SimulatePurchaseSucceeded("coins_pack");
            Assert.AreEqual(ShopUI.ShopStatus.Success, shop.Status);

            store.SimulatePurchaseFailed("coins_pack", "Failed");
            Assert.AreEqual(ShopUI.ShopStatus.Failed, shop.Status);

            Object.DestroyImmediate(go);
        }

        private class MockStoreService : IStoreService
        {
            public bool IsInitialized { get; private set; }
            public event System.Action Initialized;
            public event System.Action<string> PurchaseSucceeded;
            public event System.Action<string, string> PurchaseFailed;

            public MockStoreService(bool isInitialized)
            {
                IsInitialized = isInitialized;
            }

            public void Initialize() { }
            public void Purchase(string productId) { }
            public string GetProductPrice(string productId) => "$0.99";

            public void SimulateInitialized()
            {
                IsInitialized = true;
                Initialized?.Invoke();
            }

            public void SimulatePurchaseSucceeded(string productId)
            {
                PurchaseSucceeded?.Invoke(productId);
            }

            public void SimulatePurchaseFailed(string productId, string reason)
            {
                PurchaseFailed?.Invoke(productId, reason);
            }
        }

        private class MockAnalyticsProvider : IAnalyticsProvider
        {
            public class EventRecord
            {
                public string Name;
                public Dictionary<string, object> Params;
            }

            public List<EventRecord> LoggedEvents { get; } = new();

            public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
            {
                LoggedEvents.Add(new EventRecord { Name = eventName, Params = parameters });
            }
        }
    }
}

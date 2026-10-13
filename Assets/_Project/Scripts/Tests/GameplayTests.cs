using NUnit.Framework;
using MiniFactory.Gameplay;
using UnityEngine;

namespace MiniFactory.Tests
{
    [TestFixture]
    public class GameplayTests
    {
        private MachineConfig CreateDummyConfig(string id, int unlockCost, float baseIncome, int baseUpgradeCost)
        {
            var config = ScriptableObject.CreateInstance<MachineConfig>();
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
            var wallet = new Wallet(10);
            bool spent = wallet.TrySpend(50);

            Assert.IsFalse(spent);
            Assert.AreEqual(10, wallet.Coins);
        }

        [Test]
        public void Wallet_TrySpend_SucceedsAndDeductsCoins()
        {
            var wallet = new Wallet(50);
            bool spent = wallet.TrySpend(30);

            Assert.IsTrue(spent);
            Assert.AreEqual(20, wallet.Coins);
        }

        [Test]
        public void MachineState_Unlock_StartsAtLevel1WithBaseIncome()
        {
            var config = CreateDummyConfig("TestMachine", 50, 10f, 20);
            var machine = new MachineState(config, unlocked: false, level: 0);

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
            var config = CreateDummyConfig("TestMachine", 0, 10f, 20);
            var machine = new MachineState(config, unlocked: true, level: 1);

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

            var boost = new BoostState();
            boost.Activate(duration: 30f, multiplier: 2f);

            float offlineIncome = OfflineProgressCalculator.CalculateOfflineIncome(
                incomePerSec, elapsedSeconds, maxOfflineSeconds, boost);

            // 30 seconds at 2x (600) + 70 seconds at 1x (700) = 1300
            Assert.AreEqual(1300f, offlineIncome);
        }

        [Test]
        public void BoostState_TicksAndExpiresProperly()
        {
            var boost = new BoostState();
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
    }
}

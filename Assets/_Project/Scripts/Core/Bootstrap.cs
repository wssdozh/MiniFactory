using System;
using System.Collections.Generic;
using System.Linq;
using MiniFactory.Feedback;
using MiniFactory.Gameplay;
using MiniFactory.Services;
using MiniFactory.UI;
using UnityEngine;

namespace MiniFactory.Core
{
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private GameConfig gameConfig;
        [SerializeField] private MachineConfig[] machineConfigs;
        [SerializeField] private WalletUI walletUI;
        [SerializeField] private MachineCardUI[] machineCards;
        [SerializeField] private BoostUI boostUI;
        [SerializeField] private ShopUI shopUI;
        [SerializeField] private MachineWorldView[] machineWorldViews;
        [SerializeField] private FeedbackRoot feedbackRoot;

        private SaveService _saveService;
        private Wallet _wallet;
        private MachineCollection _machines;
        private BoostState _boostState;
        private IConfigService _configService;
        private IAnalyticsService _analyticsService;
        private IStoreService _storeService;
        private float _fractionalCoins;
        private string _coinsPackId;
        private int _coinsPackReward;

        public Wallet Wallet => _wallet;
        public MachineCollection Machines => _machines;
        public BoostState BoostState => _boostState;
        public IConfigService ConfigService => _configService;
        public IAnalyticsService AnalyticsService => _analyticsService;
        public IStoreService StoreService => _storeService;

        private void Awake()
        {
            _configService = gameConfig;
            _saveService = new SaveService();
            _analyticsService = new AnalyticsService(new DebugAnalyticsProvider());
            SaveData save = _saveService.Load();

            MachineConfig[] configs = (machineConfigs != null && machineConfigs.Length > 0)
                ? machineConfigs
                : (gameConfig != null ? gameConfig.MachineConfigs : Array.Empty<MachineConfig>());

            _wallet = new Wallet(save.coins);
            _machines = new MachineCollection(configs, save.machines);
            _boostState = new BoostState(save.boost);

            _coinsPackId = _configService != null ? _configService.CoinsPackSmallId : "coins_pack_small";
            _coinsPackReward = _configService != null ? _configService.CoinsPackSmallReward : 500;
            _storeService = new UnityIAPStoreService(new[] { _coinsPackId }, forceFakeStore: true);

            _storeService.PurchaseSucceeded += OnPurchaseSucceeded;
            _storeService.PurchaseFailed += OnPurchaseFailed;
            _storeService.Initialize();

            GameplayEvents.MachineUnlocked += OnMachineUnlocked;
            GameplayEvents.MachineUpgraded += OnMachineUpgraded;
            _boostState.BoostStarted += OnBoostStarted;
            _boostState.BoostFinished += OnBoostFinished;

            if (save.lastSaveTimestamp > 0)
            {
                float elapsed = (DateTime.UtcNow.Ticks - save.lastSaveTimestamp) / (float)TimeSpan.TicksPerSecond;
                float totalIncome = _machines.TotalIncomePerSecond;
                float maxOfflineSeconds = _configService != null ? _configService.MaxOfflineProductionSeconds : 7200f;
                float offlineIncome = OfflineProgressCalculator.CalculateOfflineIncome(
                    totalIncome, elapsed, maxOfflineSeconds, _boostState);

                _wallet.Add((int)offlineIncome);

                _analyticsService.LogEvent("offline_income_applied", new Dictionary<string, object>
                {
                    { "offline_seconds", Mathf.Min(elapsed, maxOfflineSeconds) },
                    { "offline_income", (int)offlineIncome }
                });
            }

            _analyticsService.LogEvent("game_started", new Dictionary<string, object>
            {
                { "coins", _wallet.Coins },
                { "unlocked_machines_count", _machines.AllMachines.Count(m => m.IsUnlocked) }
            });

            if (walletUI != null)
                walletUI.Initialize(_wallet);

            if (boostUI != null)
                boostUI.Initialize(_boostState, _configService, feedbackRoot);

            if (shopUI != null)
                shopUI.Initialize(_storeService, _coinsPackId, _coinsPackReward, feedbackRoot);

            List<MachineState> machineStates = _machines.AllMachines.ToList();
            if (machineCards != null)
            {
                for (int i = 0; i < machineCards.Length && i < machineStates.Count; i++)
                {
                    if (machineCards[i] != null)
                        machineCards[i].Initialize(machineStates[i], _wallet, _machines, feedbackRoot);
                }
            }

            if (machineWorldViews != null)
            {
                for (int i = 0; i < machineWorldViews.Length && i < machineStates.Count; i++)
                {
                    if (machineWorldViews[i] != null)
                        machineWorldViews[i].Initialize(machineStates[i]);
                }
            }
        }

        private void Update()
        {
            if (_boostState.IsActive)
                _boostState.Tick(Time.deltaTime);

            float multiplier = _boostState.IsActive ? _boostState.Multiplier : 1f;
            float currentRate = _machines.TotalIncomePerSecond * multiplier;

            _fractionalCoins += currentRate * Time.deltaTime;
            if (_fractionalCoins >= 1f)
            {
                int coinsToAdd = (int)_fractionalCoins;
                _fractionalCoins -= coinsToAdd;
                _wallet.Add(coinsToAdd);
            }

            if (walletUI != null)
                walletUI.SetIncomeRate(currentRate, _boostState.IsActive);
        }

        private void OnDestroy()
        {
            if (_storeService != null)
            {
                _storeService.PurchaseSucceeded -= OnPurchaseSucceeded;
                _storeService.PurchaseFailed -= OnPurchaseFailed;
            }

            GameplayEvents.MachineUnlocked -= OnMachineUnlocked;
            GameplayEvents.MachineUpgraded -= OnMachineUpgraded;

            if (_boostState != null)
            {
                _boostState.BoostStarted -= OnBoostStarted;
                _boostState.BoostFinished -= OnBoostFinished;
            }
        }

        private void OnPurchaseSucceeded(string id)
        {
            if (id != _coinsPackId)
                return;

            _wallet.Add(_coinsPackReward);
            SaveGame();

            if (feedbackRoot != null)
                feedbackRoot.Play(FeedbackType.PurchaseSuccess);

            _analyticsService.LogEvent("purchase_succeeded", new Dictionary<string, object>
            {
                { "product_id", id },
                { "coins_reward", _coinsPackReward }
            });
        }

        private void OnPurchaseFailed(string id, string reason)
        {
            _analyticsService.LogEvent("purchase_failed", new Dictionary<string, object>
            {
                { "product_id", id },
                { "reason", reason }
            });
        }

        private void OnMachineUnlocked(string id)
        {
            if (feedbackRoot != null)
                feedbackRoot.Play(FeedbackType.MachineUnlock);

            _analyticsService.LogEvent("machine_unlocked", new Dictionary<string, object>
            {
                { "machine_id", id }
            });
        }

        private void OnMachineUpgraded(string id, int level)
        {
            if (feedbackRoot != null)
                feedbackRoot.Play(FeedbackType.MachineUpgrade);

            _analyticsService.LogEvent("machine_upgraded", new Dictionary<string, object>
            {
                { "machine_id", id },
                { "new_level", level }
            });
        }

        private void OnBoostStarted(float duration, float multiplier)
        {
            if (feedbackRoot != null)
                feedbackRoot.Play(FeedbackType.BoostActive);

            _analyticsService.LogEvent("boost_started", new Dictionary<string, object>
            {
                { "duration", duration },
                { "multiplier", multiplier }
            });
        }

        private void OnBoostFinished()
        {
            _analyticsService.LogEvent("boost_finished");
        }

        private void OnApplicationQuit()
        {
            SaveGame();
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
                SaveGame();
        }

        private void SaveGame()
        {
            SaveData save = new SaveData
            {
                coins = _wallet.Coins,
                lastSaveTimestamp = DateTime.UtcNow.Ticks,
                machines = _machines.AllMachines.Select(m => new SavedMachine
                {
                    id = m.MachineId,
                    unlocked = m.IsUnlocked,
                    level = m.Level
                }).ToList(),
                boost = new SavedBoost
                {
                    active = _boostState.IsActive,
                    remainingSeconds = _boostState.RemainingSeconds,
                    multiplier = _boostState.Multiplier
                }
            };
            _saveService.Save(save);
        }
    }
}

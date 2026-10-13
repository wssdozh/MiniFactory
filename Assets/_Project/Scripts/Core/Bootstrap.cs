using System;
using System.Linq;
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
        [SerializeField] private MachineWorldView[] machineWorldViews;

        private SaveService _saveService;
        private Wallet _wallet;
        private MachineCollection _machines;
        private BoostState _boostState;
        private IConfigService _configService;
        private float _fractionalCoins;

        public Wallet Wallet => _wallet;
        public MachineCollection Machines => _machines;
        public BoostState BoostState => _boostState;
        public IConfigService ConfigService => _configService;

        private void Awake()
        {
            _configService = gameConfig;
            _saveService = new SaveService();
            SaveData save = _saveService.Load();

            var configs = (machineConfigs != null && machineConfigs.Length > 0)
                ? machineConfigs
                : (gameConfig != null ? gameConfig.MachineConfigs : Array.Empty<MachineConfig>());

            _wallet = new Wallet(save.coins);
            _machines = new MachineCollection(configs, save.machines);
            _boostState = new BoostState(save.boost);

            if (save.lastSaveTimestamp > 0)
            {
                float elapsed = (DateTime.UtcNow.Ticks - save.lastSaveTimestamp) / (float)TimeSpan.TicksPerSecond;
                float totalIncome = _machines.TotalIncomePerSecond;
                float maxOfflineSeconds = _configService != null ? _configService.MaxOfflineProductionSeconds : 7200f;
                float offlineIncome = OfflineProgressCalculator.CalculateOfflineIncome(
                    totalIncome, elapsed, maxOfflineSeconds, _boostState);

                _wallet.Add((int)offlineIncome);
            }

            if (walletUI != null)
            {
                walletUI.Initialize(_wallet);
            }

            if (boostUI != null)
            {
                boostUI.Initialize(_boostState, _configService);
            }

            var machineStates = _machines.AllMachines.ToList();
            if (machineCards != null)
            {
                for (int i = 0; i < machineCards.Length && i < machineStates.Count; i++)
                {
                    if (machineCards[i] != null)
                    {
                        machineCards[i].Initialize(machineStates[i], _wallet, _machines);
                    }
                }
            }

            if (machineWorldViews != null)
            {
                for (int i = 0; i < machineWorldViews.Length && i < machineStates.Count; i++)
                {
                    if (machineWorldViews[i] != null)
                    {
                        machineWorldViews[i].Initialize(machineStates[i]);
                    }
                }
            }
        }

        private void Update()
        {
            if (_boostState.IsActive)
            {
                _boostState.Tick(Time.deltaTime);
            }

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
            {
                walletUI.SetIncomeRate(currentRate, _boostState.IsActive);
            }
        }

        private void OnApplicationQuit() => SaveGame();
        private void OnApplicationPause(bool pause) { if (pause) SaveGame(); }

        private void SaveGame()
        {
            var save = new SaveData
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

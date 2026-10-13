using System;
using System.Linq;
using MiniFactory.Gameplay;
using MiniFactory.Services;
using UnityEngine;

namespace MiniFactory.Core
{
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private MachineConfig[] machineConfigs;

        private SaveService _saveService;
        private Wallet _wallet;
        private MachineCollection _machines;
        private BoostState _boostState;

        public Wallet Wallet => _wallet;
        public MachineCollection Machines => _machines;
        public BoostState BoostState => _boostState;

        private void Awake()
        {
            _saveService = new SaveService();

            SaveData save = _saveService.Load();

            _wallet = new Wallet(save.coins);
            _machines = new MachineCollection(machineConfigs, save.machines);
            _boostState = new BoostState(save.boost);

            // Offline progress
            if (save.lastSaveTimestamp > 0)
            {
                float elapsed = (DateTime.UtcNow.Ticks - save.lastSaveTimestamp) / (float)TimeSpan.TicksPerSecond;
                float totalIncome = _machines.AllMachines.Sum(m => m.CurrentIncomePerSecond);
                float offlineIncome = OfflineProgressCalculator.CalculateOfflineIncome(
                    totalIncome, elapsed, maxOfflineSeconds: 7200f, _boostState);

                _wallet.Add((int)offlineIncome);
                Debug.Log($"[Bootstrap] Offline income: {offlineIncome:F0} coins for {elapsed:F0}s");
            }

            Debug.Log("[Bootstrap] Initialized. Ready for gameplay.");
        }

        private void Update()
        {
            if (_boostState.IsActive)
                _boostState.Tick(Time.deltaTime);
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

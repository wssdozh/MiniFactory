using System.Collections.Generic;
using System.Linq;
using MiniFactory.Services;

namespace MiniFactory.Gameplay
{
    public class MachineCollection
    {
        private readonly Dictionary<string, MachineState> _machines = new();
        private readonly Dictionary<string, MachineConfig> _configs = new();

        public IEnumerable<MachineState> AllMachines => _machines.Values;

        public float TotalIncomePerSecond
        {
            get
            {
                float total = 0f;
                foreach (var machine in _machines.Values)
                {
                    total += machine.CurrentIncomePerSecond;
                }
                return total;
            }
        }

        public MachineCollection(MachineConfig[] configs, List<SavedMachine> savedStates = null)
        {
            foreach (var config in configs)
            {
                _configs[config.MachineId] = config;
                
                var saved = savedStates?.FirstOrDefault(s => s.id == config.MachineId);
                var state = new MachineState(
                    config,
                    saved?.unlocked ?? (config.UnlockCost == 0),
                    saved?.level ?? 0
                );
                _machines[config.MachineId] = state;
            }
        }

        public MachineState GetMachine(string id)
        {
            return _machines.TryGetValue(id, out var machine) ? machine : null;
        }

        public bool TryUnlock(string id, Wallet wallet)
        {
            var machine = GetMachine(id);
            if (machine == null || machine.IsUnlocked) return false;

            if (!_configs.TryGetValue(id, out var config)) return false;
            if (!wallet.TrySpend(config.UnlockCost)) return false;

            machine.Unlock();
            GameplayEvents.RaiseMachineUnlocked(id);
            return true;
        }

        public bool TryUpgrade(string id, Wallet wallet)
        {
            var machine = GetMachine(id);
            if (machine == null || !machine.IsUnlocked) return false;

            if (!wallet.TrySpend(machine.CurrentUpgradeCost)) return false;

            machine.Upgrade();
            GameplayEvents.RaiseMachineUpgraded(id, machine.Level);
            return true;
        }
    }
}

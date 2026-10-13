using System.Collections.Generic;
using System.Linq;
using MiniFactory.Services;

namespace MiniFactory.Gameplay
{
    public class MachineCollection
    {
        private readonly Dictionary<string, MachineState> _machines = new();

        public IEnumerable<MachineState> AllMachines => _machines.Values;

        public float TotalIncomePerSecond
        {
            get
            {
                float total = 0f;
                foreach (MachineState machine in _machines.Values)
                {
                    total += machine.CurrentIncomePerSecond;
                }
                return total;
            }
        }

        public MachineCollection(MachineConfig[] configs, List<SavedMachine> savedStates = null)
        {
            foreach (MachineConfig config in configs)
            {
                SavedMachine saved = savedStates?.FirstOrDefault(s => s.id == config.MachineId);
                MachineState state = new MachineState(
                    config,
                    saved?.unlocked ?? (config.UnlockCost == 0),
                    saved?.level ?? 0
                );
                _machines[config.MachineId] = state;
            }
        }

        public MachineState GetMachine(string id)
        {
            return _machines.TryGetValue(id, out MachineState machine) ? machine : null;
        }

        public bool TryUnlock(string id, Wallet wallet)
        {
            MachineState machine = GetMachine(id);
            if (machine == null || machine.IsUnlocked)
                return false;

            if (wallet.TrySpend(machine.UnlockCost) == false)
                return false;

            machine.Unlock();
            GameplayEvents.RaiseMachineUnlocked(id);
            return true;
        }

        public bool TryUpgrade(string id, Wallet wallet)
        {
            MachineState machine = GetMachine(id);
            if (machine == null || machine.IsUnlocked == false)
                return false;

            if (wallet.TrySpend(machine.CurrentUpgradeCost) == false)
                return false;

            machine.Upgrade();
            GameplayEvents.RaiseMachineUpgraded(id, machine.Level);
            return true;
        }
    }
}

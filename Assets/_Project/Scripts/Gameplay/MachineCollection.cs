using System;
using System.Collections.Generic;
using System.Linq;
using MiniFactory.Services;

namespace MiniFactory.Gameplay
{
    public class MachineCollection
    {
        private readonly Dictionary<string, MachineState> _machines = new();
        private readonly List<MachineState> _allMachines = new();

        public IReadOnlyList<MachineState> AllMachines => _allMachines;

        public event Action<string> MachineUnlocked;
        public event Action<string, int> MachineUpgraded;

        public float TotalIncomePerSecond
        {
            get
            {
                float total = 0f;
                for (int i = 0; i < _allMachines.Count; i++)
                    total += _allMachines[i].CurrentIncomePerSecond;

                return total;
            }
        }

        public MachineCollection(MachineConfig[] configs, List<SavedMachine> savedStates = null)
        {
            if (configs == null)
                return;

            foreach (MachineConfig config in configs)
            {
                SavedMachine saved = savedStates?.FirstOrDefault(s => s.id == config.MachineId);
                MachineState state = new MachineState(
                    config,
                    saved?.unlocked ?? (config.UnlockCost == 0),
                    saved?.level ?? 0
                );
                _machines[config.MachineId] = state;
                _allMachines.Add(state);
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
            MachineUnlocked?.Invoke(id);
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
            MachineUpgraded?.Invoke(id, machine.Level);
            return true;
        }
    }
}

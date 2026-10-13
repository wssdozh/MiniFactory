using System;
using UnityEngine;

namespace MiniFactory.Gameplay
{
    public class MachineState
    {
        public string MachineId { get; }
        public bool IsUnlocked { get; private set; }
        public int Level { get; private set; }

        public string DisplayName => _config.DisplayName;
        public Sprite Icon => _config.Icon;
        public int UnlockCost => _config.UnlockCost;
        public float BaseIncomePerSecond => _config.BaseIncomePerSecond;

        public event Action Changed;

        private readonly MachineConfig _config;

        public float CurrentIncomePerSecond
        {
            get
            {
                if (IsUnlocked == false)
                {
                    return 0f;
                }

                return _config.BaseIncomePerSecond * Mathf.Pow(_config.IncomePerLevelMultiplier, Level - 1);
            }
        }

        public int CurrentUpgradeCost
        {
            get
            {
                if (IsUnlocked == false)
                {
                    return 0;
                }

                return (int)(_config.BaseUpgradeCost * Mathf.Pow(_config.UpgradeCostMultiplier, Level - 1));
            }
        }

        public MachineState(MachineConfig config, bool unlocked = false, int level = 0)
        {
            _config = config;
            MachineId = config.MachineId;
            IsUnlocked = unlocked;
            Level = unlocked ? Math.Max(1, level) : 0;
        }

        public void Unlock()
        {
            if (IsUnlocked)
            {
                return;
            }

            IsUnlocked = true;
            Level = 1;
            Changed?.Invoke();
        }

        public void Upgrade()
        {
            if (IsUnlocked == false)
            {
                return;
            }

            Level++;
            Changed?.Invoke();
        }
    }
}

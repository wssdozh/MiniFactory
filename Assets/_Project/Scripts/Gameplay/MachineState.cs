namespace MiniFactory.Gameplay
{
    public class MachineState
    {
        public string MachineId { get; }
        public bool IsUnlocked { get; private set; }
        public int Level { get; private set; }

        public string DisplayName => _config.DisplayName;
        public UnityEngine.Sprite Icon => _config.Icon;
        public int UnlockCost => _config.UnlockCost;
        public float BaseIncomePerSecond => _config.BaseIncomePerSecond;

        private readonly MachineConfig _config;

        public float CurrentIncomePerSecond
        {
            get
            {
                if (IsUnlocked == false)
                {
                    return 0f;
                }

                float income = _config.BaseIncomePerSecond;
                for (int i = 1; i < Level; i++)
                {
                    income *= _config.IncomePerLevelMultiplier;
                }

                return income;
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

                float cost = _config.BaseUpgradeCost;
                for (int i = 1; i < Level; i++)
                {
                    cost *= _config.UpgradeCostMultiplier;
                }

                return (int)cost;
            }
        }

        public MachineState(MachineConfig config, bool unlocked = false, int level = 0)
        {
            _config = config;
            MachineId = config.MachineId;
            IsUnlocked = unlocked;
            Level = unlocked ? System.Math.Max(1, level) : 0;
        }

        public void Unlock()
        {
            if (IsUnlocked)
            {
                return;
            }

            IsUnlocked = true;
            Level = 1;
        }

        public void Upgrade()
        {
            if (IsUnlocked == false)
            {
                return;
            }

            Level++;
        }
    }
}

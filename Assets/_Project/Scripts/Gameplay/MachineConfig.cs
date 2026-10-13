using UnityEngine;

namespace MiniFactory.Gameplay
{
    [CreateAssetMenu(fileName = "MachineConfig", menuName = "MiniFactory/Machine Config")]
    public class MachineConfig : ScriptableObject
    {
        public string MachineId;
        public string DisplayName;
        public Sprite Icon;
        public int UnlockCost;
        public float BaseIncomePerSecond;
        public int BaseUpgradeCost;
        public float UpgradeCostMultiplier = 1.5f;
        public float IncomePerLevelMultiplier = 1.2f;
    }
}

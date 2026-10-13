using UnityEngine;
using MiniFactory.Gameplay;

namespace MiniFactory.Services
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "MiniFactory/Game Config")]
    public class GameConfig : ScriptableObject, IConfigService
    {
        [Header("Machines")]
        [SerializeField] private MachineConfig[] machineConfigs;

        [Header("Boost Settings")]
        [SerializeField] private float boostDuration = 30f;
        [SerializeField] private float boostMultiplier = 2f;
        [SerializeField] private bool isBoostEnabled = true;

        [Header("Offline Progress Settings")]
        [SerializeField] private float maxOfflineProductionSeconds = 7200f;

        public MachineConfig[] MachineConfigs => machineConfigs;
        public float BoostDuration => boostDuration;
        public float BoostMultiplier => boostMultiplier;
        public bool IsBoostEnabled => isBoostEnabled;
        public float MaxOfflineProductionSeconds => maxOfflineProductionSeconds;
    }
}

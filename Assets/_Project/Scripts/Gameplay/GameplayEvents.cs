using System;
using UnityEngine;

namespace MiniFactory.Gameplay
{
    public static class GameplayEvents
    {
        public static event Action<string> MachineUnlocked;
        public static event Action<string, int> MachineUpgraded;

        public static void RaiseMachineUnlocked(string id) => MachineUnlocked?.Invoke(id);
        public static void RaiseMachineUpgraded(string id, int level) => MachineUpgraded?.Invoke(id, level);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticEvents()
        {
            MachineUnlocked = null;
            MachineUpgraded = null;
        }
    }
}

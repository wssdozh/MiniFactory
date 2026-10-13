using System;

namespace MiniFactory.Gameplay
{
    public static class GameplayEvents
    {
        public static event Action<string> MachineUnlocked;
        public static event Action<string, int> MachineUpgraded;

        public static void RaiseMachineUnlocked(string id) => MachineUnlocked?.Invoke(id);
        public static void RaiseMachineUpgraded(string id, int level) => MachineUpgraded?.Invoke(id, level);
    }
}

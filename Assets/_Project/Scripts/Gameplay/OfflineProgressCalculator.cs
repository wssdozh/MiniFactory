using System;

namespace MiniFactory.Gameplay
{
    public static class OfflineProgressCalculator
    {
        public static float CalculateOfflineIncome(
            float incomePerSec,
            float elapsedSeconds,
            float maxOfflineSeconds,
            BoostState boost)
        {
            float cappedElapsed = Math.Min(elapsedSeconds, maxOfflineSeconds);

            if (boost == null || boost.IsActive == false)
                return incomePerSec * cappedElapsed;

            float boostedDuration = Math.Min(boost.RemainingSeconds, cappedElapsed);
            float normalDuration = cappedElapsed - boostedDuration;

            return (incomePerSec * boost.Multiplier * boostedDuration) + (incomePerSec * normalDuration);
        }
    }
}

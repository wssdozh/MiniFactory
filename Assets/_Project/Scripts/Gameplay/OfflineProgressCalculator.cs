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
            
            if (boost == null || !boost.IsActive)
                return incomePerSec * cappedElapsed;

            // Boost was active at save time
            float boostedDuration = Math.Min(boost.RemainingSeconds, cappedElapsed);
            float normalDuration = cappedElapsed - boostedDuration;

            return (incomePerSec * boost.Multiplier * boostedDuration) + (incomePerSec * normalDuration);
        }
    }
}

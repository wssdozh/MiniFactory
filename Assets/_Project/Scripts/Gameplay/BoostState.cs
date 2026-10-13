namespace MiniFactory.Gameplay
{
    public class BoostState
    {
        public bool IsActive { get; private set; }
        public float Multiplier { get; private set; } = 1f;
        public float RemainingSeconds { get; private set; }

        public BoostState(SavedBoost saved = null)
        {
            if (saved != null && saved.active && saved.remainingSeconds > 0)
            {
                IsActive = true;
                Multiplier = saved.multiplier;
                RemainingSeconds = saved.remainingSeconds;
            }
        }

        public void Activate(float duration, float multiplier)
        {
            IsActive = true;
            Multiplier = multiplier;
            RemainingSeconds = duration;
        }

        public void Tick(float deltaTime)
        {
            if (!IsActive) return;

            RemainingSeconds -= deltaTime;
            if (RemainingSeconds <= 0)
            {
                IsActive = false;
                Multiplier = 1f;
                RemainingSeconds = 0;
            }
        }
    }
}

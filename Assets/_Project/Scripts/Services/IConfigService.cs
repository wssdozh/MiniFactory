namespace MiniFactory.Services
{
    public interface IConfigService
    {
        float BoostDuration { get; }
        float BoostMultiplier { get; }
        bool IsBoostEnabled { get; }
        float MaxOfflineProductionSeconds { get; }
    }
}

namespace SuperMetroid.Core.Progression;

/// <summary>Records the current area's boss, mini-boss and Torizo as defeated.</summary>
public interface IAreaBossFlags : IAreaBossFlagReader
{
    /// <summary>Records <paramref name="bosses"/> as defeated in the current area.</summary>
    void SetDefeated(DefeatedBosses bosses);
}

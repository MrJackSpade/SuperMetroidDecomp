namespace SuperMetroid.Core.Progression;

/// <summary>The current area's boss, mini-boss and Torizo defeat bits.</summary>
public interface IAreaBossFlags : IAreaBossFlagReader
{
    /// <summary>Sets <paramref name="bits"/> for the current area.</summary>
    void SetDefeated(BossBits bits);
}

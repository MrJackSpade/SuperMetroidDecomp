namespace SuperMetroid.Core.Game;

/// <summary>Reads the current area's boss, mini-boss and Torizo defeat bits (#633).</summary>
public interface IAreaBossFlagReader
{
    /// <summary>Whether any of <paramref name="bits"/> is set for the current area.</summary>
    bool IsDefeated(BossBits bits);
}

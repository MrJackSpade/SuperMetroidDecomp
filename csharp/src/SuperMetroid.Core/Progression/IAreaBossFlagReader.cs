namespace SuperMetroid.Core.Progression;

/// <summary>Reads whether the current area's boss, mini-boss and Torizo are defeated (#633).</summary>
public interface IAreaBossFlagReader
{
    /// <summary>Whether any of <paramref name="bosses"/> is defeated in the current area.</summary>
    bool IsDefeated(DefeatedBosses bosses);
}

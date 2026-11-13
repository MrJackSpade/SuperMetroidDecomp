namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// The Torizo whose belly a glow lights (#1700). The Torizo starts the glow as its fight starts and hands itself to it;
/// the glow watches that Torizo (<see cref="ColourEffectCondition.EndsWhenTorizoDies"/>) and ends on the first update it
/// reports its defeat.
/// </summary>
public interface ILitTorizo
{
    /// <summary>Whether the Torizo has no health left.</summary>
    bool IsDefeated { get; }
}

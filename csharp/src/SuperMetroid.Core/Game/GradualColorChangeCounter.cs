namespace SuperMetroid.Core.Game;

/// <summary>
/// WRAM <c>$7E:C400</c> <c>PaletteChangeNumerator</c>: the one step counter every bank-$82
/// gradual colour change advances (<c>$82:DA02</c> and its palette-subset variants). Door
/// fades, Kraid's room-background fades and Torizo's body fades all share it, so a fade
/// that completes resets the counter under any other fade running in the same frames.
/// </summary>
public sealed class GradualColorChangeCounter
{
    /// <summary>The current transition number.</summary>
    public ushort Numerator { get; internal set; }
}

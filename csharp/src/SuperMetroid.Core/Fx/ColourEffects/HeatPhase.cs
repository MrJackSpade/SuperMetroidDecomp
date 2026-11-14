namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// One of the sixteen phases of the heat pulse a heated Norfair room glows in (#1598): the room's heat glow publishes each
/// phase as it shows that phase's look, and Samus's heat shimmer follows the published phase, restarting her suit's pulse
/// at the same phase. Phases run from <see cref="First"/> up and round again.
/// </summary>
public readonly record struct HeatPhase
{
    /// <summary>The phases the heat pulse has.</summary>
    public const int Count = 16;

    /// <summary>The phase the pulse starts at, and the one the session holds before any room publishes a phase.</summary>
    public static HeatPhase First { get; } = new(0);

    /// <summary>Names phase <paramref name="index"/> of the pulse.</summary>
    /// <param name="index">The phase's place in the pulse, from zero.</param>
    /// <exception cref="ArgumentOutOfRangeException">The pulse has no such phase.</exception>
    public HeatPhase(int index) => Index = Ensure.BetweenInclusive(0, Count - 1, index);

    /// <summary>The phase's place in the pulse, from zero: the frame of her suit's pulse the shimmer restarts at.</summary>
    public int Index { get; }
}

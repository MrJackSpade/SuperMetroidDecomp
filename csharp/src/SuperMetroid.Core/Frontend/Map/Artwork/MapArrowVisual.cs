namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Provides one map arrow's screen anchor and cyclic cosmetic phase timing, as installed (#1596).</summary>
public sealed class MapArrowVisual
{
    /// <summary>The installed dwell of each phase, in ticks.</summary>
    internal readonly int[] durations;

    /// <summary>Keeps the installed anchor and the complete phase timing.</summary>
    /// <param name="position">The arrow's screen-pixel anchor.</param>
    /// <param name="durations">Each phase's dwell in ticks, in phase order; the cycle has at least one phase.</param>
    /// <exception cref="InvalidDataException">The cycle has no phase.</exception>
    internal MapArrowVisual(Position16<SpritePixels> position, int[] durations)
    {
        Position = position;
        this.durations = [.. Ensure.NotNull(durations)];
        if (this.durations.Length == 0)
            throw new InvalidDataException("A map arrow's animation cycles through at least one phase.");
    }

    /// <summary>Screen-pixel anchor used when composing this direction's sprite.</summary>
    public Position16<SpritePixels> Position { get; }

    /// <summary>Bounds the cyclic phase index supplied to <see cref="Duration"/>.</summary>
    public int PhaseCount => durations.Length;

    /// <summary>The installed dwell of one phase, in ticks.</summary>
    public int Duration(int phase) =>
        (uint)phase < (uint)durations.Length ? durations[phase] : throw new ArgumentOutOfRangeException(nameof(phase));
}

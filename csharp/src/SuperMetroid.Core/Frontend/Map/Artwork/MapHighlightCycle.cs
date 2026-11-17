using System.Collections.Immutable;

namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>
/// The map highlight cycle's timing (#1512): the steps it shows in turn and how long each holds. The colours of each step
/// are a shade the import pre-colours the map sprite sheet in (<see cref="MapSpriteShades.Highlight"/>).
/// </summary>
public sealed class MapHighlightCycle
{
    /// <summary>The hold of each step, in order, in map colour animation updates.</summary>
    private readonly ImmutableArray<int> durations;

    /// <summary>Keeps each step's hold.</summary>
    /// <param name="durations">
    /// The hold of each step in order, in updates; the cycle has at least one step and each holds at least one update. The
    /// cycle wraps at its own step count and each hold is a plain count of updates, so neither has a limit of its own (#1627).
    /// </param>
    /// <exception cref="InvalidDataException">The cycle has no step, or a hold below one update.</exception>
    public MapHighlightCycle(ImmutableArray<int> durations)
    {
        if (durations.IsDefault || durations.Length < 1 || durations.Any(hold => hold < 1))
            throw new InvalidDataException("The map highlight cycle holds at least one step, each held for one or more updates.");
        this.durations = durations;
    }

    /// <summary>The number of steps the cycle shows before it wraps.</summary>
    public int FrameCount => durations.Length;

    /// <summary>The cycle's steps in the order it shows them.</summary>
    public IEnumerable<MapHighlightStep> Steps => Enumerable.Range(0, durations.Length).Select(place => new MapHighlightStep(place));

    /// <summary>How many updates <paramref name="step"/> holds.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="step"/> is outside the cycle.</exception>
    public int Duration(MapHighlightStep step) => durations[Ensure.BetweenInclusive(0, durations.Length - 1, step.Place)];
}

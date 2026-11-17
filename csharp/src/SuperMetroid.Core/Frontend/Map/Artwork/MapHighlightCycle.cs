using System.Collections.Immutable;

namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>
/// The map highlight cycle's timing (#1512): the frames it shows in turn and how long each holds. The colours of each
/// frame are a shade the import pre-colours the map sprite sheet in (<see cref="MapSpriteShades.Highlight"/>).
/// </summary>
public sealed class MapHighlightCycle
{
    /// <summary>The hold of each frame, in order, in map colour animation updates.</summary>
    private readonly ImmutableArray<int> durations;

    /// <summary>Keeps each frame's hold.</summary>
    /// <param name="durations">
    /// The hold of each frame in order, in updates; the cycle has at least one frame and each holds at least one update.
    /// The cycle wraps at its own frame count and each hold is a plain count of updates, so neither has a limit of its own
    /// (#1627).
    /// </param>
    /// <exception cref="InvalidDataException">The cycle has no frame, or a hold below one update.</exception>
    public MapHighlightCycle(ImmutableArray<int> durations)
    {
        if (durations.IsDefault || durations.Length < 1 || durations.Any(hold => hold < 1))
            throw new InvalidDataException("The map highlight cycle holds at least one frame, each held for one or more updates.");
        this.durations = durations;
    }

    /// <summary>The number of frames the cycle shows before it wraps.</summary>
    public int FrameCount => durations.Length;

    /// <summary>The cycle's frames in the order it shows them (#2359).</summary>
    public IEnumerable<MapHighlightFrame> Frames => Enumerable.Range(0, durations.Length).Select(MapHighlightFrame.At);

    /// <summary>How many updates <paramref name="frame"/> holds.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> is outside the cycle.</exception>
    public int Duration(MapHighlightFrame frame) => durations[Ensure.BetweenInclusive(0, durations.Length - 1, frame.Frame)];
}

namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// SpaceTime's progression overwrite (#1318, #1330, #1474, #1475). An uncharged SpaceTime shot, on each update once its
/// animation shows a frame, turns story events, area bosses, item locations, doors, used stations and area maps on or
/// off and changes the loading state. What it changes depends only on the shot's place and where its animation stands:
/// a shot whose animation stands late enough reaches saved progress, and an earlier one, or one with no animation,
/// changes none. The import decodes the overwrite once for each animation position a SpaceTime shot can hold and each
/// shot place. Only the progress part is kept (owner decision on #1318).
/// </summary>
/// <remarks>
/// Consulted by the SpaceTime shot's frame behaviour during the projectile update, after its animation has installed a
/// frame. Evidence: the shot's frame behaviour starts inside a long store and falls back into the beam palette's copy
/// loop with the position its update loop left behind; the copy starts as many colours into the beam palette as the
/// shot's place, plus one, reads from the place its animation has reached, and is bounded by a signed test of that read
/// position, so a shot animating from an animation laid out late in its animation data copies on through the rest of
/// that data, past the palette and on through saved progress, before the read position wraps and the loop ends; before
/// an animation installs a frame the copy starts low and ends long before progress. The copy reaches progress in the order
/// saved progress is stored: story events, area bosses, item locations, doors, used stations, area maps and the loading
/// state, so an overwrite decides everything before the point where the copy stops and nothing after.
/// The SpaceTime check replays the
/// native Pit Room probe's two input phases, whose corrupted loading state it matches, and runs the original copy over
/// the cartridge for every position and shot place, requiring this service's overwrite to leave every flag of progress
/// as the copy does.
/// </remarks>
/// <typeparam name="TCursor">Where a shot stands in its animation.</typeparam>
/// <typeparam name="TOverwrite">What one copy does to progress, in the progress's own terms.</typeparam>
public sealed partial class SpaceTimeProgressionOverwrite<TCursor, TOverwrite>
    where TCursor : notnull
    where TOverwrite : class
{
    /// <summary>What the copy overwrites, by animation position, then by shot place; null where it stops before progress.</summary>
    private readonly IReadOnlyDictionary<TCursor, IReadOnlyDictionary<int, TOverwrite?>> _overwrites;

    /// <summary>Keeps the decoded overwrite of every animation position a SpaceTime shot can hold, by shot place.</summary>
    /// <param name="overwrites">What the copy overwrites, by animation position, then keyed by the shot place.</param>
    public SpaceTimeProgressionOverwrite(IReadOnlyDictionary<TCursor, IReadOnlyDictionary<int, TOverwrite?>> overwrites) =>
        _overwrites = Ensure.NotNull(overwrites);

    /// <summary>
    /// What a SpaceTime shot in shot place <paramref name="place"/> whose animation stands at <paramref name="cursor"/> overwrites;
    /// null when its copy stops before progress.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">No SpaceTime shot can stand at the position, or the place is not a shot place.</exception>
    public TOverwrite? For(TCursor cursor, int place)
    {
        if (!_overwrites.TryGetValue(cursor, out IReadOnlyDictionary<int, TOverwrite?>? byPlace))
            throw new ArgumentOutOfRangeException(nameof(cursor), cursor, "No SpaceTime shot animates from this position.");
        return byPlace.TryGetValue(place, out TOverwrite? overwrite)
            ? overwrite
            : throw new ArgumentOutOfRangeException(nameof(place), place, $"A SpaceTime shot occupies one of {byPlace.Count} places.");
    }
}

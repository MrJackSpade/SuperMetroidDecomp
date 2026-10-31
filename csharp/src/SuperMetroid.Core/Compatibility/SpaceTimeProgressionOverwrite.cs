namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// SpaceTime's progression overwrite (#1318, #1330, #1474). An uncharged SpaceTime shot's callback starts inside a long
/// store and falls back into the beam palette's copy loop with the position its update loop left behind: the copy starts
/// one colour past the shot's slot in the beam palette and reads from the place its animation has reached. Its bound is a
/// signed test of that read position, so a shot animating from a list high in its bank copies on through the rest of the
/// bank, past the palette and on through saved progression, before the read position wraps and the loop ends. Which
/// bytes land in progression therefore depends only on the shot's slot and where its animation stands; the import
/// pipeline decodes them once into a <see cref="ProgressionOverwrite"/> for each animation position a SpaceTime shot can
/// hold and each slot. Only the progression part is kept (owner decision on #1318).
/// </summary>
/// <remarks>
/// Consulted by the SpaceTime shot's pre-instruction during the projectile update, after its animation has installed a
/// frame. Before that, or with no list, the copy starts low and ends long before progression. Evidence: the SpaceTime
/// check replays the native Pit Room probe's two alpha passes, whose progression hash and corrupted loading state it
/// matches, and its codec decodes the original copy for every position and slot and compares it with this service.
/// </remarks>
/// <typeparam name="TCursor">Where a shot's animation stands in its program.</typeparam>
[CompatibilityService("SpaceTime's palette copy runs past the palette and overwrites saved progression")]
public sealed class SpaceTimeProgressionOverwrite<TCursor>
    where TCursor : notnull
{
    /// <summary>What the copy overwrites, by animation position, then by slot; null where it stops before progression.</summary>
    private readonly IReadOnlyDictionary<TCursor, IReadOnlyList<ProgressionOverwrite?>> _overwrites;

    /// <summary>Keeps the decoded overwrite of every animation position a SpaceTime shot can hold, by slot.</summary>
    /// <param name="overwrites">What the copy overwrites, by animation position, then by slot ordinal.</param>
    public SpaceTimeProgressionOverwrite(IReadOnlyDictionary<TCursor, IReadOnlyList<ProgressionOverwrite?>> overwrites) =>
        _overwrites = Ensure.NotNull(overwrites);

    /// <summary>
    /// What a SpaceTime shot in slot <paramref name="slot"/> whose animation stands at <paramref name="cursor"/> overwrites;
    /// null when its copy stops before progression.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">No SpaceTime shot can stand at the position, or the slot is not a shot slot.</exception>
    public ProgressionOverwrite? For(TCursor cursor, int slot)
    {
        if (!_overwrites.TryGetValue(cursor, out IReadOnlyList<ProgressionOverwrite?>? bySlot))
            throw new ArgumentOutOfRangeException(nameof(cursor), cursor, "No SpaceTime shot animates from this position.");
        return (uint)slot < (uint)bySlot.Count
            ? bySlot[slot]
            : throw new ArgumentOutOfRangeException(nameof(slot), slot, $"A SpaceTime shot occupies one of {bySlot.Count} slots.");
    }
}

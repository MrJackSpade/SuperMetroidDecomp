using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The rot step each region a rotting corpse shows its rows over shows (#1392, #1406, #1463). A corpse rots row by row; its
/// sheet holds the picture of each step, so the room keeps only the step each region showed last and when, for its frames
/// to name the picture they draw.
/// </summary>
[RoomLifetime]
internal sealed class CorpseRotSteps
{
    /// <summary>Each region's last shown step, and the place of the latest look shown when it was shown.</summary>
    private readonly Dictionary<LookRegion, ShownRot> shown = [];

    /// <summary>Records that <paramref name="regions"/> show rot step <paramref name="step"/> from look place <paramref name="stamp"/> on.</summary>
    internal void Show(IEnumerable<LookRegion> regions, int step, long stamp)
    {
        Ensure.AtLeastZero(step);
        foreach (LookRegion region in Ensure.NotNull(regions))
            shown[region] = new ShownRot(step, stamp);
    }

    /// <summary>The rot step <paramref name="region"/> shows, while a corpse's rows show over it, and the look place it was shown at.</summary>
    internal bool TryShowing(LookRegion region, out int step, out long stamp)
    {
        if (shown.TryGetValue(Ensure.NotNull(region), out ShownRot row))
        {
            step = row.Step;
            stamp = row.Stamp;
            return true;
        }
        step = 0;
        stamp = 0;
        return false;
    }

    /// <summary>A region's last shown rot step and the place of the latest look shown when it was shown.</summary>
    private readonly record struct ShownRot(int Step, long Stamp);
}

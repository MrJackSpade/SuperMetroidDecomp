using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The rot step each region a rotting corpse shows its rows over shows (#1392, #1406). A corpse rots by moving the pixel rows
/// of its characters down and showing them after each step; its sheet holds the picture of each step, so the room keeps
/// only the step each region showed last, for its frames to name the picture they draw.
/// </summary>
[RoomLifetime]
internal sealed class CorpseRotUploads
{
    /// <summary>Each region's last shown step, and the place of the latest character upload when it was shown.</summary>
    private readonly Dictionary<CharacterRegion, (int Step, long Stamp)> shown = [];

    /// <summary>Records that <paramref name="regions"/> show rot step <paramref name="step"/> from upload place <paramref name="stamp"/> on.</summary>
    internal void Show(IEnumerable<CharacterRegion> regions, int step, long stamp)
    {
        Ensure.AtLeastZero(step);
        foreach (CharacterRegion region in Ensure.NotNull(regions))
            shown[region] = (step, stamp);
    }

    /// <summary>The rot step <paramref name="region"/> shows, while a corpse's rows show over it, and the upload place it was shown at.</summary>
    internal bool TryShowing(CharacterRegion region, out int step, out long stamp)
    {
        if (shown.TryGetValue(Ensure.NotNull(region), out (int Step, long Stamp) row))
        {
            (step, stamp) = row;
            return true;
        }
        step = 0;
        stamp = 0;
        return false;
    }
}

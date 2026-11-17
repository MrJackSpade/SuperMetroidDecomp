using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Bosses.Ridley;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// Ridley's fireball and afterburn segments' native beat and control addresses (#1287, #2331): each segment's own list,
/// and the shared puff as each afterburn list's own tail, where the cartridge repeats it.
/// </summary>
/// <param name="Lists">Each segment's list; the puff's is the first afterburn list's tail.</param>
/// <param name="PuffAfter">Each afterburn run's puff, by the segment the run's animation starts at, at that list's own addresses.</param>
internal sealed record RidleyFireballNativeLists(
    IReadOnlyDictionary<RidleyFireballSegment, NativeEnemyList<RidleyFireballSegment>> Lists,
    IReadOnlyDictionary<RidleyFireballSegment, NativeEnemyList<RidleyFireballSegment>> PuffAfter)
{
    /// <summary>The number of segments with a list of their own or a tail of another's.</summary>
    internal int Count => Lists.Count;

    /// <summary>
    /// The lists <paramref name="animation"/> plays at their native addresses: the afterburn run it starts at, its puff at
    /// that run's own list's tail.
    /// </summary>
    internal IReadOnlyDictionary<RidleyFireballSegment, NativeEnemyList<RidleyFireballSegment>> For(Animation<RidleyFireballSegment> animation)
    {
        if (animation.Phases.Count == 0 || !PuffAfter.TryGetValue(animation.Phases[0].Segment, out NativeEnemyList<RidleyFireballSegment>? puff))
            return Lists;
        Dictionary<RidleyFireballSegment, NativeEnemyList<RidleyFireballSegment>> played = Lists.ToDictionary(pair => pair.Key, pair => pair.Value);
        played[RidleyFireballSegment.AfterburnPuff] = puff;
        return played;
    }
}

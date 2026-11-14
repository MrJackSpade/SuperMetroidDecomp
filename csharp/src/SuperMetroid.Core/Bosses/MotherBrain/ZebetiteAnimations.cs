using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The Zebetites' typed health-tier animations (#641).</summary>
internal static class ZebetiteAnimations
{
    /// <summary>The pose each health tier shows.</summary>
    private static readonly Dictionary<ZebetiteSegment, ZebetiteFrame> PoseOf = new()
    {
        [ZebetiteSegment.LargeAtLeast800] = ZebetiteFrame.LargeAtLeast800,
        [ZebetiteSegment.LargeAtLeast600] = ZebetiteFrame.LargeAtLeast600,
        [ZebetiteSegment.LargeAtLeast400] = ZebetiteFrame.LargeAtLeast400,
        [ZebetiteSegment.LargeAtLeast200] = ZebetiteFrame.LargeAtLeast200,
        [ZebetiteSegment.LargeBelow200] = ZebetiteFrame.LargeBelow200,
        [ZebetiteSegment.SplitAtLeast800] = ZebetiteFrame.SplitAtLeast800,
        [ZebetiteSegment.SplitAtLeast600] = ZebetiteFrame.SplitAtLeast600,
        [ZebetiteSegment.SplitAtLeast400] = ZebetiteFrame.SplitAtLeast400,
        [ZebetiteSegment.SplitAtLeast200] = ZebetiteFrame.SplitAtLeast200,
        [ZebetiteSegment.SplitBelow200] = ZebetiteFrame.SplitBelow200,
    };

    /// <summary>The large barrier's tiers, from the most health to the least.</summary>
    private static readonly ZebetiteSegment[] LargeTiers =
        [ZebetiteSegment.LargeAtLeast800, ZebetiteSegment.LargeAtLeast600, ZebetiteSegment.LargeAtLeast400, ZebetiteSegment.LargeAtLeast200, ZebetiteSegment.LargeBelow200];

    /// <summary>A split pair's tiers, from the most health to the least.</summary>
    private static readonly ZebetiteSegment[] SplitTiers =
        [ZebetiteSegment.SplitAtLeast800, ZebetiteSegment.SplitAtLeast600, ZebetiteSegment.SplitAtLeast400, ZebetiteSegment.SplitAtLeast200, ZebetiteSegment.SplitBelow200];

    /// <summary>Every health-tier segment: its pose for one update.</summary>
    internal static AnimationSegments<ZebetiteSegment, ZebetiteFrame, NoAnimationCue> Segments { get; } = new(
        Enum.GetValues<ZebetiteSegment>().ToDictionary(segment => segment, segment =>
            new AnimationBeat<ZebetiteFrame, NoAnimationCue>[]
            {
                new AnimationBeat<ZebetiteFrame, NoAnimationCue>.Frame(PoseOf[segment], 1),
            }));

    /// <summary>Each health tier's animation: its pose, held as the script's sleep holds it.</summary>
    internal static IReadOnlyDictionary<ZebetiteSegment, Animation<ZebetiteSegment>> StartingAt { get; } =
        Enum.GetValues<ZebetiteSegment>().ToDictionary(segment => segment,
            segment => new Animation<ZebetiteSegment>([new(segment, 0, 1, 1)], AnimationEnd.Holds));

    /// <summary>The animation that starts at <paramref name="segment"/>, before its first play.</summary>
    internal static AnimationPlayback<ZebetiteSegment> Starting(ZebetiteSegment segment) =>
        new(StartingAt[segment], AnimationPlace.StartOfAnimation);

    /// <summary>
    /// The animation for a barrier's form and health: large or split, then one tier for each 200 health below 1000,
    /// the last covering everything under 200.
    /// </summary>
    internal static ZebetiteSegment HealthAnimation(bool linkedPair, int health)
    {
        int tier = health < 200 ? 4 :
            health < 400 ? 3 :
            health < 600 ? 2 :
            health < 800 ? 1 : 0;
        return (linkedPair ? SplitTiers : LargeTiers)[tier];
    }
}

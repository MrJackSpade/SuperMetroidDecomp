using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The Zebetites' typed health-tier animations (#641).</summary>
internal static class ZebetiteAnimations
{
    /// <summary>The health-tier animations in the order their frames are numbered.</summary>
    private static readonly IdentitySequence<ZebetiteSegment> FrameOrder = new(
        ZebetiteSegment.LargeAtLeast800, ZebetiteSegment.LargeAtLeast600, ZebetiteSegment.LargeAtLeast400,
        ZebetiteSegment.LargeAtLeast200, ZebetiteSegment.LargeBelow200, ZebetiteSegment.SplitAtLeast800,
        ZebetiteSegment.SplitAtLeast600, ZebetiteSegment.SplitAtLeast400, ZebetiteSegment.SplitAtLeast200,
        ZebetiteSegment.SplitBelow200);

    /// <summary>Every health-tier segment: its pose for one update.</summary>
    internal static AnimationSegments<ZebetiteSegment, ZebetiteFrame, NoAnimationCue> Segments { get; } = new(
        Enum.GetValues<ZebetiteSegment>().ToDictionary(segment => segment, segment =>
            new AnimationBeat<ZebetiteFrame, NoAnimationCue>[]
            {
                new AnimationBeat<ZebetiteFrame, NoAnimationCue>.Frame(new ZebetiteFrame(FrameOrder.PositionOf(segment)), 1),
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
        return (linkedPair ? ZebetiteSegment.SplitAtLeast800 : ZebetiteSegment.LargeAtLeast800) + tier;
    }
}

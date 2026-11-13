using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ridley's breakup fragment animations (#641, #1600), one per <see cref="RidleyBreakupFrame"/>: each shows its pose for
/// one update and then holds.
/// </summary>
internal static class RidleyBreakupAnimations
{
    /// <summary>Every fragment segment's one beat: its pose for one update.</summary>
    internal static AnimationSegments<RidleyBreakupFrame, RidleyBreakupFrame, NoAnimationCue> Segments { get; } = new(
        Enum.GetValues<RidleyBreakupFrame>().ToDictionary(
            frame => frame,
            frame => new AnimationBeat<RidleyBreakupFrame, NoAnimationCue>[] { new AnimationBeat<RidleyBreakupFrame, NoAnimationCue>.Frame(frame, 1) }));

    /// <summary>Every fragment animation: its pose once, then a hold.</summary>
    internal static IReadOnlyDictionary<RidleyBreakupFrame, Animation<RidleyBreakupFrame>> StartingAt { get; } =
        Enum.GetValues<RidleyBreakupFrame>().ToDictionary(
            frame => frame,
            frame => new Animation<RidleyBreakupFrame>([new(frame, 0, 1, 1)], AnimationEnd.Holds));

    /// <summary>The animation that shows <paramref name="frame"/>.</summary>
    internal static Animation<RidleyBreakupFrame> StartingWith(RidleyBreakupFrame frame) => StartingAt[frame];
}

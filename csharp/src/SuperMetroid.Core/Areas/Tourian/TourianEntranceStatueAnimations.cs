using P = SuperMetroid.Core.Areas.Tourian.TourianEntranceStatueSegment;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The three Tourian entrance-statue records' animations, each of which deletes its record at once, (#641). Each
/// frame shows a <see cref="TourianEntranceStatueFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class TourianEntranceStatueAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<TourianEntranceStatueSegment, TourianEntranceStatueFrame, NoAnimationCue> Segments { get; } = new(
        new Dictionary<TourianEntranceStatueSegment, AnimationBeat<TourianEntranceStatueFrame, NoAnimationCue>[]>
    {
        [P.Ridley] =
        [
        ],
        [P.Phantoon] =
        [
        ],
        [P.BaseDecoration] =
        [
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<TourianEntranceStatueSegment, Animation<TourianEntranceStatueSegment>> StartingAt { get; } = new Dictionary<TourianEntranceStatueSegment, Animation<TourianEntranceStatueSegment>>
    {
        [P.Ridley] = new([], AnimationEnd.Stops),
        [P.Phantoon] = new([], AnimationEnd.Stops),
        [P.BaseDecoration] = new([], AnimationEnd.Stops),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<TourianEntranceStatueSegment> StartingWith(TourianEntranceStatueSegment segment) => StartingAt[segment];
}

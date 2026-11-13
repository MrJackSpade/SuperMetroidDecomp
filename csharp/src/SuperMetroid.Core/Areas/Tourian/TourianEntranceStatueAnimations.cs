using P = SuperMetroid.Core.Areas.Tourian.TourianEntranceStatueProgram;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The three Tourian entrance-statue records' animations, each of which deletes its record at once, (#641). Each
/// frame shows a <see cref="TourianEntranceStatueFrame"/> pose and no beat raises a cue for its owner.
/// </summary>
internal static class TourianEntranceStatueAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<TourianEntranceStatueProgram, TourianEntranceStatueFrame, NoAnimationCue> Segments { get; } = new(
        new Dictionary<TourianEntranceStatueProgram, AnimationBeat<TourianEntranceStatueFrame, NoAnimationCue>[]>
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
    internal static IReadOnlyDictionary<TourianEntranceStatueProgram, Animation<TourianEntranceStatueProgram>> StartingAt { get; } = new Dictionary<TourianEntranceStatueProgram, Animation<TourianEntranceStatueProgram>>
    {
        [P.Ridley] = new([], AnimationEnd.Stops),
        [P.Phantoon] = new([], AnimationEnd.Stops),
        [P.BaseDecoration] = new([], AnimationEnd.Stops),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<TourianEntranceStatueProgram> StartingWith(TourianEntranceStatueProgram segment) => StartingAt[segment];
}

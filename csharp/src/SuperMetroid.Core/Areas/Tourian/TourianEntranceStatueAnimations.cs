using P = SuperMetroid.Core.Areas.Tourian.TourianEntranceStatueProgram;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The three Tourian entrance-statue records' animations, each of which deletes its record at once, (#641). Each
/// frame shows a <see cref="TourianEntranceStatueFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class TourianEntranceStatueAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<TourianEntranceStatueProgram, TourianEntranceStatueFrame, NoAnimationCue> Programs { get; } = new(
        new Dictionary<TourianEntranceStatueProgram, AnimationProgramStep<TourianEntranceStatueProgram, TourianEntranceStatueFrame, NoAnimationCue>[]>
    {
        [P.Ridley] =
        [
            Stop,
        ],
        [P.Phantoon] =
        [
            Stop,
        ],
        [P.BaseDecoration] =
        [
            Stop,
        ],
    });

    /// <summary>Ends the actor's program, as the stop does.</summary>
    private static AnimationProgramStep<TourianEntranceStatueProgram, TourianEntranceStatueFrame, NoAnimationCue> Stop => AnimationProgramStep<TourianEntranceStatueProgram, TourianEntranceStatueFrame, NoAnimationCue>.Stop;
}

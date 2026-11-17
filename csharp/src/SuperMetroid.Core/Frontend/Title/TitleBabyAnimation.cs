using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Frontend.Title;

/// <summary>
/// The Baby Metroid lying on the title's turning floor (#2359), played by the shared animation player: it pulses through
/// its poses (<see cref="TitleSequenceDefinitions.Graphics.BabyAnimation"/>), each for
/// <see cref="TitleSequenceDefinitions.Timing.BabyFrameDuration"/> updates, over and over. The pulse starts on its first
/// pose, one update from its next.
/// </summary>
internal static class TitleBabyAnimation
{
    /// <summary>The pulse's one segment.</summary>
    internal static AnimationSegments<TitleBabySegment, TitleBabyPose, NoAnimationCue> Segments { get; } = new(
        new Dictionary<TitleBabySegment, AnimationBeat<TitleBabyPose, NoAnimationCue>[]>
        {
            [TitleBabySegment.Pulse] =
            [
                .. TitleSequenceDefinitions.Graphics.BabyAnimation.Select(pose =>
                    new AnimationBeat<TitleBabyPose, NoAnimationCue>.Frame(pose, TitleSequenceDefinitions.Timing.BabyFrameDuration)),
            ],
        });

    /// <summary>The pulse played over and over.</summary>
    internal static Animation<TitleBabySegment> Pulsing { get; } = new([Segments.Whole(TitleBabySegment.Pulse)], AnimationEnd.LoopsFrom(0));

    /// <summary>Starts the pulse on its first pose, one update from its next.</summary>
    internal static AnimationPlayer<TitleBabySegment, TitleBabyPose, NoAnimationCue> Start() =>
        AnimationPlayer<TitleBabySegment, TitleBabyPose, NoAnimationCue>.StartShowing(Segments, Pulsing, 1);
}

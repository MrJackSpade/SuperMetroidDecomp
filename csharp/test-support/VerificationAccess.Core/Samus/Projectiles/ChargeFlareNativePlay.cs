using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>A component's animation read as the original numbers its pictures: each picture's updates, and the run its loop repeats.</summary>
/// <typeparam name="TSegment">The component's runs of pictures.</typeparam>
/// <typeparam name="TPicture">The component's pictures.</typeparam>
internal static class ChargeFlareNativePlay<TSegment, TPicture>
    where TSegment : struct
    where TPicture : struct
{
    /// <summary>The updates each picture of <paramref name="animation"/> shows for, its phases in order.</summary>
    internal static IReadOnlyList<int> Durations(AnimationSegments<TSegment, TPicture, NoAnimationCue> segments, LoopingAnimation<TSegment> animation) =>
        [.. segments.PlayedBeats(animation.Animation).Select(beat => beat is AnimationBeat<TPicture, NoAnimationCue>.Frame frame
            ? frame.Duration
            : throw new InvalidOperationException($"Charge-flare beat {beat} shows no picture."))];

    /// <summary>The pictures <paramref name="animation"/>'s loop repeats, by number: from the first it shows to one past the last.</summary>
    internal static (int From, int End) LoopedPictures(Animation<TSegment> animation)
    {
        var loop = (AnimationEnd.Loop)animation.End;
        AnimationPhase<TSegment> repeated = animation.Phases[loop.Phase];
        int from = animation.Phases.Take(loop.Phase).Sum(phase => phase.BeatCount * phase.Plays) + repeated.FirstBeat;
        return (from, from + repeated.BeatCount * repeated.Plays);
    }
}

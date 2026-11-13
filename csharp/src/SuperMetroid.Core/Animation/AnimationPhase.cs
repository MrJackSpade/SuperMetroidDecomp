namespace SuperMetroid.Core.Animation;

/// <summary>
/// One phase of an animation (#1600): a run of a segment's beats, played <paramref name="Plays"/> times before the animation
/// moves on. A counted phase (one played more than once, or counting <see cref="AnimationPhaseCounting.EveryPlay"/>) counts its plays down the owner's
/// repeat counter, which it loads as it starts: a counted phase played once still loads one and counts it to zero.
/// </summary>
/// <typeparam name="TSegment">The segment identity.</typeparam>
/// <param name="Segment">The segment whose beats the phase plays.</param>
/// <param name="FirstBeat">The segment's first beat the phase plays.</param>
/// <param name="BeatCount">How many of the segment's beats it plays, from <paramref name="FirstBeat"/>.</param>
/// <param name="Plays">How many times the run plays, at least once.</param>
/// <param name="Counting">Whether a phase played once still counts its play down the owner's repeat counter.</param>
internal readonly record struct AnimationPhase<TSegment>(TSegment Segment, int FirstBeat, int BeatCount, int Plays,
    AnimationPhaseCounting Counting = AnimationPhaseCounting.RepeatsOnly)
    where TSegment : struct
{
    /// <summary>Whether the phase loads and counts down the owner's repeat counter.</summary>
    internal bool CountsRepeats => Counting == AnimationPhaseCounting.EveryPlay || Plays > 1;
}

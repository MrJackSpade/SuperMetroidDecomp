namespace SuperMetroid.Core.Animation;

/// <summary>
/// One phase of an animation (#1600): a run of a segment's beats, played <paramref name="Plays"/> times before the animation
/// moves on. A repeated phase, one played more than once, counts its plays in the playback's place (#2006); a phase played
/// once counts nothing.
/// </summary>
/// <typeparam name="TSegment">The segment identity.</typeparam>
/// <param name="Segment">The segment whose beats the phase plays.</param>
/// <param name="FirstBeat">The segment's first beat the phase plays.</param>
/// <param name="BeatCount">How many of the segment's beats it plays, from <paramref name="FirstBeat"/>.</param>
/// <param name="Plays">How many times the run plays, at least once.</param>
internal readonly partial record struct AnimationPhase<TSegment>(TSegment Segment, int FirstBeat, int BeatCount, int Plays)
    where TSegment : struct
{
    /// <summary>Whether the phase plays its run more than once, counting its plays as it goes.</summary>
    internal bool IsRepeated => Plays > 1;
}

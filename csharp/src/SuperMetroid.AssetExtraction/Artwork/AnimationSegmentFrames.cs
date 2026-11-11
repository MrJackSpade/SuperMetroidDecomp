using SuperMetroid.Core.Programs;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// The frames a typed animation can show (#1600): the import pipeline composes a sheet of every frame a spawned object's
/// animations reach, phase by phase in the order they play.
/// </summary>
internal static class AnimationSegmentFrames
{
    /// <summary>The frames of one segment family.</summary>
    extension<TSegment, TFrame, TEvent>(AnimationSegments<TSegment, TFrame, TEvent> segments)
        where TSegment : struct, Enum
        where TFrame : struct
        where TEvent : notnull
    {
        /// <summary>Every frame <paramref name="animation"/> can show, each once, in the order its phases first show them.</summary>
        internal IEnumerable<TFrame> FramesFrom(Animation<TSegment> animation) =>
            Ensure.NotNull(animation).Phases
                .SelectMany(phase => segments.Beats(phase.Segment).Skip(phase.FirstBeat).Take(phase.BeatCount))
                .OfType<AnimationBeat<TFrame, TEvent>.Frame>()
                .Select(frame => frame.Shown)
                .Distinct();
    }
}

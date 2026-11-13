using SuperMetroid.Core;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// The frames a typed animation can show (#1600): the import pipeline composes a sheet of every frame a spawned object's
/// animation reaches, which are the frames of every phase it plays.
/// </summary>
internal static class AnimationSegmentFrames
{
    /// <summary>The frames of one family's animations.</summary>
    extension<TSegment, TFrame, TEvent>(AnimationSegments<TSegment, TFrame, TEvent> segments)
        where TSegment : struct, Enum
        where TFrame : struct
        where TEvent : notnull
    {
        /// <summary>
        /// Every frame <paramref name="animation"/> can show, each once: the frames, held ones included, of each phase's run
        /// of beats.
        /// </summary>
        internal IEnumerable<TFrame> FramesOf(Animation<TSegment> animation) =>
            Ensure.NotNull(animation).Phases
                .SelectMany(phase => segments.Beats(phase.Segment).Skip(phase.FirstBeat).Take(phase.BeatCount))
                .Select(beat => beat.ShownFrame)
                .OfType<TFrame>()
                .Distinct();
    }
}

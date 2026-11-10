namespace SuperMetroid.Core.Programs;

/// <summary>The frame an animation shows next (#1600), or that it has ended.</summary>
/// <typeparam name="TSegment">The segment identity.</typeparam>
/// <typeparam name="TFrame">The frame identity.</typeparam>
internal abstract record AnimationShown<TSegment, TFrame>
    where TSegment : struct
    where TFrame : struct
{
    /// <summary>Allows only the outcomes below.</summary>
    private AnimationShown()
    {
    }

    /// <summary>
    /// The animation shows <paramref name="Shown"/> for <paramref name="Duration"/> updates: frame
    /// <paramref name="FrameInSegment"/> of <paramref name="Segment"/>, counting its frames only.
    /// </summary>
    /// <param name="Segment">The segment the frame belongs to.</param>
    /// <param name="FrameInSegment">The frame's place among the segment's frames.</param>
    /// <param name="Shown">The frame.</param>
    /// <param name="Duration">The updates it shows for.</param>
    internal sealed record Frame(TSegment Segment, int FrameInSegment, TFrame Shown, int Duration) : AnimationShown<TSegment, TFrame>;

    /// <summary>The animation pauses for <paramref name="Duration"/> updates, showing no new frame.</summary>
    /// <param name="Duration">The updates the pause lasts.</param>
    internal sealed record Paused(int Duration) : AnimationShown<TSegment, TFrame>;

    /// <summary>The animation rests on its last frame until its owner plays another.</summary>
    internal sealed record Held : AnimationShown<TSegment, TFrame>
    {
        /// <summary>The one held outcome.</summary>
        internal static Held Instance { get; } = new();
    }

    /// <summary>The animation has ended.</summary>
    internal sealed record Ended : AnimationShown<TSegment, TFrame>
    {
        /// <summary>The one ended outcome.</summary>
        internal static Ended Instance { get; } = new();
    }
}

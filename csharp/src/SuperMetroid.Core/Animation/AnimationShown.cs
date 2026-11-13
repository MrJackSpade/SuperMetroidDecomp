namespace SuperMetroid.Core.Animation;

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

    /// <summary>The animation shows <paramref name="Shown"/>, a frame of <paramref name="Segment"/>, for <paramref name="Duration"/> updates.</summary>
    /// <param name="Segment">The segment the frame belongs to.</param>
    /// <param name="Shown">The frame.</param>
    /// <param name="Duration">The updates it shows for.</param>
    internal sealed record Frame(TSegment Segment, TFrame Shown, int Duration) : AnimationShown<TSegment, TFrame>;

    /// <summary>
    /// The animation shows <paramref name="Shown"/> and rests on it for good (#1607), until its owner plays another.
    /// </summary>
    /// <param name="Shown">The frame.</param>
    internal sealed record HeldFrame(TFrame Shown) : AnimationShown<TSegment, TFrame>;

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

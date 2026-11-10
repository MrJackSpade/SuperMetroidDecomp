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

    /// <summary>The animation shows <paramref name="Shown"/>, a frame of <paramref name="Segment"/>, for <paramref name="Duration"/> updates.</summary>
    /// <param name="Segment">The segment the frame belongs to.</param>
    /// <param name="Shown">The frame.</param>
    /// <param name="Duration">The updates it shows for.</param>
    internal sealed record Frame(TSegment Segment, TFrame Shown, int Duration) : AnimationShown<TSegment, TFrame>;

    /// <summary>The animation has ended.</summary>
    internal sealed record Ended : AnimationShown<TSegment, TFrame>
    {
        /// <summary>The one ended outcome.</summary>
        internal static Ended Instance { get; } = new();
    }
}

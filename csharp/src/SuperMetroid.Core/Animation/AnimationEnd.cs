namespace SuperMetroid.Core.Animation;

/// <summary>How an animation ends once its last phase has played (#1600).</summary>
internal abstract partial record AnimationEnd
{
    /// <summary>Allows only the endings below.</summary>
    private AnimationEnd()
    {
    }

    /// <summary>The animation is over.</summary>
    internal static AnimationEnd Stops { get; } = new Stop();

    /// <summary>The animation rests on the frame it showed last until its owner plays another.</summary>
    internal static AnimationEnd Holds { get; } = new Hold();

    /// <summary>The animation is over.</summary>
    internal sealed partial record Stop : AnimationEnd;

    /// <summary>
    /// The animation plays on from <see cref="Phase"/>: the first phase of its cycle, which
    /// <see cref="Animation{TSegment}.LeadingInto"/> places after the lead, so no definition names it by its place (#2414).
    /// </summary>
    internal sealed partial record Loop : AnimationEnd
    {
        /// <summary>Plays on from <paramref name="phase"/>.</summary>
        /// <param name="phase">The phase it plays on from.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="phase"/> is negative.</exception>
        internal Loop(int phase) => Phase = Ensure.AtLeastZero(phase);

        /// <summary>The phase it plays on from.</summary>
        internal int Phase { get; }
    }

    /// <summary>The animation rests on its last frame until its owner plays another.</summary>
    internal sealed partial record Hold : AnimationEnd;
}

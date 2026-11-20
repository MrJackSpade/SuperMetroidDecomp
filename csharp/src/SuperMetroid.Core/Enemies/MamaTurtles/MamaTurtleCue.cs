namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>One cue of the Mama Turtle's animations: what it does, with the values it acts on.</summary>
internal abstract record MamaTurtleCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private MamaTurtleCue()
    {
    }

    /// <summary>What the function for this case makes of the cue, whichever case it is.</summary>
    /// <typeparam name="TResult">What the cue is made into.</typeparam>
    /// <param name="riseToHover">Makes a <see cref="RiseToHover"/> into the result.</param>
    /// <param name="signal">Makes a <see cref="Signal"/> into the result.</param>
    internal abstract TResult Match<TResult>(Func<RiseToHover, TResult> riseToHover, Func<Signal, TResult> signal);

    /// <summary>Starts the Mama Turtle's rise the way she leaves her shell facing, and continues at its spin (#2151).</summary>
    /// <param name="Direction">The way she faces as she rises.</param>
    internal sealed record RiseToHover(HorizontalDirection Direction) : MamaTurtleCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<RiseToHover, TResult> riseToHover, Func<Signal, TResult> signal) => Ensure.NotNull(riseToHover)(this);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(MamaTurtleCueKind Kind) : MamaTurtleCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<RiseToHover, TResult> riseToHover, Func<Signal, TResult> signal) => Ensure.NotNull(signal)(this);
    }
}

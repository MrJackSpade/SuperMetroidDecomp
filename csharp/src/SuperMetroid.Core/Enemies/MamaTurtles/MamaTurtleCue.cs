namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>One cue of the Mama Turtle's animations: what it does, with the values it acts on.</summary>
internal abstract record MamaTurtleCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private MamaTurtleCue()
    {
    }

    /// <summary>What <paramref name="cases"/> makes of the cue, through the case it is.</summary>
    /// <typeparam name="TResult">What the cue is made into.</typeparam>
    /// <param name="cases">What each case of the cue is made into.</param>
    internal abstract TResult Match<TResult>(MamaTurtleCueCases<TResult> cases);

    /// <summary>Starts the Mama Turtle's rise the way she leaves her shell facing, and continues at its spin (#2151).</summary>
    /// <param name="Direction">The way she faces as she rises.</param>
    internal sealed record RiseToHover(HorizontalDirection Direction) : MamaTurtleCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(MamaTurtleCueCases<TResult> cases) => Ensure.NotNull(cases).RiseToHover(this);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(MamaTurtleCueKind Kind) : MamaTurtleCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(MamaTurtleCueCases<TResult> cases) => Ensure.NotNull(cases).Signal(this);
    }
}

/// <summary>What each case of a <see cref="MamaTurtleCue"/> is made into (#2553).</summary>
/// <typeparam name="TResult">What the cue is made into.</typeparam>
/// <param name="RiseToHover">Makes a <see cref="MamaTurtleCue.RiseToHover"/> into the result.</param>
/// <param name="Signal">Makes a <see cref="MamaTurtleCue.Signal"/> into the result.</param>
internal sealed record MamaTurtleCueCases<TResult>(Func<MamaTurtleCue.RiseToHover, TResult> RiseToHover, Func<MamaTurtleCue.Signal, TResult> Signal);

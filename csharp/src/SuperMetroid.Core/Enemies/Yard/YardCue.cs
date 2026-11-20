namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>One cue of the Yard's animations: what it does, with the values it acts on.</summary>
internal abstract record YardCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private YardCue()
    {
    }

    /// <summary>What <paramref name="cases"/> makes of the cue, through the case it is.</summary>
    /// <typeparam name="TResult">What the cue is made into.</typeparam>
    /// <param name="cases">What each case of the cue is made into.</param>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract TResult Match<TContext, TResult>(YardCueCases<TContext, TResult> cases, TContext context);

    /// <summary>Moves the Yard by a signed pixel offset in X and Y.</summary>
    /// <param name="Offset">The cue's signed pixel offset, horizontal and vertical.</param>
    internal sealed record MoveBy(Offset16<RoomPixels> Offset) : YardCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(YardCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).MoveBy(this, context);
    }

    /// <summary>Sets the attachment direction and the airborne facing it implies.</summary>
    /// <param name="Direction">The surface direction a <see cref="SetDirection"/> cue attaches the Yard in.</param>
    internal sealed record SetDirection(YardDirection Direction) : YardCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(YardCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).SetDirection(this, context);
    }

    /// <summary>Names the animation the Yard hides with, or none.</summary>
    /// <param name="OptionalTarget">The animation the Yard hides with, or none where it does not hide.</param>
    internal sealed record SetHidingAnimation(YardSegment? OptionalTarget) : YardCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(YardCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).SetHidingAnimation(this, context);
    }

    /// <summary>The Yard begins crawling <paramref name="Crawl"/> along its surface, as its crawl animation starts.</summary>
    /// <param name="Crawl">The way the Yard begins to crawl: along which surface, which way.</param>
    internal sealed record BeginCrawling(YardDirection Crawl) : YardCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(YardCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).BeginCrawling(this, context);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(YardCueKind Kind) : YardCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(YardCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).Signal(this, context);
    }
}

/// <summary>What each case of a <see cref="YardCue"/> is made into (#2553).</summary>
/// <typeparam name="TResult">What the cue is made into.</typeparam>
/// <param name="MoveBy">Makes a <see cref="YardCue.MoveBy"/> into the result.</param>
/// <param name="SetDirection">Makes a <see cref="YardCue.SetDirection"/> into the result.</param>
/// <param name="SetHidingAnimation">Makes a <see cref="YardCue.SetHidingAnimation"/> into the result.</param>
/// <param name="BeginCrawling">Makes a <see cref="YardCue.BeginCrawling"/> into the result.</param>
/// <param name="Signal">Makes a <see cref="YardCue.Signal"/> into the result.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed record YardCueCases<TContext, TResult>(Func<YardCue.MoveBy, TContext, TResult> MoveBy, Func<YardCue.SetDirection, TContext, TResult> SetDirection, Func<YardCue.SetHidingAnimation, TContext, TResult> SetHidingAnimation, Func<YardCue.BeginCrawling, TContext, TResult> BeginCrawling, Func<YardCue.Signal, TContext, TResult> Signal);

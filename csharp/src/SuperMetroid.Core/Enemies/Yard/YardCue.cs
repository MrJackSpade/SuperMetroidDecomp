namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>One cue of the Yard's animations: what it does, with the values it acts on.</summary>
internal abstract record YardCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private YardCue()
    {
    }

    /// <summary>What the function for this case makes of the cue, whichever case it is.</summary>
    /// <typeparam name="TResult">What the cue is made into.</typeparam>
    /// <param name="moveBy">Makes a <see cref="MoveBy"/> into the result.</param>
    /// <param name="setDirection">Makes a <see cref="SetDirection"/> into the result.</param>
    /// <param name="setHidingAnimation">Makes a <see cref="SetHidingAnimation"/> into the result.</param>
    /// <param name="beginCrawling">Makes a <see cref="BeginCrawling"/> into the result.</param>
    /// <param name="signal">Makes a <see cref="Signal"/> into the result.</param>
    internal abstract TResult Match<TResult>(Func<MoveBy, TResult> moveBy, Func<SetDirection, TResult> setDirection, Func<SetHidingAnimation, TResult> setHidingAnimation, Func<BeginCrawling, TResult> beginCrawling, Func<Signal, TResult> signal);

    /// <summary>Moves the Yard by a signed pixel offset in X and Y.</summary>
    /// <param name="Offset">The cue's signed pixel offset, horizontal and vertical.</param>
    internal sealed record MoveBy(Offset16<RoomPixels> Offset) : YardCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<MoveBy, TResult> moveBy, Func<SetDirection, TResult> setDirection, Func<SetHidingAnimation, TResult> setHidingAnimation, Func<BeginCrawling, TResult> beginCrawling, Func<Signal, TResult> signal) => Ensure.NotNull(moveBy)(this);
    }

    /// <summary>Sets the attachment direction and the airborne facing it implies.</summary>
    /// <param name="Direction">The surface direction a <see cref="SetDirection"/> cue attaches the Yard in.</param>
    internal sealed record SetDirection(YardDirection Direction) : YardCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<MoveBy, TResult> moveBy, Func<SetDirection, TResult> setDirection, Func<SetHidingAnimation, TResult> setHidingAnimation, Func<BeginCrawling, TResult> beginCrawling, Func<Signal, TResult> signal) => Ensure.NotNull(setDirection)(this);
    }

    /// <summary>Names the animation the Yard hides with, or none.</summary>
    /// <param name="OptionalTarget">The animation the Yard hides with, or none where it does not hide.</param>
    internal sealed record SetHidingAnimation(YardSegment? OptionalTarget) : YardCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<MoveBy, TResult> moveBy, Func<SetDirection, TResult> setDirection, Func<SetHidingAnimation, TResult> setHidingAnimation, Func<BeginCrawling, TResult> beginCrawling, Func<Signal, TResult> signal) => Ensure.NotNull(setHidingAnimation)(this);
    }

    /// <summary>The Yard begins crawling <paramref name="Crawl"/> along its surface, as its crawl animation starts.</summary>
    /// <param name="Crawl">The way the Yard begins to crawl: along which surface, which way.</param>
    internal sealed record BeginCrawling(YardDirection Crawl) : YardCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<MoveBy, TResult> moveBy, Func<SetDirection, TResult> setDirection, Func<SetHidingAnimation, TResult> setHidingAnimation, Func<BeginCrawling, TResult> beginCrawling, Func<Signal, TResult> signal) => Ensure.NotNull(beginCrawling)(this);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(YardCueKind Kind) : YardCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<MoveBy, TResult> moveBy, Func<SetDirection, TResult> setDirection, Func<SetHidingAnimation, TResult> setHidingAnimation, Func<BeginCrawling, TResult> beginCrawling, Func<Signal, TResult> signal) => Ensure.NotNull(signal)(this);
    }
}

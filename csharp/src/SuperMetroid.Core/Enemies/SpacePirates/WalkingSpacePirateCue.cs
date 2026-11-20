namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>One cue of the walking Space Pirates' animations: what it does, with the values it acts on.</summary>
internal abstract record WalkingSpacePirateCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private WalkingSpacePirateCue()
    {
    }

    /// <summary>What the function for this case makes of the cue, whichever case it is.</summary>
    /// <typeparam name="TResult">What the cue is made into.</typeparam>
    /// <param name="fireLaser">Makes a <see cref="FireLaser"/> into the result.</param>
    /// <param name="begin">Makes a <see cref="Begin"/> into the result.</param>
    /// <param name="beginHeading">Makes a <see cref="BeginHeading"/> into the result.</param>
    /// <param name="signal">Makes a <see cref="Signal"/> into the result.</param>
    internal abstract TResult Match<TResult>(Func<FireLaser, TResult> fireLaser, Func<Begin, TResult> begin, Func<BeginHeading, TResult> beginHeading, Func<Signal, TResult> signal);

    /// <summary>Fires a laser the cue's way from its height offset (#2151).</summary>
    /// <param name="Direction">The way the laser flies.</param>
    /// <param name="LaserHeight">How many pixels above the Pirate's origin the laser is emitted; negative is below.</param>
    internal sealed record FireLaser(HorizontalDirection Direction, int LaserHeight) : WalkingSpacePirateCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<FireLaser, TResult> fireLaser, Func<Begin, TResult> begin, Func<BeginHeading, TResult> beginHeading, Func<Signal, TResult> signal) => Ensure.NotNull(fireLaser)(this);
    }

    /// <summary>
    /// Begins the stand or walk the Pirate's main AI then runs, from the next update, as the animation runs on (#2151).
    /// </summary>
    /// <param name="Action">The action main AI then runs.</param>
    internal sealed record Begin(WalkingSpacePirateAction Action) : WalkingSpacePirateCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<FireLaser, TResult> fireLaser, Func<Begin, TResult> begin, Func<BeginHeading, TResult> beginHeading, Func<Signal, TResult> signal) => Ensure.NotNull(begin)(this);
    }

    /// <summary>Begins the walk the Pirate's main AI then runs the cue's way, from the next update, as the animation runs on (#2165).</summary>
    /// <param name="Action">The action main AI then runs.</param>
    /// <param name="Heading">The way the Pirate walks.</param>
    internal sealed record BeginHeading(WalkingSpacePirateAction Action, HorizontalDirection Heading) : WalkingSpacePirateCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<FireLaser, TResult> fireLaser, Func<Begin, TResult> begin, Func<BeginHeading, TResult> beginHeading, Func<Signal, TResult> signal) => Ensure.NotNull(beginHeading)(this);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(WalkingSpacePirateCueKind Kind) : WalkingSpacePirateCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<FireLaser, TResult> fireLaser, Func<Begin, TResult> begin, Func<BeginHeading, TResult> beginHeading, Func<Signal, TResult> signal) => Ensure.NotNull(signal)(this);
    }
}

using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>One cue of the ninja Space Pirates' animations: what it does, with the values it acts on.</summary>
internal abstract record NinjaSpacePirateCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private NinjaSpacePirateCue()
    {
    }

    /// <summary>What the function for this case makes of the cue, whichever case it is.</summary>
    /// <typeparam name="TResult">What the cue is made into.</typeparam>
    /// <param name="playSound">Makes a <see cref="PlaySound"/> into the result.</param>
    /// <param name="setShading">Makes a <see cref="SetShading"/> into the result.</param>
    /// <param name="throwClaw">Makes a <see cref="ThrowClaw"/> into the result.</param>
    /// <param name="begin">Makes a <see cref="Begin"/> into the result.</param>
    /// <param name="beginHeading">Makes a <see cref="BeginHeading"/> into the result.</param>
    /// <param name="signal">Makes a <see cref="Signal"/> into the result.</param>
    internal abstract TResult Match<TResult>(Func<PlaySound, TResult> playSound, Func<SetShading, TResult> setShading, Func<ThrowClaw, TResult> throwClaw, Func<Begin, TResult> begin, Func<BeginHeading, TResult> beginHeading, Func<Signal, TResult> signal);

    /// <summary>Plays the cue's enemy-library sound.</summary>
    /// <param name="Sound">The enemy-library sound it plays.</param>
    internal sealed record PlaySound(SoundEffectId Sound) : NinjaSpacePirateCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<PlaySound, TResult> playSound, Func<SetShading, TResult> setShading, Func<ThrowClaw, TResult> throwClaw, Func<Begin, TResult> begin, Func<BeginHeading, TResult> beginHeading, Func<Signal, TResult> signal) => Ensure.NotNull(playSound)(this);
    }

    /// <summary>Sets the look the Pirate draws in.</summary>
    /// <param name="Shading">The shading the Pirate is drawn in from then on (#1512).</param>
    internal sealed record SetShading(PictureShading Shading) : NinjaSpacePirateCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<PlaySound, TResult> playSound, Func<SetShading, TResult> setShading, Func<ThrowClaw, TResult> throwClaw, Func<Begin, TResult> begin, Func<BeginHeading, TResult> beginHeading, Func<Signal, TResult> signal) => Ensure.NotNull(setShading)(this);
    }

    /// <summary>Throws a claw in the cue's direction from the given offset.</summary>
    /// <param name="Throw">For a claw cue: the way the claw flies.</param>
    /// <param name="Offset">The cue's signed pixel offset, horizontal and vertical.</param>
    internal sealed record ThrowClaw(HorizontalDirection Throw, Offset16<RoomPixels> Offset) : NinjaSpacePirateCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<PlaySound, TResult> playSound, Func<SetShading, TResult> setShading, Func<ThrowClaw, TResult> throwClaw, Func<Begin, TResult> begin, Func<BeginHeading, TResult> beginHeading, Func<Signal, TResult> signal) => Ensure.NotNull(throwClaw)(this);
    }

    /// <summary>
    /// Begins the move the Pirate's main AI then runs, from the next update, as the animation runs on (#2151): a stand
    /// while the animation poses it, the wait at its post, the stance that chooses an attack, a spin jump, a divekick's
    /// steps or the walk back to its post.
    /// </summary>
    /// <param name="Action">The action main AI then runs.</param>
    internal sealed record Begin(NinjaSpacePirateAction Action) : NinjaSpacePirateCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<PlaySound, TResult> playSound, Func<SetShading, TResult> setShading, Func<ThrowClaw, TResult> throwClaw, Func<Begin, TResult> begin, Func<BeginHeading, TResult> beginHeading, Func<Signal, TResult> signal) => Ensure.NotNull(begin)(this);
    }

    /// <summary>
    /// Begins the spin jump, divekick step or walk back to its post the Pirate's main AI then runs the cue's way, from the
    /// next update, as the animation runs on (#2165).
    /// </summary>
    /// <param name="Action">The action main AI then runs.</param>
    /// <param name="Heading">The way the action goes.</param>
    internal sealed record BeginHeading(NinjaSpacePirateAction Action, HorizontalDirection Heading) : NinjaSpacePirateCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<PlaySound, TResult> playSound, Func<SetShading, TResult> setShading, Func<ThrowClaw, TResult> throwClaw, Func<Begin, TResult> begin, Func<BeginHeading, TResult> beginHeading, Func<Signal, TResult> signal) => Ensure.NotNull(beginHeading)(this);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(NinjaSpacePirateCueKind Kind) : NinjaSpacePirateCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<PlaySound, TResult> playSound, Func<SetShading, TResult> setShading, Func<ThrowClaw, TResult> throwClaw, Func<Begin, TResult> begin, Func<BeginHeading, TResult> beginHeading, Func<Signal, TResult> signal) => Ensure.NotNull(signal)(this);
    }
}

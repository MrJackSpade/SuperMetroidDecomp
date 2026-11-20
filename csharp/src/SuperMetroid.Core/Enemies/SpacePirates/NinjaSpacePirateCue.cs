using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>One cue of the ninja Space Pirates' animations: what it does, with the values it acts on.</summary>
internal abstract record NinjaSpacePirateCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private NinjaSpacePirateCue()
    {
    }

    /// <summary>What <paramref name="cases"/> makes of the cue, through the case it is.</summary>
    /// <typeparam name="TResult">What the cue is made into.</typeparam>
    /// <param name="cases">What each case of the cue is made into.</param>
    internal abstract TResult Match<TResult>(NinjaSpacePirateCueCases<TResult> cases);

    /// <summary>Plays the cue's enemy-library sound.</summary>
    /// <param name="Sound">The enemy-library sound it plays.</param>
    internal sealed record PlaySound(SoundEffectId Sound) : NinjaSpacePirateCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(NinjaSpacePirateCueCases<TResult> cases) => Ensure.NotNull(cases).PlaySound(this);
    }

    /// <summary>Sets the look the Pirate draws in.</summary>
    /// <param name="Shading">The shading the Pirate is drawn in from then on (#1512).</param>
    internal sealed record SetShading(PictureShading Shading) : NinjaSpacePirateCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(NinjaSpacePirateCueCases<TResult> cases) => Ensure.NotNull(cases).SetShading(this);
    }

    /// <summary>Throws a claw in the cue's direction from the given offset.</summary>
    /// <param name="Throw">For a claw cue: the way the claw flies.</param>
    /// <param name="Offset">The cue's signed pixel offset, horizontal and vertical.</param>
    internal sealed record ThrowClaw(HorizontalDirection Throw, Offset16<RoomPixels> Offset) : NinjaSpacePirateCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(NinjaSpacePirateCueCases<TResult> cases) => Ensure.NotNull(cases).ThrowClaw(this);
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
        internal override TResult Match<TResult>(NinjaSpacePirateCueCases<TResult> cases) => Ensure.NotNull(cases).Begin(this);
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
        internal override TResult Match<TResult>(NinjaSpacePirateCueCases<TResult> cases) => Ensure.NotNull(cases).BeginHeading(this);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(NinjaSpacePirateCueKind Kind) : NinjaSpacePirateCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(NinjaSpacePirateCueCases<TResult> cases) => Ensure.NotNull(cases).Signal(this);
    }
}

/// <summary>What each case of a <see cref="NinjaSpacePirateCue"/> is made into (#2553).</summary>
/// <typeparam name="TResult">What the cue is made into.</typeparam>
/// <param name="PlaySound">Makes a <see cref="NinjaSpacePirateCue.PlaySound"/> into the result.</param>
/// <param name="SetShading">Makes a <see cref="NinjaSpacePirateCue.SetShading"/> into the result.</param>
/// <param name="ThrowClaw">Makes a <see cref="NinjaSpacePirateCue.ThrowClaw"/> into the result.</param>
/// <param name="Begin">Makes a <see cref="NinjaSpacePirateCue.Begin"/> into the result.</param>
/// <param name="BeginHeading">Makes a <see cref="NinjaSpacePirateCue.BeginHeading"/> into the result.</param>
/// <param name="Signal">Makes a <see cref="NinjaSpacePirateCue.Signal"/> into the result.</param>
internal sealed record NinjaSpacePirateCueCases<TResult>(Func<NinjaSpacePirateCue.PlaySound, TResult> PlaySound, Func<NinjaSpacePirateCue.SetShading, TResult> SetShading, Func<NinjaSpacePirateCue.ThrowClaw, TResult> ThrowClaw, Func<NinjaSpacePirateCue.Begin, TResult> Begin, Func<NinjaSpacePirateCue.BeginHeading, TResult> BeginHeading, Func<NinjaSpacePirateCue.Signal, TResult> Signal);

using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>One cue of the ninja Space Pirates' animations: what it does, with the values it acts on.</summary>
internal abstract partial record NinjaSpacePirateCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private NinjaSpacePirateCue()
    {
    }

    /// <summary>What <paramref name="cases"/> makes of the cue, through the case it is.</summary>
    /// <typeparam name="TResult">What the cue is made into.</typeparam>
    /// <param name="cases">What each case of the cue is made into.</param>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract TResult Match<TContext, TResult>(NinjaSpacePirateCueCases<TContext, TResult> cases, TContext context);

    /// <summary>Plays the cue's enemy-library sound.</summary>
    /// <param name="Sound">The enemy-library sound it plays.</param>
    internal sealed partial record PlaySound(SoundEffectId Sound) : NinjaSpacePirateCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(NinjaSpacePirateCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).PlaySound(this, context);
    }

    /// <summary>Sets the look the Pirate draws in.</summary>
    /// <param name="Shading">The shading the Pirate is drawn in from then on (#1512).</param>
    internal sealed partial record SetShading(PictureShading Shading) : NinjaSpacePirateCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(NinjaSpacePirateCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).SetShading(this, context);
    }

    /// <summary>Throws a claw in the cue's direction from the given offset.</summary>
    /// <param name="Throw">For a claw cue: the way the claw flies.</param>
    /// <param name="Offset">The cue's signed pixel offset, horizontal and vertical.</param>
    internal sealed partial record ThrowClaw(HorizontalDirection Throw, Offset16<RoomPixels> Offset) : NinjaSpacePirateCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(NinjaSpacePirateCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).ThrowClaw(this, context);
    }

    /// <summary>
    /// Begins the move the Pirate's main AI then runs, from the next update, as the animation runs on (#2151): a stand
    /// while the animation poses it, the wait at its post, the stance that chooses an attack, a spin jump, a divekick's
    /// steps or the walk back to its post.
    /// </summary>
    /// <param name="Action">The action main AI then runs.</param>
    internal sealed partial record Begin(NinjaSpacePirateAction Action) : NinjaSpacePirateCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(NinjaSpacePirateCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).Begin(this, context);
    }

    /// <summary>
    /// Begins the spin jump, divekick step or walk back to its post the Pirate's main AI then runs the cue's way, from the
    /// next update, as the animation runs on (#2165).
    /// </summary>
    /// <param name="Action">The action main AI then runs.</param>
    /// <param name="Heading">The way the action goes.</param>
    internal sealed partial record BeginHeading(NinjaSpacePirateAction Action, HorizontalDirection Heading) : NinjaSpacePirateCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(NinjaSpacePirateCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).BeginHeading(this, context);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed partial record Signal(NinjaSpacePirateCueKind Kind) : NinjaSpacePirateCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(NinjaSpacePirateCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).Signal(this, context);
    }
}

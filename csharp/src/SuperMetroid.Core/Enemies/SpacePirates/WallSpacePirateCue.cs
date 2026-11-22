namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>One cue of the wall Space Pirates' animations: what it does, with the values it acts on.</summary>
internal abstract partial record WallSpacePirateCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private WallSpacePirateCue()
    {
    }

    /// <summary>What <paramref name="cases"/> makes of the cue, through the case it is.</summary>
    /// <typeparam name="TResult">What the cue is made into.</typeparam>
    /// <param name="cases">What each case of the cue is made into.</param>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract TResult Match<TContext, TResult>(WallSpacePirateCueCases<TContext, TResult> cases, TContext context);

    /// <summary>Climbs the cue's pixels on the cue's wall, continuing at the opposite climb after terrain contact (#2151).</summary>
    /// <param name="Wall">The wall the Pirate climbs.</param>
    /// <param name="ClimbPixels">The whole pixels the Pirate moves down the wall; negative climbs up.</param>
    internal sealed partial record Climb(HorizontalDirection Wall, int ClimbPixels) : WallSpacePirateCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(WallSpacePirateCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).Climb(this, context);
    }

    /// <summary>Picks a random climb direction and continues at that climb on the cue's wall (#2151).</summary>
    /// <param name="Wall">The wall the Pirate climbs.</param>
    internal sealed partial record ChooseClimb(HorizontalDirection Wall) : WallSpacePirateCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(WallSpacePirateCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).ChooseClimb(this, context);
    }

    /// <summary>Fires a laser the cue's way (#2151).</summary>
    /// <param name="Direction">The way the laser flies.</param>
    internal sealed partial record FireLaser(HorizontalDirection Direction) : WallSpacePirateCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(WallSpacePirateCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).FireLaser(this, context);
    }

    /// <summary>Prepares the jump to the wall the cue's way (#2151).</summary>
    /// <param name="Direction">The way the Pirate jumps.</param>
    internal sealed partial record PrepareJump(HorizontalDirection Direction) : WallSpacePirateCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(WallSpacePirateCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).PrepareJump(this, context);
    }

    /// <summary>
    /// Begins the cling, attack or jump the Pirate's main AI then runs, as the animation runs on (#2151): clinging to a
    /// wall and watching Samus as it climbs, holding on while its attack fires, or jumping across to the other wall.
    /// </summary>
    /// <param name="Action">The action main AI then runs.</param>
    internal sealed partial record Begin(WallSpacePirateAction Action) : WallSpacePirateCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(WallSpacePirateCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).Begin(this, context);
    }

    /// <summary>
    /// Begins the cling or jump the Pirate's main AI then runs on the cue's wall, as the animation runs on (#2165): clinging
    /// to that wall and watching Samus as it climbs, or jumping across to it.
    /// </summary>
    /// <param name="Action">The action main AI then runs.</param>
    /// <param name="Wall">The wall the Pirate clings to, or jumps to.</param>
    internal sealed partial record BeginOnWall(WallSpacePirateAction Action, HorizontalDirection Wall) : WallSpacePirateCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(WallSpacePirateCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).BeginOnWall(this, context);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed partial record Signal(WallSpacePirateCueKind Kind) : WallSpacePirateCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(WallSpacePirateCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).Signal(this, context);
    }
}

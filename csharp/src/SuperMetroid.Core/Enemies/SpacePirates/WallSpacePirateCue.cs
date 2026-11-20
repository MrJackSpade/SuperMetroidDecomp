namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>One cue of the wall Space Pirates' animations: what it does, with the values it acts on.</summary>
internal abstract record WallSpacePirateCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private WallSpacePirateCue()
    {
    }

    /// <summary>What the function for this case makes of the cue, whichever case it is.</summary>
    /// <typeparam name="TResult">What the cue is made into.</typeparam>
    /// <param name="climb">Makes a <see cref="Climb"/> into the result.</param>
    /// <param name="chooseClimb">Makes a <see cref="ChooseClimb"/> into the result.</param>
    /// <param name="fireLaser">Makes a <see cref="FireLaser"/> into the result.</param>
    /// <param name="prepareJump">Makes a <see cref="PrepareJump"/> into the result.</param>
    /// <param name="begin">Makes a <see cref="Begin"/> into the result.</param>
    /// <param name="beginOnWall">Makes a <see cref="BeginOnWall"/> into the result.</param>
    /// <param name="signal">Makes a <see cref="Signal"/> into the result.</param>
    internal abstract TResult Match<TResult>(Func<Climb, TResult> climb, Func<ChooseClimb, TResult> chooseClimb, Func<FireLaser, TResult> fireLaser, Func<PrepareJump, TResult> prepareJump, Func<Begin, TResult> begin, Func<BeginOnWall, TResult> beginOnWall, Func<Signal, TResult> signal);

    /// <summary>Climbs the cue's pixels on the cue's wall, continuing at the opposite climb after terrain contact (#2151).</summary>
    /// <param name="Wall">The wall the Pirate climbs.</param>
    /// <param name="ClimbPixels">The whole pixels the Pirate moves down the wall; negative climbs up.</param>
    internal sealed record Climb(HorizontalDirection Wall, int ClimbPixels) : WallSpacePirateCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<Climb, TResult> climb, Func<ChooseClimb, TResult> chooseClimb, Func<FireLaser, TResult> fireLaser, Func<PrepareJump, TResult> prepareJump, Func<Begin, TResult> begin, Func<BeginOnWall, TResult> beginOnWall, Func<Signal, TResult> signal) => Ensure.NotNull(climb)(this);
    }

    /// <summary>Picks a random climb direction and continues at that climb on the cue's wall (#2151).</summary>
    /// <param name="Wall">The wall the Pirate climbs.</param>
    internal sealed record ChooseClimb(HorizontalDirection Wall) : WallSpacePirateCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<Climb, TResult> climb, Func<ChooseClimb, TResult> chooseClimb, Func<FireLaser, TResult> fireLaser, Func<PrepareJump, TResult> prepareJump, Func<Begin, TResult> begin, Func<BeginOnWall, TResult> beginOnWall, Func<Signal, TResult> signal) => Ensure.NotNull(chooseClimb)(this);
    }

    /// <summary>Fires a laser the cue's way (#2151).</summary>
    /// <param name="Direction">The way the laser flies.</param>
    internal sealed record FireLaser(HorizontalDirection Direction) : WallSpacePirateCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<Climb, TResult> climb, Func<ChooseClimb, TResult> chooseClimb, Func<FireLaser, TResult> fireLaser, Func<PrepareJump, TResult> prepareJump, Func<Begin, TResult> begin, Func<BeginOnWall, TResult> beginOnWall, Func<Signal, TResult> signal) => Ensure.NotNull(fireLaser)(this);
    }

    /// <summary>Prepares the jump to the wall the cue's way (#2151).</summary>
    /// <param name="Direction">The way the Pirate jumps.</param>
    internal sealed record PrepareJump(HorizontalDirection Direction) : WallSpacePirateCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<Climb, TResult> climb, Func<ChooseClimb, TResult> chooseClimb, Func<FireLaser, TResult> fireLaser, Func<PrepareJump, TResult> prepareJump, Func<Begin, TResult> begin, Func<BeginOnWall, TResult> beginOnWall, Func<Signal, TResult> signal) => Ensure.NotNull(prepareJump)(this);
    }

    /// <summary>
    /// Begins the cling, attack or jump the Pirate's main AI then runs, as the animation runs on (#2151): clinging to a
    /// wall and watching Samus as it climbs, holding on while its attack fires, or jumping across to the other wall.
    /// </summary>
    /// <param name="Action">The action main AI then runs.</param>
    internal sealed record Begin(WallSpacePirateAction Action) : WallSpacePirateCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<Climb, TResult> climb, Func<ChooseClimb, TResult> chooseClimb, Func<FireLaser, TResult> fireLaser, Func<PrepareJump, TResult> prepareJump, Func<Begin, TResult> begin, Func<BeginOnWall, TResult> beginOnWall, Func<Signal, TResult> signal) => Ensure.NotNull(begin)(this);
    }

    /// <summary>
    /// Begins the cling or jump the Pirate's main AI then runs on the cue's wall, as the animation runs on (#2165): clinging
    /// to that wall and watching Samus as it climbs, or jumping across to it.
    /// </summary>
    /// <param name="Action">The action main AI then runs.</param>
    /// <param name="Wall">The wall the Pirate clings to, or jumps to.</param>
    internal sealed record BeginOnWall(WallSpacePirateAction Action, HorizontalDirection Wall) : WallSpacePirateCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<Climb, TResult> climb, Func<ChooseClimb, TResult> chooseClimb, Func<FireLaser, TResult> fireLaser, Func<PrepareJump, TResult> prepareJump, Func<Begin, TResult> begin, Func<BeginOnWall, TResult> beginOnWall, Func<Signal, TResult> signal) => Ensure.NotNull(beginOnWall)(this);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(WallSpacePirateCueKind Kind) : WallSpacePirateCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<Climb, TResult> climb, Func<ChooseClimb, TResult> chooseClimb, Func<FireLaser, TResult> fireLaser, Func<PrepareJump, TResult> prepareJump, Func<Begin, TResult> begin, Func<BeginOnWall, TResult> beginOnWall, Func<Signal, TResult> signal) => Ensure.NotNull(signal)(this);
    }
}

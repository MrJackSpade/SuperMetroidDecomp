namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>One cue of the wall Space Pirates' animations: what it does, with the values it acts on.</summary>
internal abstract record WallSpacePirateCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private WallSpacePirateCue()
    {
    }

    /// <summary>Climbs the cue's pixels on the cue's wall, continuing at the opposite climb after terrain contact (#2151).</summary>
    /// <param name="Wall">The wall the Pirate climbs.</param>
    /// <param name="ClimbPixels">The whole pixels the Pirate moves down the wall; negative climbs up.</param>
    internal sealed record Climb(HorizontalDirection Wall, int ClimbPixels) : WallSpacePirateCue;

    /// <summary>Picks a random climb direction and continues at that climb on the cue's wall (#2151).</summary>
    /// <param name="Wall">The wall the Pirate climbs.</param>
    internal sealed record ChooseClimb(HorizontalDirection Wall) : WallSpacePirateCue;

    /// <summary>Fires a laser the cue's way (#2151).</summary>
    /// <param name="Direction">The way the laser flies.</param>
    internal sealed record FireLaser(HorizontalDirection Direction) : WallSpacePirateCue;

    /// <summary>Prepares the jump to the wall the cue's way (#2151).</summary>
    /// <param name="Direction">The way the Pirate jumps.</param>
    internal sealed record PrepareJump(HorizontalDirection Direction) : WallSpacePirateCue;

    /// <summary>
    /// Begins the cling, attack or jump the Pirate's main AI then runs, as the animation runs on (#2151): clinging to a
    /// wall and watching Samus as it climbs, holding on while its attack fires, or jumping across to the other wall.
    /// </summary>
    /// <param name="Action">The action main AI then runs.</param>
    internal sealed record Begin(WallSpacePirateAction Action) : WallSpacePirateCue;

    /// <summary>
    /// Begins the cling or jump the Pirate's main AI then runs on the cue's wall, as the animation runs on (#2165): clinging
    /// to that wall and watching Samus as it climbs, or jumping across to it.
    /// </summary>
    /// <param name="Action">The action main AI then runs.</param>
    /// <param name="Wall">The wall the Pirate clings to, or jumps to.</param>
    internal sealed record BeginOnWall(WallSpacePirateAction Action, HorizontalDirection Wall) : WallSpacePirateCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(WallSpacePirateCueKind Kind) : WallSpacePirateCue;
}

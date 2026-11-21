namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>One cue of the walking Space Pirates' animations: what it does, with the values it acts on.</summary>
internal abstract partial record WalkingSpacePirateCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private WalkingSpacePirateCue()
    {
    }

    /// <summary>Fires a laser the cue's way from its height offset (#2151).</summary>
    /// <param name="Direction">The way the laser flies.</param>
    /// <param name="LaserHeight">How many pixels above the Pirate's origin the laser is emitted; negative is below.</param>
    internal sealed partial record FireLaser(HorizontalDirection Direction, int LaserHeight) : WalkingSpacePirateCue;

    /// <summary>
    /// Begins the stand or walk the Pirate's main AI then runs, from the next update, as the animation runs on (#2151).
    /// </summary>
    /// <param name="Action">The action main AI then runs.</param>
    internal sealed partial record Begin(WalkingSpacePirateAction Action) : WalkingSpacePirateCue;

    /// <summary>Begins the walk the Pirate's main AI then runs the cue's way, from the next update, as the animation runs on (#2165).</summary>
    /// <param name="Action">The action main AI then runs.</param>
    /// <param name="Heading">The way the Pirate walks.</param>
    internal sealed partial record BeginHeading(WalkingSpacePirateAction Action, HorizontalDirection Heading) : WalkingSpacePirateCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed partial record Signal(WalkingSpacePirateCueKind Kind) : WalkingSpacePirateCue;
}

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>One cue of the wall Space Pirates' animations: what it does, with the values it acts on.</summary>
internal abstract record WallSpacePirateCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private WallSpacePirateCue()
    {
    }

    /// <summary>Climbs the cue's pixels on the left wall, continuing at the opposite climb after terrain contact.</summary>
    /// <param name="ClimbPixels">For a climb cue: the whole pixels the Pirate moves down the wall; negative climbs up.</param>
    internal sealed record ClimbLeftWall(int ClimbPixels) : WallSpacePirateCue;

    /// <summary>Climbs the cue's pixels on the right wall, continuing at the opposite climb after terrain contact.</summary>
    /// <param name="ClimbPixels">For a climb cue: the whole pixels the Pirate moves down the wall; negative climbs up.</param>
    internal sealed record ClimbRightWall(int ClimbPixels) : WallSpacePirateCue;

    /// <summary>Sets the function main AI runs next.</summary>
    /// <param name="Function">The function main AI runs next.</param>
    internal sealed record SetFunction(WallSpacePirateFunction Function) : WallSpacePirateCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(WallSpacePirateCueKind Kind) : WallSpacePirateCue;
}

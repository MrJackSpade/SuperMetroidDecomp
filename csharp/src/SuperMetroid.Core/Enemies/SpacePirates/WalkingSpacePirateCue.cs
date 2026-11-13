namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>One cue of the walking Space Pirates' animations, with its typed operands.</summary>
internal abstract record WalkingSpacePirateCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private WalkingSpacePirateCue()
    {
    }

    /// <summary>Fires a laser left from the operand's height offset.</summary>
    /// <param name="LaserHeight">For a laser cue: how many pixels above the Pirate's origin the laser is emitted; negative is below.</param>
    internal sealed record FireLaserLeft(int LaserHeight) : WalkingSpacePirateCue;

    /// <summary>Fires a laser right from the operand's height offset.</summary>
    /// <param name="LaserHeight">For a laser cue: how many pixels above the Pirate's origin the laser is emitted; negative is below.</param>
    internal sealed record FireLaserRight(int LaserHeight) : WalkingSpacePirateCue;

    /// <summary>Sets the function main AI runs next.</summary>
    /// <param name="Function">The cue's Function operand.</param>
    internal sealed record SetFunction(WalkingSpacePirateFunction Function) : WalkingSpacePirateCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(WalkingSpacePirateCueKind Kind) : WalkingSpacePirateCue;
}

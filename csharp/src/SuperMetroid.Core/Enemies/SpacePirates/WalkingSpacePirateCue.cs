namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>One cue of the walking Space Pirates' programs, with the operands its native instruction carries.</summary>
internal abstract record WalkingSpacePirateCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private WalkingSpacePirateCue()
    {
    }

    /// <summary>Fires a laser left from the operand's height offset (<c>Instruction_PirateWalking_FireLaserLeftWithYOffsetInY</c>, $B2:FC68).</summary>
    /// <param name="LaserHeight">For a laser cue: how many pixels above the Pirate's origin the laser is emitted; negative is below.</param>
    internal sealed record FireLaserLeft(short LaserHeight) : WalkingSpacePirateCue;

    /// <summary>Fires a laser right from the operand's height offset (<c>Instruction_PirateWalking_FireLaserRightWithYOffsetInY</c>, $B2:FC90).</summary>
    /// <param name="LaserHeight">For a laser cue: how many pixels above the Pirate's origin the laser is emitted; negative is below.</param>
    internal sealed record FireLaserRight(short LaserHeight) : WalkingSpacePirateCue;

    /// <summary>Sets the function main AI runs next (<c>Instruction_PirateWalking_FunctionInY</c>, $B2:FCB8).</summary>
    /// <param name="Function">The cue's Function operand.</param>
    internal sealed record SetFunction(WalkingSpacePirateFunction Function) : WalkingSpacePirateCue;

    /// <summary>A cue that carries no operand.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Command(WalkingSpacePirateCueKind Kind) : WalkingSpacePirateCue;
}

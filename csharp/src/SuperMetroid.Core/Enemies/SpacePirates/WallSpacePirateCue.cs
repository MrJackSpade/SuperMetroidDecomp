namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>One cue of the wall Space Pirates' programs, with the operands its native instruction carries.</summary>
internal abstract record WallSpacePirateCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private WallSpacePirateCue()
    {
    }

    /// <summary>Climbs the operand's pixels on the left wall, continuing at the opposite climb after terrain contact (<c>Inst_PirateWall_MoveYPixelsDown_ChangeDirOnCollision_Left</c>, $B2:EE40).</summary>
    /// <param name="ClimbPixels">For a climb cue: the whole pixels the Pirate moves down the wall; negative climbs up.</param>
    internal sealed record ClimbLeftWall(short ClimbPixels) : WallSpacePirateCue;

    /// <summary>Climbs the operand's pixels on the right wall, continuing at the opposite climb after terrain contact (<c>Inst_PirateWall_MoveYPixelsDown_ChangeDirOnCollision_Right</c>, $B2:EE72).</summary>
    /// <param name="ClimbPixels">For a climb cue: the whole pixels the Pirate moves down the wall; negative climbs up.</param>
    internal sealed record ClimbRightWall(short ClimbPixels) : WallSpacePirateCue;

    /// <summary>Sets the function main AI runs next (<c>Instruction_PirateWall_FunctionInY</c>, $B2:EF83).</summary>
    /// <param name="Function">The cue's Function operand.</param>
    internal sealed record SetFunction(WallSpacePirateFunction Function) : WallSpacePirateCue;

    /// <summary>A cue that carries no operand.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Command(WallSpacePirateCueKind Kind) : WallSpacePirateCue;
}

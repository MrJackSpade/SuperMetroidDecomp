namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>The fixed animation selections Shaktool's mechanics state machine makes.</summary>
internal static class ShaktoolInstructionDefinitions
{
    /// <summary>The eight head animations in their native storage order, starting aimed left and turning clockwise.</summary>
    private static readonly ShaktoolProgram[] HeadAims =
    [
        ShaktoolProgram.HeadAimingLeft,
        ShaktoolProgram.HeadAimingUpLeft,
        ShaktoolProgram.HeadAimingUp,
        ShaktoolProgram.HeadAimingUpRight,
        ShaktoolProgram.HeadAimingRight,
        ShaktoolProgram.HeadAimingDownRight,
        ShaktoolProgram.HeadAimingDown,
        ShaktoolProgram.HeadAimingDownLeft,
    ];

    /// <summary>
    /// Calculates $AA:DD15-$DD24 from the eight head animations stored from $AA:DAA4.
    /// The direction bucket starts at up; program storage starts two eighth-turns earlier at left.
    /// </summary>
    internal static ShaktoolProgram ForOrientationBucket(ushort directionBucket)
    {
        if ((directionBucket & 0x001f) != 0 || directionBucket > 0x00e0)
        {
            throw new InvalidDataException(
                $"Shaktool orientation bucket ${directionBucket:X4} is invalid.");
        }

        int program = ((directionBucket >> 5) + 2) & 7;
        return HeadAims[program];
    }

    /// <summary>Returns the collision-recovery animation for one physical segment.</summary>
    internal static ShaktoolProgram CollisionForSegment(int segmentIndex) =>
        ForSegment(segmentIndex).CollisionProgram;

    /// <summary>
    /// $AA:DF13 and $AA:DF21 select collision and dormant-attack behavior by body part:
    /// primary saw, rear arm, front arm, head, front arm, rear arm, final saw.
    /// </summary>
    private static ShaktoolSegmentInstructionDefinition ForSegment(int segmentIndex)
    {
        if ((uint)segmentIndex >= 7)
        {
            throw new InvalidDataException(
                $"Shaktool instruction segment {segmentIndex} is outside seven records.");
        }

        return segmentIndex switch
        {
            0 => new(ShaktoolProgram.SawHandHeadBobPrimaryPiece),
            1 or 5 => new(ShaktoolProgram.ArmPieceHeadBobBack),
            2 or 4 => new(ShaktoolProgram.ArmPieceHeadBobFront),
            3 => new(ShaktoolProgram.HeadHeadBob),
            _ => new(ShaktoolProgram.SawHandHeadBobFinalPiece),
        };
    }
}

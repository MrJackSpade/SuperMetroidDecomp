namespace SuperMetroid.Core.Game;

/// <summary>Collision-recovery and dormant-attack programs for one Shaktool segment.</summary>
internal readonly record struct ShaktoolSegmentInstructionDefinition(
    ushort CollisionInstruction,
    ushort AttackInstruction);

/// <summary>Compiled fixed instruction selectors used by Shaktool's mechanics state machine.</summary>
internal static class ShaktoolInstructionDefinitions
{
    /// <summary>
    /// Calculates $AA:DD15-$DD24 from the eight-byte head programs at $AA:DAA4.
    /// The direction bucket starts at up; program storage starts two eighth-turns earlier at left.
    /// </summary>
    internal static ushort ForOrientationBucket(ushort directionBucket)
    {
        if ((directionBucket & 0x001f) != 0 || directionBucket > 0x00e0)
        {
            throw new InvalidDataException(
                $"Shaktool orientation bucket ${directionBucket:X4} is invalid.");
        }

        int program = ((directionBucket >> 5) + 2) & 7;
        return (ushort)(ShaktoolInstructionProgramDefinitions.HeadAimingLeft + 8 * program);
    }

    /// <summary>Returns the collision-recovery list for one physical segment.</summary>
    internal static ushort CollisionForSegment(int segmentIndex) =>
        ForSegment(segmentIndex).CollisionInstruction;

    /// <summary>Returns the dormant retail attack list for one physical segment.</summary>
    internal static ushort AttackForSegment(int segmentIndex) =>
        ForSegment(segmentIndex).AttackInstruction;

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
            0 => new(ShaktoolInstructionProgramDefinitions.SawHandHeadBobPrimaryPiece,
                ShaktoolInstructionProgramDefinitions.SawHandAttackPrimaryPiece),
            1 or 5 => new(ShaktoolInstructionProgramDefinitions.ArmPieceHeadBobBack,
                ShaktoolInstructionProgramDefinitions.ArmPieceAttackBack),
            2 or 4 => new(ShaktoolInstructionProgramDefinitions.ArmPieceHeadBobFront,
                ShaktoolInstructionProgramDefinitions.ArmPieceAttackFront),
            3 => new(ShaktoolInstructionProgramDefinitions.HeadHeadBob,
                ShaktoolInstructionProgramDefinitions.HeadAttack),
            _ => new(ShaktoolInstructionProgramDefinitions.SawHandHeadBobFinalPiece,
                ShaktoolInstructionProgramDefinitions.SawHandAttackFinalPiece),
        };
    }
}

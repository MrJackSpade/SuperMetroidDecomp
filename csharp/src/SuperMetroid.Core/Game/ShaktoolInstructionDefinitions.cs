namespace SuperMetroid.Core.Game;

/// <summary>Collision-recovery and dormant-attack programs for one Shaktool segment.</summary>
internal readonly record struct ShaktoolSegmentInstructionDefinition(
    ushort CollisionInstruction,
    ushort AttackInstruction);

/// <summary>Compiled fixed instruction selectors used by Shaktool's mechanics state machine.</summary>
internal static class ShaktoolInstructionDefinitions
{
    /// <summary>The eight center-orientation instruction lists at <c>$AA:DD15-$DD24</c>.</summary>
    private static readonly ushort[] OrientationInstructions =
    [
        ShaktoolInstructionProgramDefinitions.HeadAimingUp,
        ShaktoolInstructionProgramDefinitions.HeadAimingUpRight,
        ShaktoolInstructionProgramDefinitions.HeadAimingRight,
        ShaktoolInstructionProgramDefinitions.HeadAimingDownRight,
        ShaktoolInstructionProgramDefinitions.HeadAimingDown,
        ShaktoolInstructionProgramDefinitions.HeadAimingDownLeft,
        ShaktoolInstructionProgramDefinitions.HeadAimingLeft,
        ShaktoolInstructionProgramDefinitions.HeadAimingUpLeft,
    ];

    /// <summary>
    /// The seven collision lists at <c>$AA:DF13-$DF20</c> and parallel dormant-attack
    /// lists at <c>$AA:DF21-$DF2E</c>.
    /// </summary>
    private static readonly ShaktoolSegmentInstructionDefinition[] SegmentInstructions =
    [
        new(ShaktoolInstructionProgramDefinitions.SawHandHeadBobPrimaryPiece,
            ShaktoolInstructionProgramDefinitions.SawHandAttackPrimaryPiece),
        new(ShaktoolInstructionProgramDefinitions.ArmPieceHeadBobBack,
            ShaktoolInstructionProgramDefinitions.ArmPieceAttackBack),
        new(ShaktoolInstructionProgramDefinitions.ArmPieceHeadBobFront,
            ShaktoolInstructionProgramDefinitions.ArmPieceAttackFront),
        new(ShaktoolInstructionProgramDefinitions.HeadHeadBob,
            ShaktoolInstructionProgramDefinitions.HeadAttack),
        new(ShaktoolInstructionProgramDefinitions.ArmPieceHeadBobFront,
            ShaktoolInstructionProgramDefinitions.ArmPieceAttackFront),
        new(ShaktoolInstructionProgramDefinitions.ArmPieceHeadBobBack,
            ShaktoolInstructionProgramDefinitions.ArmPieceAttackBack),
        new(ShaktoolInstructionProgramDefinitions.SawHandHeadBobFinalPiece,
            ShaktoolInstructionProgramDefinitions.SawHandAttackFinalPiece),
    ];

    /// <summary>Returns the list selected by the native five-bit center-direction bucket.</summary>
    internal static ushort ForOrientationBucket(ushort directionBucket)
    {
        if ((directionBucket & 0x001f) != 0 || directionBucket > 0x00e0)
        {
            throw new InvalidDataException(
                $"Shaktool orientation bucket ${directionBucket:X4} is invalid.");
        }

        return OrientationInstructions[directionBucket >> 5];
    }

    /// <summary>Returns the collision-recovery list for one physical segment.</summary>
    internal static ushort CollisionForSegment(int segmentIndex) =>
        ForSegment(segmentIndex).CollisionInstruction;

    /// <summary>Returns the dormant retail attack list for one physical segment.</summary>
    internal static ushort AttackForSegment(int segmentIndex) =>
        ForSegment(segmentIndex).AttackInstruction;

    private static ShaktoolSegmentInstructionDefinition ForSegment(int segmentIndex)
    {
        if ((uint)segmentIndex >= SegmentInstructions.Length)
        {
            throw new InvalidDataException(
                $"Shaktool instruction segment {segmentIndex} is outside seven records.");
        }

        return SegmentInstructions[segmentIndex];
    }
}

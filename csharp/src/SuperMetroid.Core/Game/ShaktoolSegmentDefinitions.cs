namespace SuperMetroid.Core.Game;

/// <summary>Fixed initialization metadata for one of Shaktool's seven linked segments.</summary>
internal readonly record struct ShaktoolSegmentDefinition(
    ushort PropertyMask,
    ushort OwnerNativeOffset,
    ushort InitialOrbitAngle,
    ushort InitialInstruction,
    ushort Layer,
    ShaktoolPreInstruction PreInstruction,
    ushort AngularVelocity);

/// <summary>Compiled mechanics and callback metadata for Shaktool's linked body records.</summary>
internal static class ShaktoolSegmentDefinitions
{
    /// <summary>
    /// The seven parallel records at <c>$AA:DE95-$DEF6</c>, transposed from the native
    /// property, owner-offset, angle, list, layer, callback, and angular-velocity tables.
    /// </summary>
    private static readonly ShaktoolSegmentDefinition[] Segments =
    [
        new(0x2800, 0x0000, 0x0000,
            ShaktoolInstructionProgramDefinitions.SawHandPrimaryPiece,
            0x0002, ShaktoolPreInstruction.IdleHead, 0x0000),
        new(0x2c00, 0x0040, 0xf800,
            ShaktoolInstructionProgramDefinitions.ArmPieceNormal,
            0x0004, ShaktoolPreInstruction.OrbitPreviousSegment, 0x0020),
        new(0x2c00, 0x0080, 0xe800,
            ShaktoolInstructionProgramDefinitions.ArmPieceNormal,
            0x0004, ShaktoolPreInstruction.OrbitPreviousSegment, 0x0060),
        new(0x2c00, 0x00c0, 0xd000,
            ShaktoolInstructionProgramDefinitions.HeadAimingDown,
            0x0002, ShaktoolPreInstruction.OrbitAndOrientCenter, 0x00c0),
        new(0x2c00, 0x0100, 0xb000,
            ShaktoolInstructionProgramDefinitions.ArmPieceNormal,
            0x0004, ShaktoolPreInstruction.OrbitPreviousSegment, 0x0140),
        new(0x2c00, 0x0140, 0x9800,
            ShaktoolInstructionProgramDefinitions.ArmPieceNormal,
            0x0004, ShaktoolPreInstruction.OrbitPreviousSegment, 0x01a0),
        new(0x2800, 0x0180, 0x8800,
            ShaktoolInstructionProgramDefinitions.SawHandPrimaryPiece,
            0x0002, ShaktoolPreInstruction.DriveTailAndReverseAtWalls, 0x01e0),
    ];

    /// <summary>Returns one of the seven physical linked-segment definitions.</summary>
    internal static ShaktoolSegmentDefinition ForIndex(int index)
    {
        if ((uint)index >= Segments.Length)
        {
            throw new InvalidDataException(
                $"Shaktool segment {index} is outside the seven native definition records.");
        }

        return Segments[index];
    }
}

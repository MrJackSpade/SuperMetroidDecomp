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
    /// <summary>$AA:DE95 ShaktoolPieceData_properties, seven property words.</summary>
    internal const int NativePropertiesAddress = 0xaade95;
    /// <summary>$AA:DEA3 ShaktoolPieceData_RAMOffset, seven owner-relative enemy offsets.</summary>
    internal const int NativeOwnerOffsetAddress = 0xaadea3;
    /// <summary>$AA:DEBF ShaktoolPieceData_initialInstListPointer, seven initial lists.</summary>
    internal const int NativeInstructionAddress = 0xaadebf;
    /// <summary>$AA:DECD ShaktoolPieceData_layerControl, seven drawing layers.</summary>
    internal const int NativeLayerAddress = 0xaadecd;
    /// <summary>$AA:DEDB ShaktoolPieceData_functionPointer, seven callbacks also used by reset.</summary>
    internal const int NativeCallbackAddress = 0xaadedb;

    /// <summary>$AA:DEB1 ShaktoolPieceData_initialNeighborAngle, seven authored initial joint angles.</summary>
    /// <remarks>#1165 review reopened by the performance-policy correction. The prior
    /// retain decision is withdrawn; lookup speed, repeated evaluation and caching
    /// cannot justify retaining this storage. Conversion/review remains outstanding.
    /// See lookup-performance-audit-1165.json for evidence and required follow-up.</remarks>
    private static ReadOnlySpan<ushort> InitialAngles => [0, 0xf800, 0xe800, 0xd000, 0xb000, 0x9800, 0x8800];
    /// <summary>$AA:DEE9 ShaktoolPieceData_initialCurlingNeighborAngleDelta, seven authored curl rates.</summary>
    /// <remarks>#1165 review reopened by the performance-policy correction. The prior
    /// retain decision is withdrawn; lookup speed, repeated evaluation and caching
    /// cannot justify retaining this storage. Conversion/review remains outstanding.
    /// See lookup-performance-audit-1165.json for evidence and required follow-up.</remarks>
    private static ReadOnlySpan<ushort> AngularVelocities => [0, 0x20, 0x60, 0xc0, 0x140, 0x1a0, 0x1e0];

    /// <summary>
    /// $AA:DE95 properties: end saws 0/6 have $2800, the five internal pieces $2C00.
    /// Independently checked against NTSC J/U v1.0 and pinned bank_AA.asm for #1165.
    /// Input has already been validated as segment 0..6; native initialization ORs it.
    /// </summary>
    private static ushort PropertiesForSegment(int index) => index is 0 or 6 ? (ushort)0x2800 : (ushort)0x2c00;

    /// <summary>
    /// $AA:DEA3 RAMOffset: 64*index for validated segment 0..6, the native enemy-record
    /// stride. Independently checked for #1165; caller subtracts with ushort wrap.
    /// </summary>
    private static ushort OwnerOffsetForSegment(int index) => (ushort)(64 * index);

    /// <summary>
    /// $AA:DEBF initialInstListPointer: both saws select their primary-piece list,
    /// center segment 3 the head-down list, and the four arms the normal arm list.
    /// Independently checked for #1165 against every original NTSC word and named
    /// disassembly target; indices are validated before this case selection.
    /// </summary>
    private static ushort InstructionForSegment(int index) => index switch
    {
        0 or 6 => ShaktoolInstructionProgramDefinitions.SawHandPrimaryPiece,
        3 => ShaktoolInstructionProgramDefinitions.HeadAimingDown,
        _ => ShaktoolInstructionProgramDefinitions.ArmPieceNormal,
    };

    /// <summary>
    /// $AA:DECD layerControl: saws and head (0,3,6) use layer 2; arms use layer 4.
    /// Independently checked for #1165 against all seven NTSC words. This is a
    /// part-role selection, not permission to wrap indices modulo three.
    /// </summary>
    private static ushort LayerForSegment(int index) => index is 0 or 3 or 6 ? (ushort)2 : (ushort)4;

    /// <summary>
    /// $AA:DEDB functionPointer: fixed anchor, head, final saw and ordinary arms
    /// have distinct roles. Independently checked for #1165 against NTSC words and
    /// pinned bank_AA.asm; both initialization and group reset use this selection.
    /// </summary>
    private static ShaktoolPreInstruction CallbackForSegment(int index) => index switch
    {
        0 => ShaktoolPreInstruction.IdleHead,
        3 => ShaktoolPreInstruction.OrbitAndOrientCenter,
        6 => ShaktoolPreInstruction.DriveTailAndReverseAtWalls,
        _ => ShaktoolPreInstruction.OrbitPreviousSegment,
    };

    /// <summary>Returns one of the seven physical linked-segment definitions.</summary>
    internal static ShaktoolSegmentDefinition ForIndex(int index)
    {
        if ((uint)index >= 7)
        {
            throw new InvalidDataException(
                $"Shaktool segment {index} is outside the seven native definition records.");
        }

        return new(PropertiesForSegment(index), OwnerOffsetForSegment(index),
            InitialAngles[index], InstructionForSegment(index), LayerForSegment(index),
            CallbackForSegment(index), AngularVelocities[index]);
    }
}

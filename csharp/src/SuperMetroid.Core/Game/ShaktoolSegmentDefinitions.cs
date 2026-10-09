namespace SuperMetroid.Core.Game;

/// <summary>Fixed initialization metadata for one of Shaktool's seven linked segments.</summary>
/// <param name="PropertyMask">Native properties word selected for the segment's role as an end saw or internal piece.</param>
/// <param name="OwnerNativeOffset">Byte offset from Shaktool's owner record to this segment's native enemy record.</param>
/// <param name="InitialOrbitAngle">Wrapped 16-bit turn angle assigned when the segment is initialized.</param>
/// <param name="InitialInstruction">Native instruction-list address that starts this segment's animation.</param>
/// <param name="Layer">Native layer-control value used to place the saw/head or arm piece.</param>
/// <param name="PreInstruction">Callback role that updates the segment before its instruction list runs.</param>
/// <param name="AngularVelocity">Initial angular increment used to keep the segment synchronized with the orbit target.</param>
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

    // Integrated symmetric joint increments 1,2,3,4,3,2. The first arm uses
    // triangular numbers; the other arm subtracts the remaining triangle from 16.
    // ForIndex validates the physical segment domain 0..6 before this calculation.
    /// <summary>Maps a validated segment index to its cumulative symmetric joint step along Shaktool's linked body.</summary>
    /// <param name="index">Physical segment position from 0 through 6.</param>
    /// <returns>The accumulated joint increment for that position, rising to the center and then mirroring back.</returns>
    private static int IntegratedJointStep(int index) => index <= 4
        ? index * (index + 1) / 2
        : 16 - (7 - index) * (8 - index) / 2;

    /// <summary>$AA:DEB1 initialNeighborAngle: -2048 times the integrated joint
    /// step, wrapped to a 16-bit turn. All seven words match the original NTSC data.</summary>
    private static ushort InitialAngleForSegment(int index) =>
        unchecked((ushort)(-2048 * IntegratedJointStep(index)));

    /// <summary>$AA:DEE9 initialCurlingNeighborAngleDelta: 32 times the integrated
    /// joint step. This is the same curvature at 1/64 of the initial angle magnitude;
    /// initialization and orbit-target synchronization use these exact seven rates.</summary>
    private static ushort AngularVelocityForSegment(int index) =>
        (ushort)(32 * IntegratedJointStep(index));

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
            InitialAngleForSegment(index), InstructionForSegment(index), LayerForSegment(index),
            CallbackForSegment(index), AngularVelocityForSegment(index));
    }
}

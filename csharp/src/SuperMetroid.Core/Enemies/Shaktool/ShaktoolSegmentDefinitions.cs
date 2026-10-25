namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Compiled mechanics and callback metadata for Shaktool's linked body records.</summary>
internal static class ShaktoolSegmentDefinitions
{

    /// <summary>Integrates symmetric joint increments 1,2,3,4,3,2 using triangular counts across the seven validated segments.</summary>
    private static int IntegratedJointStep(int index) => index <= 4
        ? index * (index + 1) / 2
        : 16 - (7 - index) * (8 - index) / 2;

    /// <summary>$AA:DEB1 initialNeighborAngle: -2048 times the integrated joint
    /// step, wrapped to a 16-bit turn. All seven words match the original NTSC data.</summary>
    private static RotationAngle InitialAngleForSegment(int index) =>
        RotationAngle.FromRaw(unchecked((ushort)(-2048 * IntegratedJointStep(index))));

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
    private static EnemyProperties PropertiesForSegment(int index) => index is 0 or 6
        ? EnemyProperties.ProcessInstructions | EnemyProperties.ProcessOffScreen
        : EnemyProperties.ProcessInstructions | EnemyProperties.ProcessOffScreen | EnemyProperties.IgnoreSamusCollision;

    /// <summary>
    /// $AA:DEA3 RAMOffset: how many slots segment 0..6 sits after the chain's first segment, its owner: its own index.
    /// </summary>
    private static int OwnerDistanceForSegment(int index) => index;

    /// <summary>
    /// $AA:DEBF initialInstListPointer: both saws select their primary-piece list,
    /// center segment 3 the head-down list, and the four arms the normal arm list.
    /// Independently checked for #1165 against every original NTSC word and named
    /// disassembly target; indices are validated before this case selection.
    /// </summary>
    private static ShaktoolProgram InstructionForSegment(int index) => index switch
    {
        0 or 6 => ShaktoolProgram.SawHandPrimaryPiece,
        3 => ShaktoolProgram.HeadAimingDown,
        _ => ShaktoolProgram.ArmPieceNormal,
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

        return new(PropertiesForSegment(index), OwnerDistanceForSegment(index),
            InitialAngleForSegment(index), InstructionForSegment(index), LayerForSegment(index),
            CallbackForSegment(index), AngularVelocityForSegment(index));
    }
}

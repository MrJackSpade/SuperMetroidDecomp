using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Compiled pose dispatch rules, independent of editable pose artwork and display offsets.</summary>
internal static class SamusPoseDispatchDefinitions
{
    /// <summary>$91:B629 PoseDefinitions_XDirection: native facing byte; values zero, one and two are retained, not guessed as left/right.</summary>
    private static ReadOnlySpan<byte> Facing =>
    [
        0, 8, 4, 8, 4, 8, 4, 8, 4, 8, 4, 8, 4, 8, 4, 8,
        4, 8, 4, 8, 4, 8, 4, 8, 4, 8, 4, 8, 4, 8, 8, 4,
        8, 1, 2, 4, 8, 4, 8, 8, 4, 8, 4, 8, 4, 8, 4, 4,
        8, 8, 4, 8, 4, 8, 4, 8, 4, 8, 4, 8, 4, 8, 4, 8,
        4, 4, 4, 4, 8, 8, 4, 8, 4, 8, 4, 8, 4, 8, 4, 8,
        4, 8, 4, 8, 4, 8, 4, 8, 4, 8, 4, 8, 4, 8, 4, 8,
        4, 8, 4, 8, 4, 8, 4, 8, 4, 8, 4, 8, 4, 8, 4, 8,
        4, 8, 4, 8, 4, 8, 4, 8, 4, 8, 4, 8, 4, 8, 4, 8,
        4, 8, 4, 8, 4, 8, 4, 4, 8, 8, 4, 4, 8, 4, 8, 4,
        8, 4, 8, 4, 8, 4, 8, 4, 8, 4, 8, 0, 4, 8, 4, 8,
        4, 8, 4, 8, 8, 4, 8, 4, 8, 4, 8, 4, 8, 4, 8, 4,
        8, 4, 8, 4, 8, 4, 8, 4, 8, 4, 4, 4, 4, 4, 4, 4,
        8, 4, 8, 4, 8, 4, 4, 8, 4, 8, 4, 8, 4, 8, 4, 8,
        4, 8, 4, 8, 4, 8, 4, 8, 4, 8, 4, 8, 4, 8, 4, 4,
        8, 4, 8, 4, 8, 4, 8, 4, 8, 4, 8, 4, 8, 8, 8, 8,
        8, 8, 4, 8, 4, 8, 4, 8, 4, 8, 4, 8, 4,
    ];

    /// <summary>$91:B62A PoseDefinitions_movementType: exclusive movement dispatcher for each authored pose.</summary>
    private static ReadOnlySpan<byte> Movement =>
    [
        0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1,
        1, 1, 1, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4,
        7, 7, 7, 7, 7, 14, 14, 5, 5, 6, 6, 6, 6, 6, 6, 23,
        23, 8, 8, 9, 9, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15,
        15, 4, 7, 14, 14, 1, 1, 0, 0, 16, 16, 2, 2, 2, 2, 25,
        25, 2, 2, 10, 10, 2, 2, 2, 2, 2, 2, 22, 22, 11, 11, 11,
        11, 22, 22, 13, 13, 13, 13, 6, 6, 2, 2, 2, 2, 6, 6, 6,
        6, 5, 5, 5, 5, 16, 16, 16, 16, 17, 17, 17, 17, 19, 19, 18,
        18, 3, 3, 20, 20, 5, 5, 24, 24, 21, 21, 14, 14, 14, 14, 23,
        23, 23, 23, 24, 24, 24, 24, 23, 23, 23, 23, 0, 14, 14, 23, 23,
        24, 24, 23, 23, 0, 0, 0, 0, 22, 22, 22, 22, 22, 22, 22, 22,
        22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 26, 26, 26, 26, 26, 14,
        14, 14, 14, 14, 14, 26, 26, 27, 27, 27, 27, 27, 27, 27, 27, 21,
        21, 21, 21, 27, 27, 0, 0, 10, 10, 5, 5, 15, 15, 15, 15, 26,
        0, 0, 0, 0, 0, 0, 0, 0, 27, 27, 27, 27, 26, 26, 26, 26,
        26, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15,
    ];

    /// <summary>$91:B62B PoseDefinitions_newPose: no-input fallback; $FF retains the current pose.</summary>
    private static ReadOnlySpan<byte> NoInputPose =>
    [
        255, 255, 255, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1,
        2, 1, 2, 255, 255, 81, 82, 255, 255, 255, 255, 255, 255, 255, 29, 65,
        255, 32, 32, 66, 32, 255, 255, 39, 40, 255, 255, 41, 42, 255, 255, 255,
        255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
        255, 255, 255, 255, 255, 1, 2, 255, 255, 2, 1, 255, 255, 255, 255, 78,
        77, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 93, 94, 95,
        96, 178, 179, 255, 255, 41, 42, 255, 255, 81, 82, 81, 82, 41, 42, 41,
        42, 39, 40, 39, 40, 6, 5, 8, 7, 255, 255, 121, 122, 255, 255, 255,
        255, 255, 255, 25, 26, 39, 40, 255, 255, 255, 255, 255, 255, 255, 255, 255,
        255, 255, 255, 255, 255, 255, 255, 40, 40, 40, 40, 255, 255, 255, 255, 255,
        255, 255, 40, 40, 255, 255, 255, 255, 1, 2, 7, 8, 103, 104, 45, 46,
        111, 112, 178, 179, 39, 40, 39, 40, 255, 255, 255, 186, 186, 186, 186, 255,
        255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 137,
        138, 137, 138, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
        255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 236, 236, 236,
        236, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
    ];

    internal static byte ReadFacing(ISnesAddressSpace bus, byte pose) => Read(bus, pose, Facing, 0);
    internal static byte ReadMovement(ISnesAddressSpace bus, byte pose) => Read(bus, pose, Movement, 1);
    internal static byte ReadNoInputPose(ISnesAddressSpace bus, byte pose) => Read(bus, pose, NoInputPose, 2);

    private static byte Read(ISnesAddressSpace bus, byte pose, ReadOnlySpan<byte> values, int field)
    {
        if (pose < values.Length) return values[pose];
        // The final three byte indexes reach adjacent executable data. Preserve their
        // explicit bus behavior instead of clamping them into an authored pose.
        return bus.ReadByte(SamusMovementRomData.Poses.Definitions +
            pose * SamusMovementRomData.Poses.DefinitionByteCount + field);
    }
}

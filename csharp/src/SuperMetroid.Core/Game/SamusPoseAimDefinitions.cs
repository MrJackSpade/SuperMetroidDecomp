using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Application-owned arm-cannon direction and pose firing restrictions, separate from artwork.</summary>
internal static class SamusPoseAimDefinitions
{
    /// <summary>$91:B62C PoseDefinitions_shotDirection, byte three of each authored pose record. Native $FA/$FB/$FC/$FF restrictions are preserved without masking them into directions.</summary>
    private static ReadOnlySpan<byte> Directions =>
    [
        255, 2, 7, 0, 9, 1, 8, 3, 6, 2, 7, 2, 7, 0, 9, 1,
        8, 3, 6, 2, 7, 0, 9, 4, 5, 255, 255, 255, 255, 255, 255, 255,
        255, 255, 255, 255, 255, 251, 251, 2, 7, 2, 7, 0, 9, 4, 5, 251,
        251, 255, 255, 255, 255, 2, 7, 255, 255, 255, 255, 2, 7, 255, 255, 255,
        255, 255, 255, 251, 251, 7, 2, 2, 7, 7, 2, 2, 7, 2, 7, 255,
        255, 2, 7, 255, 255, 0, 9, 1, 8, 3, 6, 255, 255, 255, 255, 255,
        255, 255, 255, 255, 255, 255, 255, 2, 7, 1, 8, 3, 6, 1, 8, 3,
        6, 1, 8, 3, 6, 8, 1, 6, 3, 255, 255, 255, 255, 255, 255, 255,
        255, 255, 255, 255, 255, 0, 9, 251, 251, 2, 7, 250, 250, 252, 252, 250,
        250, 252, 252, 250, 250, 252, 252, 250, 250, 252, 252, 255, 250, 250, 250, 250,
        250, 250, 250, 250, 2, 7, 2, 7, 2, 7, 3, 6, 2, 7, 4, 5,
        3, 6, 255, 255, 2, 7, 3, 6, 3, 6, 7, 8, 7, 6, 255, 251,
        251, 250, 250, 252, 252, 255, 251, 255, 255, 2, 7, 0, 9, 1, 8, 1,
        8, 3, 6, 255, 255, 2, 7, 2, 7, 2, 7, 255, 255, 255, 255, 255,
        0, 9, 1, 8, 3, 6, 2, 7, 255, 255, 255, 255, 2, 1, 2, 3,
        255, 0, 9, 1, 8, 3, 6, 0, 9, 1, 8, 3, 6,
    ];

    internal static byte Read(ISnesAddressSpace bus, byte pose)
    {
        if (pose < Directions.Length) return Directions[pose];
        // Non-authored indexes intentionally retain their adjacent cartridge-data
        // behavior. Do not clamp a restored/glitch pose into a valid aiming pose.
        return bus.ReadByte(SamusMovementRomData.Poses.Definitions +
            pose * SamusMovementRomData.Poses.DefinitionByteCount + 3);
    }
}

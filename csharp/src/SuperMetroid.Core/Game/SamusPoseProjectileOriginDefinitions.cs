using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Physical projectile-origin corrections, separate from editable Samus graphics offsets.</summary>
internal static class SamusPoseProjectileOriginDefinitions
{
    /// <summary>$91:B629 PoseDefinitions byte four for authored poses $00..$FC. Native beam/Grapple origin calculations zero-extend this byte.</summary>
    private static ReadOnlySpan<byte> YCorrections =>
    [
        8, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
        6, 6, 6, 8, 8, 8, 8, 6, 6, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 6, 6, 0, 0, 8, 8, 8, 8, 6, 6, 8,
        8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 6, 6, 0, 0, 0,
        0, 0, 0, 0, 0, 6, 6, 6, 6, 6, 6, 3, 3, 8, 8, 8,
        8, 8, 8, 6, 6, 3, 3, 3, 3, 3, 3, 16, 16, 16, 16, 16,
        16, 16, 16, 12, 12, 12, 12, 8, 8, 8, 8, 8, 8, 8, 8, 8,
        8, 0, 0, 0, 0, 6, 6, 6, 6, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 8, 8, 0, 0, 8, 8, 6, 6, 6, 6, 6, 6, 8,
        8, 8, 8, 8, 8, 8, 8, 0, 0, 0, 0, 8, 6, 6, 8, 8,
        8, 8, 0, 0, 3, 3, 3, 3, 6, 6, 6, 6, 8, 8, 6, 6,
        8, 8, 16, 16, 0, 0, 0, 0, 0, 0, 6, 6, 6, 6, 6, 6,
        6, 8, 8, 8, 8, 0, 6, 8, 8, 8, 8, 8, 8, 8, 8, 6,
        6, 6, 6, 6, 6, 6, 6, 6, 6, 0, 0, 0, 0, 6, 6, 0,
        3, 3, 3, 3, 3, 3, 3, 3, 0xfc, 0xfc, 0xfc, 0xfc, 6, 6, 6, 6,
        6, 8, 8, 8, 8, 8, 8, 3, 3, 3, 3, 3, 3,
    ];

    internal static byte ReadYOffset(ISnesAddressSpace bus, byte pose)
    {
        if (pose < YCorrections.Length)
            return YCorrections[pose];
        // $FD..$FF index the following executable bytes, not authored pose records.
        // Preserve the existing adjacent-data fallback until that policy is migrated.
        return bus.ReadByte(SamusMovementRomData.Poses.Definitions +
            pose * SamusMovementRomData.Poses.DefinitionByteCount + 4);
    }
}

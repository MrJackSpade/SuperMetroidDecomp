using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Physical pose extents, independent of editable artwork and its display offsets.</summary>
internal static class SamusPoseCollisionDefinitions
{
    /// <summary>$91:B629 PoseDefinitions byte six, for the 253 authored poses $00..$FC. Consumed by $90:EC22 Samus_SetRadius.</summary>
    private static ReadOnlySpan<byte> VerticalRadii =>
    [
        24, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21,
        21, 21, 21, 19, 19, 19, 19, 10, 10, 12, 12, 12, 12, 7, 7, 7,
        7, 7, 7, 7, 7, 21, 21, 16, 16, 19, 19, 19, 19, 10, 10, 19,
        19, 7, 7, 7, 7, 16, 16, 7, 7, 7, 7, 21, 21, 16, 16, 7,
        7, 7, 7, 16, 16, 21, 21, 21, 21, 21, 21, 19, 19, 19, 19, 19,
        19, 19, 19, 21, 21, 19, 19, 19, 19, 19, 19, 16, 16, 16, 16, 16,
        16, 16, 16, 12, 12, 12, 12, 19, 19, 19, 19, 19, 19, 19, 19, 19,
        19, 16, 16, 16, 16, 21, 21, 21, 21, 7, 7, 7, 7, 7, 7, 7,
        7, 12, 12, 19, 19, 16, 16, 19, 19, 21, 21, 21, 21, 21, 21, 19,
        19, 19, 19, 19, 19, 19, 19, 16, 16, 16, 16, 24, 21, 21, 19, 19,
        19, 19, 16, 16, 21, 21, 21, 21, 21, 21, 21, 21, 19, 19, 10, 10,
        19, 19, 17, 17, 16, 16, 16, 16, 16, 16, 21, 21, 21, 21, 21, 21,
        21, 21, 21, 21, 21, 7, 21, 19, 19, 19, 19, 19, 19, 19, 19, 21,
        21, 21, 21, 21, 21, 21, 21, 21, 21, 16, 16, 7, 7, 21, 21, 7,
        21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21,
        21, 16, 16, 16, 16, 16, 16, 21, 21, 21, 21, 21, 21,
    ];

    internal static byte ReadVerticalRadius(ISnesAddressSpace bus, byte pose)
    {
        if (pose < VerticalRadii.Length)
            return VerticalRadii[pose];
        // Trailing pose indexes address adjacent code, not authored metadata. Keep
        // those reads explicit until the wider out-of-table policy is migrated.
        return bus.ReadByte(SamusMovementRomData.Poses.Definitions +
            pose * SamusMovementRomData.Poses.DefinitionByteCount + 6);
    }
}

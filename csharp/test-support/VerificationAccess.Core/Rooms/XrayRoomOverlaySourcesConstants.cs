using SuperMetroid.Core.Rooms.Xray;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="XrayRoomOverlaySources"/> that only verification reads.</summary>
internal static class XrayRoomOverlaySourcesConstants
{
    /// <summary>Every overlay pointer, in ascending order.</summary>
    public static IEnumerable<ushort> Pointers => XrayRoomOverlaySourceDefinitions.ByPointer.Keys.Order();
    /// <summary>The overlay at <paramref name="pointer"/>.</summary>
    public static XrayRoomOverlayDefinition Overlay(int pointer) =>
        (uint)pointer <= ushort.MaxValue &&
        XrayRoomOverlaySourceDefinitions.ByPointer.TryGetValue((ushort)pointer, out XrayRoomOverlayDefinition? found)
            ? found
            : throw new ArgumentOutOfRangeException(nameof(pointer), pointer, "Pointer is not a retail room X-ray overlay.");
}

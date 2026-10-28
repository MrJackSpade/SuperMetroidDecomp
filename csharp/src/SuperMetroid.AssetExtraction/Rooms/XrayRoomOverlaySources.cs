using SuperMetroid.Core.Rooms.Xray;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The import pipeline's map between the bank-$8F special X-ray pointers room states and reveals.json store and the
/// overlays they name; the simulation knows only the named overlays.
/// </summary>
public static class XrayRoomOverlaySources
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

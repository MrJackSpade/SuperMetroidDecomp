using SuperMetroid.Core.Rooms.Xray;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The import pipeline's map between the bank-$8F special X-ray pointers room states store and the overlays they name;
/// the simulation and the installed reveals know only the named overlays.
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

    /// <summary>The bank-$8F pointer of <paramref name="overlay"/>'s list.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="overlay"/> is not a retail overlay.</exception>
    public static ushort PointerOf(XrayRoomOverlayDefinition overlay)
    {
        Core.Ensure.NotNull(overlay);
        foreach (KeyValuePair<ushort, XrayRoomOverlayDefinition> source in XrayRoomOverlaySourceDefinitions.ByPointer)
        {
            if (ReferenceEquals(source.Value, overlay))
                return source.Key;
        }
        throw new ArgumentOutOfRangeException(nameof(overlay), overlay.Name, "Not a retail room X-ray overlay.");
    }
}

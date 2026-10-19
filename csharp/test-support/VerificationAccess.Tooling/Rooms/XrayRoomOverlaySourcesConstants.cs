using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.Xray;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="XrayRoomOverlaySources"/> that only verification reads.</summary>
internal static class XrayRoomOverlaySourcesConstants
{
    /// <summary>The pointer of <paramref name="overlay"/>.</summary>
    public static ushort PointerOf(XrayRoomOverlayDefinition overlay)
    {
        Ensure.NotNull(overlay);
        return XrayRoomOverlaySourceDefinitions.ByPointer.Single(pair => Object.ReferenceEquals(pair.Value, overlay)).Key;
    }
}

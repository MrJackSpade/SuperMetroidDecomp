using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.Core.Rooms.Xray;

/// <summary>Verification's view of the import pipeline's special X-ray pointers (<see cref="XrayRoomOverlaySources"/>).</summary>
internal static class XrayRoomOverlayDefinitionsAccess
{
    /// <summary>The special X-ray pointer of a room state.</summary>
    extension(CartridgeRoomState self)
    {
        /// <summary>The bank-$8F pointer of this state's X-ray overlay, or zero for none.</summary>
        internal ushort XrayPointer => self.XrayOverlay is { } overlay ? XrayRoomOverlaySourcesConstants.PointerOf(overlay) : (ushort)0;
    }
}

using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Abilities.Artwork;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Core.Rooms.Xray;

/// <summary>Verification access to <see cref="InstalledXrayOverlays"/> members production does not use.</summary>
internal static class InstalledXrayOverlaysAccess
{
    /// <summary>Constructs partial X-Ray room overlay catalogs for focused rendering fixtures.</summary>
    extension(InstalledXrayOverlays)
    {
        /// <summary>Explicitly partial room overlays for focused fixtures; never an installed catalog.</summary>
        internal static InstalledXrayOverlays FromOverlaysForVerification(IReadOnlyDictionary<InWorldCollectibleKind, RoomBlockAppearance> tankBlocks,
            IEnumerable<XrayRoomOverlayBlocks> rooms) =>
            global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Samus.Abilities.Artwork.InstalledXrayOverlays>([typeof(global::System.Collections.Generic.IReadOnlyDictionary<global::SuperMetroid.Core.Rooms.RoomObjects.InWorldCollectibleKind, global::SuperMetroid.Core.Rooms.LevelData.RoomBlockAppearance>), typeof(global::System.Collections.Generic.IEnumerable<global::SuperMetroid.Core.Samus.Abilities.XrayRoomOverlayBlocks>), typeof(global::System.Collections.Generic.IEnumerable<global::SuperMetroid.Core.Rooms.Xray.XrayRoomOverlayDefinition>)], new object?[] { (global::System.Collections.Generic.IReadOnlyDictionary<global::SuperMetroid.Core.Rooms.RoomObjects.InWorldCollectibleKind, global::SuperMetroid.Core.Rooms.LevelData.RoomBlockAppearance>)(tankBlocks), (global::System.Collections.Generic.IEnumerable<global::SuperMetroid.Core.Samus.Abilities.XrayRoomOverlayBlocks>)(rooms), (global::System.Collections.Generic.IEnumerable<global::SuperMetroid.Core.Rooms.Xray.XrayRoomOverlayDefinition>)([]) });
    }
}

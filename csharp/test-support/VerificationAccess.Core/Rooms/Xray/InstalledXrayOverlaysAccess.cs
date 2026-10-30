using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Abilities.Artwork;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Core.Rooms.Xray;

/// <summary>Verification access to <see cref="InstalledXrayOverlays"/> members production does not use.</summary>
internal static class InstalledXrayOverlaysAccess
{
    /// <summary>Constructs partial X-Ray room overlay catalogs for focused rendering fixtures.</summary>
    extension(InstalledXrayOverlays)
    {
        /// <summary>Explicitly partial room overlays for focused fixtures; never an installed catalog.</summary>
        internal static InstalledXrayOverlays FromOverlaysForVerification(IEnumerable<RoomBlockAppearance> itemMetatiles,
            IEnumerable<XrayRoomOverlayTiles> rooms) =>
            new InstalledXrayOverlays(itemMetatiles, rooms, requiredOverlays: []);
    }
}

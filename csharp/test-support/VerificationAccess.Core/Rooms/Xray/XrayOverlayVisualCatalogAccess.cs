using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Core.Rooms.Xray;

/// <summary>Verification access to <see cref="XrayOverlayVisualCatalog"/> members production does not use.</summary>
internal static class XrayOverlayVisualCatalogAccess
{
    /// <summary>Constructs partial X-Ray room overlay catalogs for focused rendering fixtures.</summary>
    extension(XrayOverlayVisualCatalog)
    {
        /// <summary>Explicitly partial room overlays for focused fixtures; never an installed catalog.</summary>
        internal static XrayOverlayVisualCatalog FromOverlaysForVerification(IEnumerable<ushort> itemMetatiles,
            IEnumerable<(ushort Pointer, IReadOnlyList<XrayRoomOverlayVisual> Tiles)> rooms) =>
            new XrayOverlayVisualCatalog(itemMetatiles, rooms, false);
    }
}

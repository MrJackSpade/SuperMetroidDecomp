using SuperMetroid.ResourceAudit.Reports;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Rooms;

/// <summary>Complete terrain-reveal and installed overlay stores.</summary>
internal static class RoomRevealClosedContractDefinitions
{
    /// <summary>Reviewed source for X-ray visual ownership shared by reveal and overlay contracts.</summary>
    private static readonly ReviewedSource XrayCatalog = new(
        "csharp/src/SuperMetroid.Core/Samus/Abilities/Artwork/InstalledXrayReveals.cs");
    /// <summary>Names the two closed audits for reveal commands and room overlays.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Samus.Abilities.Artwork.InstalledXrayReveals", "xray-command-and-visual-share-one-identity", ["Apply"],
            [XrayCatalog,
             new("csharp/src/SuperMetroid.Core/Samus/Abilities/XrayRevealTable.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Abilities/XrayRevealDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Abilities/XrayReveals.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/LevelData/RoomBlockAppearance.cs")]),
        new("SuperMetroid.Core.Samus.Abilities.Artwork.InstalledXrayOverlays", "xray-complete-items-and-required-room-overlays", ["ItemMetatile", "RoomTiles"],
            [XrayCatalog, new("csharp/src/SuperMetroid.Core/Samus/Abilities/Artwork/InstalledXrayOverlays.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Abilities/XrayRoomOverlayVisual.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Xray/XrayRoomOverlayDefinition.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Xray/XrayRoomOverlayDefinitions.cs")]),
    ];
}

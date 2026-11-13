using SuperMetroid.ResourceAudit.Reports;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Rooms;

/// <summary>
/// Complete pre-coloured room pictures (#1513, #1514, #1536): the installed room artwork every retail room state selects
/// by tileset, background, sky and effect, the turning floor's animation frames, and the message box forms.
/// </summary>
internal static class RoomPictureClosedContractDefinitions
{
    /// <summary>The retail room states the import walks and the tileset each selects.</summary>
    private static readonly ReviewedSource[] RetailStates =
    [
        new("csharp/src/SuperMetroid.Core/Rooms/RoomDefinitions.cs"),
        new("csharp/src/SuperMetroid.AssetExtraction/Rooms/RetailRooms.cs"),
        new("csharp/src/SuperMetroid.AssetExtraction/World/RoomStates/RoomStateLists.cs"),
        new("csharp/src/SuperMetroid.Core/Rooms/LevelData/RoomTilesets.cs"),
        new("csharp/src/SuperMetroid.Core/World/RoomStates/SelectedRoom.cs"),
    ];

    /// <summary>The loader that reads the room artwork and requires what the retail room states select.</summary>
    private static readonly ReviewedSource Loader = new("csharp/src/SuperMetroid.AssetExtraction/Installation/RoomBlockSheetFiles.cs");

    /// <summary>The retail room states' selections the loader requires, and the composition that installs them.</summary>
    private static readonly ReviewedSource Uploads = new("csharp/src/SuperMetroid.AssetExtraction/Rooms/RoomBlockUploads.cs");

    /// <summary>Reviewed room picture providers, each with the sources its complete key domain rests on.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.Artwork.InstalledRoomArtwork", "room-artwork-complete-retail-selections",
            ["Get", "FloorOf", "MessageBox", "FxOverlay", "Background", "Sky"],
            [new("csharp/src/SuperMetroid.Core/Rooms/Artwork/InstalledRoomArtwork.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Artwork/RoomBlockSheet.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Artwork/RoomBackgroundImage.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Backgrounds/LibraryBackground.cs"),
             // Only a background the player sees as its own picture is installed and looked up (#1649).
             new("csharp/src/SuperMetroid.Core/Rooms/Backgrounds/LibraryBackgroundView.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Artwork/RoomLibraryBackgroundPicture.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Rooms/PicturedLibraryBackgrounds.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/ScrollingSkyState.cs"),
             new("csharp/src/SuperMetroid.Core/World/RoomStates/LoadedRoomAssets.cs"),
             .. RetailStates, Uploads, Loader,
             new("csharp/src/SuperMetroid.AssetExtraction/Rooms/RoomBlockSheetComposer.cs")]),
        new("SuperMetroid.Core.Rooms.Artwork.RoomFloorSheet", "room-floor-complete-animation-frames", ["Picture"],
            [new("csharp/src/SuperMetroid.Core/Rooms/Artwork/RoomFloorSheet.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Backgrounds/CeresFloorAnimationFrame.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Rooms/RoomFloorSheetComposer.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Areas/Ceres/CeresFloorDrawings.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Areas/Ceres/CeresFloorDrawingDefinitions.cs")]),
        new("SuperMetroid.Core.Rooms.Artwork.MessageBoxSheet", "message-box-complete-shown-forms", ["Picture"],
            [new("csharp/src/SuperMetroid.Core/Rooms/Artwork/MessageBoxSheet.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/ShownMessage.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/GameplayMessageId.cs"),
             new("csharp/src/SuperMetroid.Core/Hud/GameplayMessageBoxState.cs"),
             new("csharp/src/SuperMetroid.Core/Hud/GameplayMessageButtons.cs"),
             new("csharp/src/SuperMetroid.Core/Hud/GameplayMessageBoxDefinitions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Hud/GameplayMessageArtwork.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Rooms/MessageBoxSheetComposer.cs"),
             Loader]),
        new("SuperMetroid.Core.Fx.Artwork.FxOverlayPagePictures", "fx-overlay-complete-effect-pages", ["Page"],
            [new("csharp/src/SuperMetroid.Core/Fx/Artwork/FxOverlayPagePictures.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/RoomFxOverlayState.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/FxOverlayPlane.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/FxOverlayPictureResolution.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Artwork/WrittenPictureSheet.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Rooms/WrittenPictureSheetComposer.cs"),
             .. RetailStates, Uploads, Loader]),
    ];
}


namespace SuperMetroid.ResourceAudit.ClosedContracts.Bosses;

/// <summary>Complete authored effect/upload domains, not display placement or effect mechanics.</summary>
internal static class BackgroundTransferClosedContractDefinitions
{
    /// <summary>
    /// Closed contracts for room FX transfers, ending fragments, and gunship takeoff artwork. Boss bodies draw installed
    /// body frames (#1466); nothing reads their BG2 frame catalogs' writes at run time.
    /// </summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Fx.Artwork.RoomFxLayer3Pages", "room-fx-complete-six-pages", ["Resolve"],
            [new("csharp/src/SuperMetroid.Core/Fx/Artwork/RoomFxLayer3Pages.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/Artwork/RoomFxLayer3TilemapFormat.cs")]),
        new("SuperMetroid.Core.Fx.Artwork.RoomFxBlendInks", "room-fx-complete-eight-blends-and-zero-clear", ["Apply"],
            [new("csharp/src/SuperMetroid.Core/Fx/Artwork/RoomFxBlendInks.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/Artwork/RoomFxPaletteBlendDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/Artwork/RoomFxPaletteBlendRules.cs")]),
        new("SuperMetroid.AssetExtraction.Frontend.Ending.Artwork.EndingObjectSourceArtwork", "ending-complete-four-fragments", ["Fragment"],
            [new("csharp/src/SuperMetroid.AssetExtraction/Frontend/Ending/Artwork/EndingObjectSourceArtwork.cs")]),
        new("SuperMetroid.AssetExtraction.Enemies.GunshipLiftoffArtworkCatalog", "gunship-complete-five-takeoff-transfers", ["Resolve"],
            [new("csharp/src/SuperMetroid.AssetExtraction/Enemies/GunshipLiftoffArtworkCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Artwork/GunshipLiftoffTransferDefinitions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Enemies/GunshipLiftoffArtwork.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Enemies/GunshipLiftoffCharacterDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Artwork/GunshipLiftoffTransferDefinition.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/IInstalledArtworkContent.cs")]),
    ];
}

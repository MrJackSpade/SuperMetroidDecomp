namespace SuperMetroid.ResourceAudit;

/// <summary>Complete ordinary sheet/palette admission and owned enemy DMA sources.</summary>
internal static class EnemyArtworkClosedContractDefinitions
{
    /// <summary>Stores all.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.EnemyTileArtworkCatalog", "installed-enemy-sheets-palettes-and-dma", ["LoadTo", "LoadPaletteTo", "TryResolve"],
            // #1172: reviewed schema 68 -> 69 bump; provider and DMA routing are unchanged.
            [new("csharp/src/SuperMetroid.Core/Assets/EnemyTileArtworkCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/EnemyTileSourceDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Game/RoomEnemyGraphicsSetDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Game/RoomEnemyDefinitionCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Game/RoomEnemyData.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/RoomCharacterAtlas.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/EnemyPaletteSheet.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/CeresEscapeTileArtwork.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/CeresEscapeTileRomData.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/CeresEscapeOverlayTilemapCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/TorizoInstructionVramArtwork.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/TorizoInstructionTileRomData.cs"),
             new("csharp/src/SuperMetroid.Core/Game/CeresEscapeVramTransferDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Game/CeresRidleyPaletteRomData.cs")]),
    ];
}

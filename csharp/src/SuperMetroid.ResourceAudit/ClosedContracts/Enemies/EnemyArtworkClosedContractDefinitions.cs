namespace SuperMetroid.ResourceAudit.ClosedContracts.Enemies;

/// <summary>Complete ordinary sheet/palette admission and owned enemy DMA sources.</summary>
internal static class EnemyArtworkClosedContractDefinitions
{
    /// <summary>Constrains ordinary enemy sheets, palettes, and DMA routing to the installed catalog and its reviewed source owners.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Enemies.Artwork.EnemyTileArtworkCatalog", "installed-enemy-sheets-palettes-and-dma", ["LoadTo", "LoadPaletteTo", "TryResolve"],
            [new("csharp/src/SuperMetroid.Core/Enemies/Artwork/EnemyTileArtworkCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Artwork/EnemyTileSourceDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/RoomEnemyGraphicsSetDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/RoomEnemyDefinitionCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/RoomEnemyDefinition.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/RoomEnemyPopulationRecord.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/RoomEnemyGraphicsSetEntry.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/RoomEnemySpawnSnapshot.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/RoomEnemySlot.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Gunship/GunshipFrameEvent.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Gunship/GunshipLoadScenario.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Artwork/RoomCharacterAtlas.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Artwork/EnemyPaletteSheet.cs"),
             new("csharp/src/SuperMetroid.Core/Areas/Ceres/Artwork/CeresEscapeTileArtwork.cs"),
             new("csharp/src/SuperMetroid.Core/Areas/Ceres/Artwork/CeresEscapeTileRomData.cs"),
             new("csharp/src/SuperMetroid.Core/Areas/Ceres/Artwork/CeresEscapeOverlayTilemapCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Torizo/Artwork/TorizoInstructionVramArtwork.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Torizo/Artwork/TorizoInstructionTileRomData.cs"),
             new("csharp/src/SuperMetroid.Core/Areas/Ceres/CeresRidleyPaletteRomData.cs")]),
    ];
}

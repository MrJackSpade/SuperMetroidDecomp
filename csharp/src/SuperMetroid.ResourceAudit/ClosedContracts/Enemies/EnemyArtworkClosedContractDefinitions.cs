namespace SuperMetroid.ResourceAudit.ClosedContracts.Enemies;

/// <summary>Complete ordinary sheet/palette admission.</summary>
internal static class EnemyArtworkClosedContractDefinitions
{
    /// <summary>Constrains ordinary enemy sheets and palettes to the installed catalog and its reviewed source owners.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Enemies.Artwork.EnemyTileArtworkCatalog", "installed-enemy-sheets-and-palettes", ["LoadTo", "LoadPaletteTo"],
            [new("csharp/src/SuperMetroid.Core/Enemies/Artwork/EnemyTileArtworkCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Artwork/EnemyTileSourceDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/RoomEnemyGraphicsSetDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/RoomEnemyGraphicsSets.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/RoomEnemyDefinitionCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/RoomEnemyDefinitions.cs"),
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
             new("csharp/src/SuperMetroid.Core/Areas/Ceres/Artwork/CeresEscapeTileArtworkDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Areas/Ceres/Artwork/CeresEscapeOverlayTilemapCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Torizo/Artwork/TorizoSwapArtwork.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Torizo/Artwork/TorizoSwapArtworkDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Ridley/CeresRidleyPaletteDefinitions.cs")]),
    ];
}

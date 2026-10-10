namespace SuperMetroid.ResourceAudit;

/// <summary>Complete ordinary sheet/palette admission and owned enemy DMA sources.</summary>
internal static class EnemyArtworkClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.EnemyTileArtworkCatalog", "installed-enemy-sheets-palettes-and-dma", ["LoadTo", "LoadPaletteTo", "TryResolve"],
            // #1172: reviewed schema 68 -> 69 bump; provider and DMA routing are unchanged.
            [new("csharp/src/SuperMetroid.Core/Assets/EnemyTileArtworkCatalog.cs", "31C7E308A22A9FF0429D9F8407732FB64B2A6413ECEF6BE8E419AD105547D2DE"),
             new("csharp/src/SuperMetroid.Core/Assets/EnemyTileArtworkCatalog.Construction.cs", "E2F48346CD397115897E688BFC218937052D98290E8CF9F4386FE7740056E766"),
             new("csharp/src/SuperMetroid.Core/Assets/EnemyTileSourceDefinitions.cs", "C8900892C369F7D578BF1C091099E0A2ECE7F504CBC9F8F7E7903EB23B4A25A4"),
             new("csharp/src/SuperMetroid.Core/Game/RoomEnemyGraphicsSetDefinitions.cs", "22576CBA629B51D4CBE21F8B1BF2A09655EEB1ECD88880DAE68DC00AD06AE795"),
             new("csharp/src/SuperMetroid.Core/Game/RoomEnemyGraphicsSetDefinitions.Segment0.cs", "430DB84B82DDA8FF98A707DBACE34192F036904AA34B1FA3C8F237ACD8A3585D"),
             new("csharp/src/SuperMetroid.Core/Game/RoomEnemyGraphicsSetDefinitions.Segment1.cs", "3B782DB5D320EBF57291BD63C86815975DB5F6BC88280AE3FF35D85DF3767C13"),
             new("csharp/src/SuperMetroid.Core/Game/RoomEnemyDefinitionCatalog.cs", "AD7253859A71EBDEA489B3E80D741A798A330CD838E27A185FF8E3ACA67F82B0"),
             new("csharp/src/SuperMetroid.Core/Game/RoomEnemyData.cs", "58AAB23BFD45068CB5ED1B46ACCB8B4171221355A62C524EE9240A982F533FAC"),
             new("csharp/src/SuperMetroid.Core/Assets/RoomCharacterAtlas.cs", "2C71C8210FE457FA1D1FF6FFF05EE71BC9BBB661E562CA156F44A6FDEF7B985C"),
             new("csharp/src/SuperMetroid.Core/Assets/EnemyPaletteSheet.cs", "4CC11BE1A89390882C28C76E0EAB42A97F2C45E9B4552A41359EE7DAE289F286"),
             new("csharp/src/SuperMetroid.Core/Assets/CeresEscapeTileArtwork.cs", "C97DFD5E72DAB11249F88DBCCEF5996F602A6560680E64FDB3667C51DFE1B35F"),
             new("csharp/src/SuperMetroid.Core/Assets/CeresEscapeTileRomData.cs", "E7051AB9EF862988C95DEC6DECC4C2A1C38D696942F445E1259BC58E33B89A93"),
             new("csharp/src/SuperMetroid.Core/Assets/CeresEscapeOverlayTilemapCatalog.cs", "00A6C0222B8FCFADCE360EA3AAA08793612FD933C700346AF5F813796709AF92"),
             new("csharp/src/SuperMetroid.Core/Assets/TorizoInstructionVramArtwork.cs", "DCCBBD1AD05A05241997D84D3BD6192C20803411399A91C76C535339ECA1371A"),
             new("csharp/src/SuperMetroid.Core/Assets/TorizoInstructionTileRomData.cs", "CBFA2E7FD479107E91B024AB67CB061356B484E6496401738115592B1C9FB708"),
             new("csharp/src/SuperMetroid.Core/Game/CeresEscapeVramTransferDefinitions.cs", "9E546C3E2FC69B26020FEDF3EAAD3186F91A3C69EE84384F419E1AE0A8D0BBC9"),
             new("csharp/src/SuperMetroid.Core/Game/CeresRidleyPaletteRomData.cs", "E3EFB5FA4E69B08685BA5D077C28F6A8818EF2EA9514284BEB3419E0F16EB406")]),
    ];
}

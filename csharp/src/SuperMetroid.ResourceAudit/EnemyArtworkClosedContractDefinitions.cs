namespace SuperMetroid.ResourceAudit;

/// <summary>Complete ordinary sheet/palette admission and owned enemy DMA sources.</summary>
internal static class EnemyArtworkClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.EnemyTileArtworkCatalog", "installed-enemy-sheets-palettes-and-dma", ["LoadTo", "LoadPaletteTo", "TryResolve"],
            // #1172: reviewed schema 68 -> 69 bump; provider and DMA routing are unchanged.
            [new("csharp/src/SuperMetroid.Core/Assets/EnemyTileArtworkCatalog.cs", "F51F1BBB2643F58E618B0BD3310899DFCC00896F4CA73DFE75B33BE95181AD4A"),
             new("csharp/src/SuperMetroid.Core/Assets/EnemyTileArtworkCatalog.Construction.cs", "01CDA4CA13BE7A3CA179F92B2A27FB4250BDFF5F632D6428F6C25C041C927F83"),
             new("csharp/src/SuperMetroid.Core/Assets/EnemyTileSourceDefinitions.cs", "4169C1F970535F5D03B6BBCCEAB32C50C41FB9CDB660F12672E7CB1AE4CA4DBE"),
             new("csharp/src/SuperMetroid.Core/Game/RoomEnemyGraphicsSetDefinitions.cs", "CEF2D6749BB7759E33E6E6C97954FD0BDD89E42686EF681160A7D114CD01D536"),
             new("csharp/src/SuperMetroid.Core/Game/RoomEnemyGraphicsSetDefinitions.Segment0.cs", "F89CA79521EC322761F483B562D940CDA3419A15F2494C65809DD25007770E0F"),
             new("csharp/src/SuperMetroid.Core/Game/RoomEnemyGraphicsSetDefinitions.Segment1.cs", "E055B7F6F3000F15895AD916733BBEEF347D0C9E42D9EC7B17514EA7FD132C7C"),
             new("csharp/src/SuperMetroid.Core/Game/RoomEnemyDefinitionCatalog.cs", "E369DF9E8D20E4480A1DE0629C9CC3AD399F66CDD703939732E2FF5DA82D7F1C"),
             new("csharp/src/SuperMetroid.Core/Game/RoomEnemyData.cs", "81ED72344C27206C7705C0CAE87C30D699B6F4784B4F1A85861664DBDFB2A1C9"),
             new("csharp/src/SuperMetroid.Core/Assets/RoomCharacterAtlas.cs", "0C2CD85F446A356CF2A0E226E7F9D45C64F23C118A58097058E2B33152F97512"),
             new("csharp/src/SuperMetroid.Core/Assets/EnemyPaletteSheet.cs", "A315AC77B3BC2E0E17D83FFFB7ED84F8F9353A129BC06714BF4E8BB8998B9CB5"),
             new("csharp/src/SuperMetroid.Core/Assets/CeresEscapeTileArtwork.cs", "EEDA9FFC087B4CFDB12BF640D8AEE275DCD02E18309A69B6ADB67693B96EEADE"),
             new("csharp/src/SuperMetroid.Core/Assets/CeresEscapeTileRomData.cs", "DF8836E127366B910D5649CE661AFA7F33554113B28D4A126745013489B664CA"),
             new("csharp/src/SuperMetroid.Core/Assets/CeresEscapeOverlayTilemapCatalog.cs", "5F76BC5D3BACA269E5BC4AAD2F21848CDF4D9E8380D3DB4199A8D1795E79C052"),
             new("csharp/src/SuperMetroid.Core/Assets/TorizoInstructionVramArtwork.cs", "E713CC6BBF734DD700CBE4397EF8D81ABEDF19299DB65A359453DFC1AEB43FBC"),
             new("csharp/src/SuperMetroid.Core/Assets/TorizoInstructionTileRomData.cs", "10F23555CAF58BB4A559874B08847132683CB1EB1487B3BF51C6B381F93286F1"),
             new("csharp/src/SuperMetroid.Core/Game/CeresEscapeVramTransferDefinitions.cs", "22089C1DDF1302D2F523E73799DBA8E522CD1E932B9AA5E19B81C85A47837AE3"),
             new("csharp/src/SuperMetroid.Core/Game/CeresRidleyPaletteRomData.cs", "20CB1E292CC94F8BB30D9F237F148789E3E165F58BD116D5259990370C9E17CE")],
            "The installed factory requires all 122 compiled graphics-set identities with nonnull palettes and exact native tile lengths; pinned IDs reject substitutions. Native DMA aliases are derived or validated, and complete Ceres and Torizo transfer providers are required. Dictionaries are copied. LoadTo/LoadPaletteTo cover valid definition IDs; TryResolve covers native sheet sources, bounded Ceres/Torizo slices and exact warning tilemaps, returning false for unowned sources. Invalid known identities/lengths remain findings. External Core use of the internal partial fixture factory or new metadata declarations revokes this rule. Other optional boss attachments, caller selection, destinations, timing and rendered pixels are not certified."),
    ];
}

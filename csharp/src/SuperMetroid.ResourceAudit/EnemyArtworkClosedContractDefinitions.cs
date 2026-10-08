namespace SuperMetroid.ResourceAudit;

/// <summary>Complete ordinary sheet/palette admission and owned enemy DMA sources.</summary>
internal static class EnemyArtworkClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.EnemyTileArtworkCatalog", "installed-enemy-sheets-palettes-and-dma", ["LoadTo", "LoadPaletteTo", "TryResolve"],
            // #1172: reviewed schema 68 -> 69 bump; provider and DMA routing are unchanged.
            [new("csharp/src/SuperMetroid.Core/Assets/EnemyTileArtworkCatalog.cs", "85EEF67BCBEC8E580DE48B1792F6E56686FC2909A3FB5544E014E7FE18B75889"),
             new("csharp/src/SuperMetroid.Core/Assets/EnemyTileArtworkCatalog.Construction.cs", "AE0B9D052F83A87D0C9B002BE34419892CE20EB21E22F80B7BD241507F965FF7"),
             new("csharp/src/SuperMetroid.Core/Assets/EnemyTileSourceDefinitions.cs", "4169C1F970535F5D03B6BBCCEAB32C50C41FB9CDB660F12672E7CB1AE4CA4DBE"),
             new("csharp/src/SuperMetroid.Core/Game/RoomEnemyGraphicsSetDefinitions.cs", "CEF2D6749BB7759E33E6E6C97954FD0BDD89E42686EF681160A7D114CD01D536"),
             new("csharp/src/SuperMetroid.Core/Game/RoomEnemyGraphicsSetDefinitions.Segment0.cs", "F89CA79521EC322761F483B562D940CDA3419A15F2494C65809DD25007770E0F"),
             new("csharp/src/SuperMetroid.Core/Game/RoomEnemyGraphicsSetDefinitions.Segment1.cs", "E055B7F6F3000F15895AD916733BBEEF347D0C9E42D9EC7B17514EA7FD132C7C"),
             new("csharp/src/SuperMetroid.Core/Game/RoomEnemyDefinitionCatalog.cs", "D35143910B969E2AB079907C0E89130EA1323AA3B91404C475635AA756532D14"),
             new("csharp/src/SuperMetroid.Core/Game/RoomEnemyData.cs", "9DB6BE2C152B6875DB3E324588D479EAEBC5797D111B2625FDB822CB4DE05E5F"),
             new("csharp/src/SuperMetroid.Core/Assets/RoomCharacterAtlas.cs", "0C2CD85F446A356CF2A0E226E7F9D45C64F23C118A58097058E2B33152F97512"),
             new("csharp/src/SuperMetroid.Core/Assets/EnemyPaletteSheet.cs", "A315AC77B3BC2E0E17D83FFFB7ED84F8F9353A129BC06714BF4E8BB8998B9CB5"),
             new("csharp/src/SuperMetroid.Core/Assets/CeresEscapeTileArtwork.cs", "EEDA9FFC087B4CFDB12BF640D8AEE275DCD02E18309A69B6ADB67693B96EEADE"),
             new("csharp/src/SuperMetroid.Core/Assets/CeresEscapeTileRomData.cs", "DF8836E127366B910D5649CE661AFA7F33554113B28D4A126745013489B664CA"),
             new("csharp/src/SuperMetroid.Core/Assets/CeresEscapeOverlayTilemapCatalog.cs", "DCEEA9B948035D40AC9E6145E50C6B79E9F2D1977CB669EE3C16EE4FD9AB0DC3"),
             new("csharp/src/SuperMetroid.Core/Assets/TorizoInstructionVramArtwork.cs", "E713CC6BBF734DD700CBE4397EF8D81ABEDF19299DB65A359453DFC1AEB43FBC"),
             new("csharp/src/SuperMetroid.Core/Assets/TorizoInstructionTileRomData.cs", "10F23555CAF58BB4A559874B08847132683CB1EB1487B3BF51C6B381F93286F1"),
             new("csharp/src/SuperMetroid.Core/Game/CeresEscapeVramTransferDefinitions.cs", "3BA73AC61F296986D8A753AA84AC3DE271EE9AB9B2E73BB54E2B19DBEDB51D39"),
             new("csharp/src/SuperMetroid.Core/Game/CeresRidleyPaletteRomData.cs", "A5FEEC244162AEA362469C0106D049D25DA76D015B6E2ECBD010F349541A9876")]),
    ];
}

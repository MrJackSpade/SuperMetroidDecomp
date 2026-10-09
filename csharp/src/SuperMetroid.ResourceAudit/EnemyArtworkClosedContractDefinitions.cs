namespace SuperMetroid.ResourceAudit;

/// <summary>Complete ordinary sheet/palette admission and owned enemy DMA sources.</summary>
internal static class EnemyArtworkClosedContractDefinitions
{
    /// <summary>Fingerprints that close ordinary enemy sheet, palette and owned-DMA provider domains.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.EnemyTileArtworkCatalog", "installed-enemy-sheets-palettes-and-dma", ["LoadTo", "LoadPaletteTo", "TryResolve"],
            // #1172: reviewed schema 68 -> 69 bump; provider and DMA routing are unchanged.
            [new("csharp/src/SuperMetroid.Core/Assets/EnemyTileArtworkCatalog.cs", "A4242F0106C1E585671DCBCFDDA05166997117822289AD3DF2889B72AA0888E7"),
             new("csharp/src/SuperMetroid.Core/Assets/EnemyTileArtworkCatalog.Construction.cs", "DD1DF29F72BB8EE7BA61D15BF3FBE892BDDA0A410A95327F3E8E4A6B76E8802E"),
             new("csharp/src/SuperMetroid.Core/Assets/EnemyTileSourceDefinitions.cs", "4840EB5F05C07FDEB5A2C569326FB650D6EA2EA442410B14C26EA915FFE9B49E"),
             new("csharp/src/SuperMetroid.Core/Game/RoomEnemyGraphicsSetDefinitions.cs", "37AE47CA99314E6511E41D106874BE956CD8170AE7A5D7A442C46F9930F4F4CD"),
             new("csharp/src/SuperMetroid.Core/Game/RoomEnemyGraphicsSetDefinitions.Segment0.cs", "5380EFED9822C09CA85770845263411EC3D301C70629D5CCAC950A859690B93F"),
             new("csharp/src/SuperMetroid.Core/Game/RoomEnemyGraphicsSetDefinitions.Segment1.cs", "8209B4864E4E80A800136C3D25998F3EF941BC2D97FAB80B1C3C9CA091D5CC72"),
             new("csharp/src/SuperMetroid.Core/Game/RoomEnemyDefinitionCatalog.cs", "FF6B7CDF86FADEC16BD4E7CA4BFE62D91E7E265844ABA64DCA2A5C022BA7A6FA"),
             new("csharp/src/SuperMetroid.Core/Game/RoomEnemyData.cs", "D4AC062F0E3692EBA649EB9AE2A49D39C42BF3D5DC4F47C2D93D968BD545D6D8"),
             new("csharp/src/SuperMetroid.Core/Assets/RoomCharacterAtlas.cs", "2C71C8210FE457FA1D1FF6FFF05EE71BC9BBB661E562CA156F44A6FDEF7B985C"),
             new("csharp/src/SuperMetroid.Core/Assets/EnemyPaletteSheet.cs", "3E6B2C3A32C7F0B5849F81698B09D727ED62C21E092E2E3278CA724DFD3FDC6B"),
             new("csharp/src/SuperMetroid.Core/Assets/CeresEscapeTileArtwork.cs", "C97DFD5E72DAB11249F88DBCCEF5996F602A6560680E64FDB3667C51DFE1B35F"),
             new("csharp/src/SuperMetroid.Core/Assets/CeresEscapeTileRomData.cs", "E7051AB9EF862988C95DEC6DECC4C2A1C38D696942F445E1259BC58E33B89A93"),
             new("csharp/src/SuperMetroid.Core/Assets/CeresEscapeOverlayTilemapCatalog.cs", "00A6C0222B8FCFADCE360EA3AAA08793612FD933C700346AF5F813796709AF92"),
             new("csharp/src/SuperMetroid.Core/Assets/TorizoInstructionVramArtwork.cs", "DCCBBD1AD05A05241997D84D3BD6192C20803411399A91C76C535339ECA1371A"),
             new("csharp/src/SuperMetroid.Core/Assets/TorizoInstructionTileRomData.cs", "CBFA2E7FD479107E91B024AB67CB061356B484E6496401738115592B1C9FB708"),
             new("csharp/src/SuperMetroid.Core/Game/CeresEscapeVramTransferDefinitions.cs", "9E546C3E2FC69B26020FEDF3EAAD3186F91A3C69EE84384F419E1AE0A8D0BBC9"),
             new("csharp/src/SuperMetroid.Core/Game/CeresRidleyPaletteRomData.cs", "E3EFB5FA4E69B08685BA5D077C28F6A8818EF2EA9514284BEB3419E0F16EB406")]),
    ];
}

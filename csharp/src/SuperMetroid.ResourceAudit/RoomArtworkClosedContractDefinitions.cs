namespace SuperMetroid.ResourceAudit;

/// <summary>Complete graphics-set source membership and the contiguous sky transfer store.</summary>
internal static class RoomArtworkClosedContractDefinitions
{
    private static readonly ReviewedSource TilesetSources = new(
        "csharp/src/SuperMetroid.Core/Rooms/RoomTilesetDefinitions.cs", "56273CD38941D7860E1D9670FFB167FE11012732F3E4D700726CA20283C8C1C4");
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.RoomCharacterAtlasCatalog", "room-complete-tileset-character-sources", ["Get"],
            [TilesetSources, new("csharp/src/SuperMetroid.Core/Assets/RoomCharacterAtlasCatalog.cs", "E8800C6B90309ADC667C7014DDE81918307A5D56B2587BEF968E2A665B858BA6")],
            "Public construction checks every character source selected by all 29 compiled graphics sets for a nonnull atlas, then copies the dictionary. Get's reviewed domain is that required source set, not arbitrary optional keys. Caller graphics-set selection, atlas geometry, CRE properties, upload placement and pixels are not certified."),
        new("SuperMetroid.Core.Assets.RoomMetatileCatalog", "room-complete-tileset-metatile-sources", ["Get"],
            [TilesetSources, new("csharp/src/SuperMetroid.Core/Assets/RoomMetatileCatalog.cs", "A1BA5CE10E43B8B0FA70C8ECB783ADEA47F824B53466CC69101004E7596D2351")],
            "Public construction checks every block-definition source selected by all 29 graphics sets for a nonnull atlas and copies the dictionary. Get's required sparse source set is complete. This does not certify optional keys, CRE properties, level collision/BTS, selected tile positions or pixels."),
        new("SuperMetroid.Core.Assets.RoomStaticPaletteCatalog", "room-complete-tileset-palette-sources", ["Get"],
            [TilesetSources, new("csharp/src/SuperMetroid.Core/Assets/RoomStaticPaletteCatalog.cs", "77527587EDA3C37432261E2903334603D5A7EF75C4AFABADBB9108AADD488D62")],
            "Public construction requires a nonnull palette for every source selected by all 29 graphics sets and independently copies the dictionary. This proves required-source membership only, not optional keys, palette selection, CGRAM destinations, FX interactions or colors."),
        new("SuperMetroid.Core.Assets.RoomSkyTilemapCatalog", "room-complete-seven-page-sky-transfer-store", ["TryResolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/RoomSkyTilemapCatalog.cs", "76A235967E736A9B6FDC9CA2E67B26FEC0DBE71074D63F83A901608AC7E48ABA"),
             new("csharp/src/SuperMetroid.Core/Assets/RoomBackgroundTilemapAtlas.cs", "9FA2182B31CC5B874EC8DD279E72208D7EAA60147C92BF6099B21DCF99863A07"),
             new("csharp/src/SuperMetroid.Core/Game/RoomFxRomData.cs", "A98454755D909EA0A4C25C901602544A1875948D2A5939DA1731A5906D84B333")],
            "Public construction copies every byte of all seven complete ordered sky pages. Resolve accepts only aligned full pages or even-addressed complete 64-byte rows inside that store; unowned sources return false. Known source/length correlations are checked without imposing page alignment on native mid-page row overreads. Camera arithmetic, sky ordering, VRAM handoff and pixels are not certified."),
    ];
}

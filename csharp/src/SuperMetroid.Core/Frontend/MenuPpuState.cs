using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Shared PPU memory image installed by <c>LoadInitialMenuTiles</c> at $81:8DDB.
/// </summary>
/// <remarks>
/// File select, options, and several later menus share these literal DMA transfers. Owning
/// them once prevents each screen from accumulating its own magic copy of the same banks,
/// byte counts, VRAM destinations, and PPU base words.
/// </remarks>
internal sealed class MenuPpuState
{
    public static ushort ObjectPaletteBits => SnesObjPalettes.Index7.PaletteBits;
    public const int SpritemapPointerTableAddress = 0x82c569;

    public MenuPpuState(ISnesAddressSpace bus, MapTileAtlas? mapTiles = null, MapStaticPalettes? mapPalettes = null, WorldMapArtwork? worldArtwork = null, MapSpriteCatalog? sprites = null,
        bool loadInitialBackground = true)
    {
        Ensure.NotNull(bus);
        BindWorldArtwork(worldArtwork);
        BindMapTiles(mapTiles);
        BindMapSprites(sprites);
        BindMapPalettes(mapPalettes);
        // Saved-map views do not consume this template: the world view excludes
        // BG2 from its layers, and room select installs its own complete frame.
        // Other menus still require the shared native initialization.
        if (loadInitialBackground)
            throw new InvalidOperationException(
                "Menu PPU requires an installed background page from its owning menu.");
    }

    /// <summary>Refreshes the two world character regions only; retains tilemaps and ongoing palette state.</summary>
    public void BindWorldArtwork(WorldMapArtwork? worldArtwork)
    {
        (worldArtwork ?? throw new InvalidOperationException(
            "Menu PPU requires installed world-map artwork.")).LoadTo(Vram);
    }

    public SnesVram Vram { get; } = new();

    public void BindMapTiles(MapTileAtlas? mapTiles)
    {
        (mapTiles ?? throw new InvalidOperationException(
            "Menu PPU requires installed map tiles.")).LoadTo(new MenuVram(Vram).MapCharacters);
    }

    public void BindMapSprites(MapSpriteCatalog? sprites)
    {
        (sprites ?? throw new InvalidOperationException(
            "Menu PPU requires installed sprite artwork."))
            .LoadArtworkTo(new MenuVram(Vram).ObjectCharacters);
    }

    public void BindMapPalettes(MapStaticPalettes? mapPalettes)
    {
        MapStaticPalettes palettes = mapPalettes ?? throw new InvalidOperationException(
            "Menu PPU requires installed palettes.");
        new CgramPalettes(Cgram).LoadImage(palettes.FileSelect);
    }

    public SnesCgram Cgram { get; } = new();

    public void LoadBg1(ReadOnlySpan<byte> tilemapBytes)
    {
        Ensure.LengthEqual(tilemapBytes, 0x0800);
        new MenuVram(Vram).Bg1Tilemap.Load(tilemapBytes);
    }
}

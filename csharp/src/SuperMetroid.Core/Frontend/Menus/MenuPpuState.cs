using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Map.Artwork;

namespace SuperMetroid.Core.Frontend.Menus;

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
    /// <summary>OBJ palette seven bits used by shared menu spritemaps.</summary>
    public static ushort ObjectPaletteBits => SnesObjPalettes.Index7.PaletteBits;

    /// <summary>Installs shared world, map, sprite, and palette assets into menu PPU memory.</summary>
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

    /// <summary>Menu-owned VRAM image receiving shared character and tilemap transfers.</summary>
    public SnesVram Vram { get; } = new();

    /// <summary>Replaces the shared map-character region from installed indexed artwork.</summary>
    public void BindMapTiles(MapTileAtlas? mapTiles)
    {
        (mapTiles ?? throw new InvalidOperationException(
            "Menu PPU requires installed map tiles.")).LoadTo(new MenuVram(Vram).MapCharacters);
    }

    /// <summary>Replaces the menu OBJ-character region from installed sprite artwork.</summary>
    public void BindMapSprites(MapSpriteCatalog? sprites)
    {
        (sprites ?? throw new InvalidOperationException(
            "Menu PPU requires installed sprite artwork."))
            .LoadArtworkTo(new MenuVram(Vram).ObjectCharacters);
    }

    /// <summary>Loads the file-select palette image into menu CGRAM.</summary>
    public void BindMapPalettes(MapStaticPalettes? mapPalettes)
    {
        MapStaticPalettes palettes = mapPalettes ?? throw new InvalidOperationException(
            "Menu PPU requires installed palettes.");
        new CgramPalettes(Cgram).LoadImage(palettes.FileSelect);
    }

    /// <summary>Menu-owned CGRAM image used by file select and related screens.</summary>
    public SnesCgram Cgram { get; } = new();

    /// <summary>Copies a complete 2048-byte BG1 tilemap into the menu VRAM region.</summary>
    public void LoadBg1(ReadOnlySpan<byte> tilemapBytes)
    {
        Ensure.LengthEqual(tilemapBytes, 2048);
        new MenuVram(Vram).Bg1Tilemap.Load(tilemapBytes);
    }
}

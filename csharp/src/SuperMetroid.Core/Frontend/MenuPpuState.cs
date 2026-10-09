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
    /// <summary>PPU BG1 tilemap base word shared by the native menu screens.</summary>
    public const ushort Bg1TilemapWord = SnesPpuLayout.MenuBg1TilemapWord;

    /// <summary>PPU BG2 tilemap base word used by menus that display the second background layer.</summary>
    public const ushort Bg2TilemapWord = SnesPpuLayout.MenuBg2TilemapWord;

    /// <summary>Object-palette attribute bits assigned to menu sprites using palette row seven.</summary>
    public static ushort ObjectPaletteBits => SnesObjPalettes.Index7.PaletteBits;

    /// <summary>ROM address of the native menu spritemap pointer table.</summary>
    public const int SpritemapPointerTableAddress = 0x82c569;

    /// <summary>Creates shared menu PPU memory and binds the installed world, map-tile, sprite, and palette assets.</summary>
    /// <param name="bus">SNES address-space context supplied during setup; installed asset objects provide the copied payloads.</param>
    /// <param name="mapTiles">Installed map character tiles copied to their menu VRAM region.</param>
    /// <param name="mapPalettes">Installed static map colors copied into CGRAM.</param>
    /// <param name="worldArtwork">Installed world-map characters copied into the shared VRAM image.</param>
    /// <param name="sprites">Installed map sprite artwork copied to its file-select VRAM destination.</param>
    /// <param name="loadInitialBackground">Must be false because the owning menu supplies its complete initial background page.</param>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="InvalidOperationException">A required installed asset is missing or <paramref name="loadInitialBackground"/> is true.</exception>
    public MenuPpuState(ISnesAddressSpace bus, MapTileAtlas? mapTiles = null, MapStaticPalettes? mapPalettes = null, WorldMapArtwork? worldArtwork = null, MapSpriteCatalog? sprites = null,
        bool loadInitialBackground = true)
    {
        ArgumentNullException.ThrowIfNull(bus);
        BindWorldArtwork(bus, worldArtwork);
        BindMapTiles(bus, mapTiles);
        BindMapSprites(bus, sprites);
        BindMapPalettes(bus, mapPalettes);
        // Saved-map views do not consume this template: the world view excludes
        // BG2 from its layers, and room select installs its own complete frame.
        // Other menus still require the shared native initialization.
        if (loadInitialBackground)
            throw new InvalidOperationException(
                "Menu PPU requires an installed background page from its owning menu.");
    }

    /// <summary>Refreshes the two world character regions only; retains tilemaps and ongoing palette state.</summary>
    public void BindWorldArtwork(ISnesAddressSpace bus, WorldMapArtwork? worldArtwork)
    {
        (worldArtwork ?? throw new InvalidOperationException(
            "Menu PPU requires installed world-map artwork.")).LoadTo(Vram);
    }

    /// <summary>Shared VRAM image receiving menu character data and tilemaps.</summary>
    public SnesVram Vram { get; } = new();

    /// <summary>Copies installed map characters into the shared VRAM region beginning at word address <c>$6000</c>.</summary>
    /// <param name="bus">Address-space context supplied by the menu caller; atlas bytes are copied directly from <paramref name="mapTiles"/>.</param>
    /// <param name="mapTiles">Installed atlas whose tile bytes are copied into VRAM.</param>
    /// <exception cref="InvalidOperationException"><paramref name="mapTiles"/> is missing.</exception>
    public void BindMapTiles(ISnesAddressSpace bus, MapTileAtlas? mapTiles)
    {
        (mapTiles ?? throw new InvalidOperationException(
            "Menu PPU requires installed map tiles.")).LoadTo(Vram, 0x6000);
    }

    /// <summary>Copies installed file-select sprite characters into their assigned VRAM range.</summary>
    /// <param name="bus">Address-space context supplied by the menu caller; sprite bytes are copied directly from <paramref name="sprites"/>.</param>
    /// <param name="sprites">Installed sprite catalog supplying the artwork.</param>
    /// <exception cref="InvalidOperationException"><paramref name="sprites"/> is missing.</exception>
    public void BindMapSprites(ISnesAddressSpace bus, MapSpriteCatalog? sprites)
    {
        (sprites ?? throw new InvalidOperationException(
            "Menu PPU requires installed sprite artwork."))
            .LoadArtworkTo(Vram, MapSpriteFormat.FileSelectDestination);
    }

    /// <summary>Replaces every CGRAM color with the installed file-select palette.</summary>
    /// <param name="bus">Address-space context supplied by the menu caller; colors are read from <paramref name="mapPalettes"/>.</param>
    /// <param name="mapPalettes">Installed static palettes containing the file-select color row.</param>
    /// <exception cref="InvalidOperationException"><paramref name="mapPalettes"/> is missing.</exception>
    public void BindMapPalettes(ISnesAddressSpace bus, MapStaticPalettes? mapPalettes)
    {
        MapStaticPalettes palettes = mapPalettes ?? throw new InvalidOperationException(
            "Menu PPU requires installed palettes.");
        for (int color = 0; color < SnesCgram.ColorCount; color++)
            Cgram.SetColor(color, palettes.FileSelect[color]);
    }

    /// <summary>Shared CGRAM image receiving the selected menu palette and subsequent screen color updates.</summary>
    public SnesCgram Cgram { get; } = new();

    /// <summary>Loads a complete 32-by-32 BG1 tilemap into the shared VRAM image at the native BG1 base.</summary>
    /// <param name="tilemapBytes">Exactly 2048 bytes of little-endian SNES tilemap words in row-major order.</param>
    /// <exception cref="ArgumentException">The supplied data is not exactly one 32-by-32 tilemap.</exception>
    public void LoadBg1(ReadOnlySpan<byte> tilemapBytes)
    {
        if (tilemapBytes.Length != 0x0800)
            throw new ArgumentException("Menu BG1 must contain one 32x32 two-byte tilemap.", nameof(tilemapBytes));
        Vram.LoadBytes(Bg1TilemapWord * 2, tilemapBytes);
    }
}

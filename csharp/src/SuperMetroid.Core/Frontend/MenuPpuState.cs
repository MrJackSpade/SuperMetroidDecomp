using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
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
    public const ushort Bg1TilemapWord = SnesPpuLayout.MenuBg1TilemapWord;
    public const ushort Bg2TilemapWord = SnesPpuLayout.MenuBg2TilemapWord;
    public static ushort ObjectPaletteBits => SnesObjPalettes.Index7.PaletteBits;
    public const int SpritemapPointerTableAddress = 0x82c569;

    public MenuPpuState(ISnesAddressSpace bus, MapTileAtlas? mapTiles = null, MapStaticPalettes? mapPalettes = null, WorldMapArtwork? worldArtwork = null, MapSpriteCatalog? sprites = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        BindWorldArtwork(bus, worldArtwork);
        if (mapTiles is null) Vram.LoadBytes(0x6000, RomDataReader.ReadFixedBank(bus, MapTileAtlasFormat.SourceAddress, MapTileAtlasFormat.ByteCount));
        else mapTiles.LoadTo(Vram, 0x6000);
        BindMapSprites(bus, sprites);
        if (mapPalettes is null) Cgram.LoadFromBus(bus, FileSelectMapRomData.EntryPalette);
        else for (int color = 0; color < SnesCgram.ColorCount; color++) Cgram.SetColor(color, mapPalettes.FileSelect[color]);
        Vram.LoadBytes(Bg2TilemapWord * 2, RomDataReader.ReadFixedBank(bus, 0x8edc00, 0x0800));
    }

    /// <summary>Refreshes the two world character regions only; retains tilemaps and ongoing palette state.</summary>
    public void BindWorldArtwork(ISnesAddressSpace bus, WorldMapArtwork? worldArtwork)
    {
        if (worldArtwork is null)
        {
            Vram.LoadBytes(WorldMapArtworkFormat.ForegroundDestination, RomDataReader.ReadFixedBank(bus, WorldMapArtworkFormat.ForegroundSource, WorldMapArtworkFormat.ForegroundBytes));
            Vram.LoadBytes(WorldMapArtworkFormat.BackgroundDestination, RomDataReader.ReadFixedBank(bus, WorldMapArtworkFormat.BackgroundSource, WorldMapArtworkFormat.BackgroundBytes));
        }
        else worldArtwork.LoadTo(Vram);
    }

    public SnesVram Vram { get; } = new();
    public void BindMapSprites(ISnesAddressSpace bus, MapSpriteCatalog? sprites)
    {
        if (sprites is null) Vram.LoadBytes(MapSpriteFormat.FileSelectDestination, RomDataReader.ReadFixedBank(bus, MapSpriteFormat.SourceAddress, MapSpriteFormat.ByteCount));
        else sprites.LoadArtworkTo(Vram, MapSpriteFormat.FileSelectDestination);
    }

    public SnesCgram Cgram { get; } = new();

    public void LoadBg1(ReadOnlySpan<byte> tilemapBytes)
    {
        if (tilemapBytes.Length != 0x0800)
            throw new ArgumentException("Menu BG1 must contain one 32x32 two-byte tilemap.", nameof(tilemapBytes));
        Vram.LoadBytes(Bg1TilemapWord * 2, tilemapBytes);
    }
}

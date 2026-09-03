using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

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
    public const ushort ObjectPaletteBits = 0x0e00;
    public const int SpritemapPointerTableAddress = 0x82c569;

    public MenuPpuState(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        Vram.LoadBytes(0x0000, RomDataReader.ReadFixedBank(bus, 0x8e8000, 0x5600));
        Vram.LoadBytes(0x6000, RomDataReader.ReadFixedBank(bus, 0xb68000, 0x2000));
        Vram.LoadBytes(0xc000, RomDataReader.ReadFixedBank(bus, 0xb6c000, 0x2000));
        Vram.LoadBytes(0x8000, RomDataReader.ReadFixedBank(bus, 0x8ed600, 0x0600));
        Cgram.LoadFromBus(bus, 0x8ee400);
        Vram.LoadBytes(Bg2TilemapWord * 2, RomDataReader.ReadFixedBank(bus, 0x8edc00, 0x0800));
    }

    public SnesVram Vram { get; } = new();

    public SnesCgram Cgram { get; } = new();

    public void LoadBg1(ReadOnlySpan<byte> tilemapBytes)
    {
        if (tilemapBytes.Length != 0x0800)
            throw new ArgumentException("Menu BG1 must contain one 32x32 two-byte tilemap.", nameof(tilemapBytes));
        Vram.LoadBytes(Bg1TilemapWord * 2, tilemapBytes);
    }
}

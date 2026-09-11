using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Tile and palette owners for the saved-game world map, from $81:A37C-$81:A58A.
/// Labels and transition windows are separate owners; this renders the unmasked BGs.
/// </summary>
public sealed partial class FileSelectAreaMapGraphics
{
    private readonly ISnesAddressSpace bus;
    private readonly MenuPpuState ppu;

    public FileSelectAreaMapGraphics(ISnesAddressSpace bus, int selectedArea, MapTileAtlas? mapTiles = null)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        ppu = new MenuPpuState(bus, mapTiles);
        ppu.LoadBg1(RomDataReader.ReadFixedBank(bus, FileSelectMapRomData.AreaForeground, FileSelectMapRomData.TilemapBytes));
        // State one completes its first-two-palette fade with these entries black.
        ppu.Cgram.SetColor(14, 0);
        ppu.Cgram.SetColor(30, 0);
        SelectArea(selectedArea);
    }

    public SnesVram Vram => ppu.Vram;
    public SnesCgram Cgram => ppu.Cgram;
    public int SelectedArea { get; private set; }

    public void SelectArea(int selectedArea)
    {
        if ((uint)selectedArea >= FileSelectMapRomData.AreaCount)
            throw new ArgumentOutOfRangeException(nameof(selectedArea));
        for (int area = 0; area < FileSelectMapRomData.AreaCount; area++)
            LoadAreaPalette(area, area == selectedArea);
        ppu.Vram.LoadBytes(FileSelectMapRomData.AreaBackgroundVram * 2,
            RomDataReader.ReadFixedBank(bus,
                FileSelectMapRomData.AreaBackgrounds + selectedArea * FileSelectMapRomData.TilemapBytes,
                FileSelectMapRomData.TilemapBytes));
        SelectedArea = selectedArea;
    }

    private void LoadAreaPalette(int area, bool active)
    {
        int offsets = active ? FileSelectMapRomData.ActivePaletteOffsets : FileSelectMapRomData.InactivePaletteOffsets;
        int cursor = FileSelectMapRomData.PalettePrograms + RomDataReader.ReadWordFixedBank(bus, offsets + area * 2);
        // At most one whole bank of four-byte records can exist. Retain a corruption
        // guard instead of silently accepting an unterminated palette program.
        for (int records = 0; records < 16384; records++, cursor += 4)
        {
            ushort source = RomDataReader.ReadWordFixedBank(bus, cursor);
            if (source == ushort.MaxValue) return;
            ushort destination = RomDataReader.ReadWordFixedBank(bus, cursor + 2);
            if ((destination & 1) != 0 || destination / 2 + 5 > SnesCgram.ColorCount)
                throw new InvalidDataException("Area-map palette program addresses invalid CGRAM colors.");
            ppu.Cgram.LoadFromBus(bus, FileSelectMapRomData.PaletteColors + source, 5, destination / 2);
        }
        throw new InvalidDataException("Area-map palette program has no terminator.");
    }

    /// <summary>
    /// Steady state six leaves BG1 on main and BG3 on sub, with additive color math
    /// enabled for BG1 and backdrop. Menu OBJ labels must be composed afterwards.
    /// </summary>
    public Rgba32[] RenderBackgrounds(bool includeBackdropInColorMath = true)
    {
        Rgba32[] pixels = SnesLayerCompositor.CreateBackdrop(ppu.Cgram, 256 * 224);
        Rgba32[] foreground = SnesBgTilemapRenderer.Render4BppViewport(
            ppu.Vram, ppu.Cgram, MenuPpuState.Bg1TilemapWord, 0, 0, 0, 256, 224, 32, 32);
        SnesLayerCompositor.Composite(pixels, foreground);
        Rgba32[] subscreen = SnesBgTilemapRenderer.Render2Bpp(ppu.Vram, ppu.Cgram,
            FileSelectMapRomData.AreaBackgroundVram, FileSelectMapRomData.AreaBackgroundCharacters,
            28, transparentColorZero: true);
        // $81:AAAC changes CGADSUB from $25 to $05: BG1 still adds BG3, but
        // transparent BG1 cells now retain the backdrop instead of adding BG3 to it.
        if (!includeBackdropInColorMath)
            for (int pixel = 0; pixel < subscreen.Length; pixel++)
                if (foreground[pixel].A == 0) subscreen[pixel] = default;
        SnesLayerCompositor.AddSubscreen(pixels, subscreen);
        return pixels;
    }

    /// <summary>
    /// Adds $81:A97E's label objects. Retail enables a label only if a used station
    /// bit refers to a real map coordinate, not an unused $FFFE or end $FFFF entry.
    /// Debug-only area/station navigation is deliberately not enabled here.
    /// </summary>
    public Rgba32[] Render(ReadOnlySpan<ushort> usedStationMasks, bool includeBackdropInColorMath = true)
    {
        if (usedStationMasks.Length < FileSelectMapRomData.AreaCount)
            throw new ArgumentException("Area labels require six used-station masks.", nameof(usedStationMasks));
        Rgba32[] frame = RenderBackgrounds(includeBackdropInColorMath);
        OamBuffer oam = PrepareLabels(usedStationMasks);
        // CGADSUB excludes OBJ: labels are composited after BG1/subscreen addition.
        SnesLayerCompositor.Composite(frame, SnesObjRenderer.Render(oam, ppu.Vram, ppu.Cgram, obsel: 0x03));
        return frame;
    }

    private OamBuffer PrepareLabels(ReadOnlySpan<ushort> usedStationMasks)
    {
        if (usedStationMasks.Length < FileSelectMapRomData.AreaCount)
            throw new ArgumentException("Area labels require six used-station masks.", nameof(usedStationMasks));
        var oam = new OamBuffer();
        oam.BeginFrame();
        ushort title = RomDataReader.ReadWordFixedBank(bus, FileSelectMapRomData.LabelSpritemapBase);
        Draw(title, 128, 16, 0);
        for (int displayArea = 0; displayArea < FileSelectMapRomData.AreaCount; displayArea++)
        {
            ushort area = RomDataReader.ReadWordFixedBank(bus, FileSelectMapRomData.DisplayAreaIndices + displayArea * 2);
            if (area >= FileSelectMapRomData.AreaCount)
                throw new InvalidDataException("File-select map display table contains an invalid area.");
            ushort pointer = RomDataReader.ReadWordFixedBank(bus, FileSelectMapRomData.SavePointMapPointers + area * 2);
            for (int station = 0; station < 16; station++)
            {
                ushort x = RomDataReader.ReadWordFixedBank(bus, FileSelectMapRomData.MenuObjectBank | (ushort)(pointer + station * 4));
                if (x == ushort.MaxValue) break;
                if (x == ushort.MaxValue - 1 || (usedStationMasks[area] & (1 << station)) == 0) continue;
                int label = FileSelectMapRomData.LabelPositions + area * 4;
                Draw((ushort)(title + area + 1), RomDataReader.ReadWordFixedBank(bus, label),
                    RomDataReader.ReadWordFixedBank(bus, label + 2), area == SelectedArea ? (ushort)0 : (ushort)0x200);
                break;
            }
        }
        oam.FinalizeFrame();
        return oam;

        void Draw(ushort id, ushort x, ushort y, ushort palette)
        {
            ushort pointer = RomDataReader.ReadWordFixedBank(bus, MenuPpuState.SpritemapPointerTableAddress + id * 2);
            oam.AddOnScreenSpritemap(bus, FileSelectMapRomData.MenuObjectBank | pointer, x, y, palette);
        }
    }
}

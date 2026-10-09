using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Resolves native palette-copy programs at import time into named world-selection palettes.</summary>
internal static class MapStaticPalettesExtractor
{
    /// <summary>Resolves pause, file-select, and per-area active/inactive palette programs into presentation JSON.</summary>
    /// <param name="bus">Cartridge address space containing palette colors, copy lists, and map-role selectors.</param>
    /// <returns>Serialized RGB5 palettes keyed by world-map area and UI role.</returns>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        var pause = new SnesCgram(); CartridgePaletteImporter.LoadToCgram(pause, bus, MapStaticPalettesRomData.PausePalette);
        var file = new SnesCgram(); CartridgePaletteImporter.LoadToCgram(file, bus, FileSelectMapRomData.EntryPalette);
        var world = new Dictionary<string, PaletteRgb5[]>();
        for (int selected = 0; selected < FileSelectMapRomData.AreaCount; selected++)
        {
            var colors = new SnesCgram(); CartridgePaletteImporter.LoadToCgram(colors, bus, FileSelectMapRomData.EntryPalette);
            colors.SetColor(14, 0); colors.SetColor(30, 0);
            for (int area = 0; area < FileSelectMapRomData.AreaCount; area++)
            {
                int offsets = area == selected ? FileSelectMapRomData.ActivePaletteOffsets : FileSelectMapRomData.InactivePaletteOffsets;
                int cursor = FileSelectMapRomData.PalettePrograms + RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), offsets + area * sizeof(ushort));
                bool terminated = false;
                for (int record = 0; record < MapStaticPalettesRomData.MaximumCopyRecords; record++, cursor += MapStaticPalettesRomData.CopyRecordBytes)
                {
                    ushort source = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), cursor);
                    if (source == ushort.MaxValue) { terminated = true; break; }
                    ushort destination = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), cursor + sizeof(ushort));
                    if ((destination & 1) != 0 || destination / 2 + MapStaticPalettesRomData.CopyColorCount > SnesCgram.ColorCount)
                        throw new InvalidDataException("World-map palette copy exceeds the color range.");
                    CartridgePaletteImporter.LoadToCgram(colors, bus, FileSelectMapRomData.PaletteColors + source, MapStaticPalettesRomData.CopyColorCount, destination / 2);
                }
                if (!terminated) throw new InvalidDataException("World-map palette copy program has no terminator.");
            }
            world.Add(((AreaId)selected).ToString(), ToRgb(colors));
        }
        using var json = new MemoryStream();
        MapStaticPalettes.Write(json, new() { Version = MapStaticPalettesFormat.Version, Pause = ToRgb(pause), FileSelect = ToRgb(file), World = world });
        return json.ToArray();
    }
    /// <summary>Converts CGRAM words into the editable RGB5 palette representation.</summary>
    /// <param name="palette">Palette colors in SNES packed RGB5 format.</param>
    /// <returns>Colors with separate five-bit red, green, and blue channels.</returns>
    private static PaletteRgb5[] ToRgb(SnesCgram palette) => palette.Colors.ToArray()
        .Select(word => new PaletteRgb5 { Red = word & 31, Green = word >> 5 & 31, Blue = word >> 10 & 31 }).ToArray();
}

using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Resolves native palette-copy programs at import time into named world-selection palettes.</summary>
internal static class MapStaticPalettesExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        var pause = new SnesCgram(); pause.LoadFromBus(bus, MapStaticPalettesRomData.PausePalette);
        var file = new SnesCgram(); file.LoadFromBus(bus, FileSelectMapRomData.EntryPalette);
        var world = new Dictionary<string, PaletteRgb5[]>();
        for (int selected = 0; selected < FileSelectMapRomData.AreaCount; selected++)
        {
            var colors = new SnesCgram(); colors.LoadFromBus(bus, FileSelectMapRomData.EntryPalette);
            colors.SetColor(14, 0); colors.SetColor(30, 0);
            for (int area = 0; area < FileSelectMapRomData.AreaCount; area++)
            {
                int offsets = area == selected ? FileSelectMapRomData.ActivePaletteOffsets : FileSelectMapRomData.InactivePaletteOffsets;
                int cursor = FileSelectMapRomData.PalettePrograms + RomDataReader.ReadWordFixedBank(bus, offsets + area * sizeof(ushort));
                bool terminated = false;
                for (int record = 0; record < MapStaticPalettesRomData.MaximumCopyRecords; record++, cursor += MapStaticPalettesRomData.CopyRecordBytes)
                {
                    ushort source = RomDataReader.ReadWordFixedBank(bus, cursor);
                    if (source == ushort.MaxValue) { terminated = true; break; }
                    ushort destination = RomDataReader.ReadWordFixedBank(bus, cursor + sizeof(ushort));
                    if ((destination & 1) != 0 || destination / 2 + MapStaticPalettesRomData.CopyColorCount > SnesCgram.ColorCount)
                        throw new InvalidDataException("World-map palette copy exceeds the color range.");
                    colors.LoadFromBus(bus, FileSelectMapRomData.PaletteColors + source, MapStaticPalettesRomData.CopyColorCount, destination / 2);
                }
                if (!terminated) throw new InvalidDataException("World-map palette copy program has no terminator.");
            }
            world.Add(((AreaId)selected).ToString(), ToRgb(colors));
        }
        using var json = new MemoryStream();
        MapStaticPalettes.Write(json, new() { Version = MapStaticPalettesFormat.Version, Pause = ToRgb(pause), FileSelect = ToRgb(file), World = world });
        return json.ToArray();
    }
    private static PaletteRgb5[] ToRgb(SnesCgram palette) => palette.Colors.ToArray()
        .Select(word => new PaletteRgb5 { Red = word & 31, Green = word >> 5 & 31, Blue = word >> 10 & 31 }).ToArray();
}

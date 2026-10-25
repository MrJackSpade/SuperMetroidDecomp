using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Resolves native palette-copy programs at import time into named world-selection palettes.</summary>
[CpuBusBoundary("codec: ROM map palette-copy programs and colors → map world-selection palettes")]
internal static class MapStaticPalettesExtractor
{
    /// <summary>
    /// Imports pause and file-select palettes, executes each selected area's bounded native
    /// world-map copy program, and emits validated RGB5 JSON for all six selections.
    /// </summary>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        var pause = new SnesCgram(); CartridgePaletteImporter.LoadToCgram(pause, bus, MapSourceDefinitions.Artwork.PauseColors);
        var file = new SnesCgram(); CartridgePaletteImporter.LoadToCgram(file, bus, MapSourceDefinitions.Menu.Colors);
        var world = new Dictionary<string, PaletteRgb5[]>();
        for (int selected = 0; selected < FileSelectMapDefinitions.AreaCount; selected++)
        {
            var colors = new SnesCgram(); CartridgePaletteImporter.LoadToCgram(colors, bus, MapSourceDefinitions.Menu.Colors);
            for (int row = 0; row < FileSelectEntryColourDefinitions.BlackRows; row++)
                new CgramPalettes(colors).BackgroundRow(row).Write(FileSelectEntryColourDefinitions.BlackColour, Bgr555.Black);
            for (int area = 0; area < FileSelectMapDefinitions.AreaCount; area++)
            {
                int offsets = area == selected ? MapSourceDefinitions.FileSelectMap.ActivePaletteOffsets : MapSourceDefinitions.FileSelectMap.InactivePaletteOffsets;
                int cursor = MapSourceDefinitions.FileSelectMap.PalettePrograms + RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), offsets + area * sizeof(ushort));
                bool terminated = false;
                for (int record = 0; record < MapSourceDefinitions.Artwork.MaximumCopyRecords; record++, cursor += MapSourceDefinitions.Artwork.CopyRecordBytes)
                {
                    ushort source = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), cursor);
                    if (source == ushort.MaxValue) { terminated = true; break; }
                    ushort destination = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), cursor + sizeof(ushort));
                    if ((destination & 1) != 0 || destination / 2 + MapSourceDefinitions.Artwork.CopyColorCount > CgramPalettes.ColorCount)
                        throw new InvalidDataException("World-map palette copy exceeds the color range.");
                    CartridgePaletteImporter.LoadToCgram(colors, bus, MapSourceDefinitions.FileSelectMap.PaletteColors + source, MapSourceDefinitions.Artwork.CopyColorCount, destination / 2);
                }
                if (!terminated) throw new InvalidDataException("World-map palette copy program has no terminator.");
            }
            world.Add(((AreaId)selected).ToString(), ToRgb(colors));
        }
        using var json = new MemoryStream();
        MapStaticPalettesFile.Write(json, new() { Version = MapStaticPalettesFormat.Version, Pause = ToRgb(pause), FileSelect = ToRgb(file), World = world });
        return json.ToArray();
    }
    /// <summary>Converts all CGRAM words to RGB5 values for the installed palette document.</summary>
    private static PaletteRgb5[] ToRgb(SnesCgram palette) => new CgramPalettes(palette).Image.SelectToArray(PaletteRgb5.From);
}

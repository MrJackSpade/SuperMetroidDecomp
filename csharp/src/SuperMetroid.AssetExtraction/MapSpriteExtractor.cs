using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Imports ordered map sprite artwork only, excluding all game collision and navigation data.</summary>
public static class MapSpriteExtractor
{
    public static Dictionary<string, byte[]> Extract(ISnesAddressSpace bus)
    {
        if (RomDataReader.ReadWordFixedBank(bus, FileSelectMapRomData.LabelSpritemapBase) != MapSpriteDefinitions.WorldTitle)
            throw new InvalidDataException("Unexpected native world-title sprite binding.");
        var frames = new Dictionary<string, SpriteVisualPart[]>();
        foreach (var definition in MapSpriteDefinitions.Frames)
        {
            int address = FileSelectMapRomData.MenuObjectBank | RomDataReader.ReadWordFixedBank(bus, MenuPpuState.SpritemapPointerTableAddress + definition.NativeId * 2);
            int count = RomDataReader.ReadWordFixedBank(bus, address);
            if (count > MapSpriteFormat.MaximumParts) throw new InvalidDataException($"Map sprite {definition.Name} exceeds OAM capacity.");
            var parts = new SpriteVisualPart[count];
            for (int index = 0; index < count; index++)
            {
                int part = address + 2 + index * 5;
                var x = new SnesSpritemapXWord(RomDataReader.ReadWordFixedBank(bus, part));
                var attributes = new SnesObjAttributeWord(RomDataReader.ReadWordFixedBank(bus, part + 3));
                parts[index] = new() { OffsetX = x.SignedOffset, OffsetY = unchecked((sbyte)bus.ReadByte(part + 2)),
                    TileColumn = attributes.TileNumber % MapSpriteFormat.TileColumns, TileRow = attributes.TileNumber / MapSpriteFormat.TileColumns,
                    Size = x.IsLarge ? 16 : 8, Priority = attributes.Priority, FlipX = attributes.FlipHorizontally, FlipY = attributes.FlipVertically,
                    // $81:879F replaces source palette bits with its caller's live palette.
                    Palette = null };
            }
            frames.Add(definition.Name, parts);
        }
        byte[] pixels = SnesGraphics.DecodePlanarTiles(RomDataReader.ReadFixedBank(bus, MapSpriteFormat.SourceAddress, MapSpriteFormat.ByteCount),
            4, MapSpriteFormat.TileColumns, out int width, out int height);
        using var png = new MemoryStream();
        IndexedPng.Write(png, width, height, pixels, SnesGraphics.DiagnosticPalette(16));
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(new MapSpriteDocument { Version = MapSpriteFormat.Version, Frames = frames },
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true });
        byte[] pngBytes = png.ToArray();
        _ = MapSpriteCatalog.Load(new MemoryStream(json), new MemoryStream(pngBytes));
        return new() { [MapSpriteFormat.JsonFile] = json, [MapSpriteFormat.PngFile] = pngBytes };
    }
}

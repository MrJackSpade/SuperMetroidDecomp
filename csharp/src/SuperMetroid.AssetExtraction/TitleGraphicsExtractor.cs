using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Expands title graphics and OBJ compositions into indexed PNG/JSON assets.</summary>
internal static class TitleGraphicsExtractor
{
    public static IReadOnlyDictionary<string, byte[]> Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        byte[] mode7 = Read(
            TitleSequenceRomData.Assets.Mode7CharactersAddress,
            TitleSequenceRomData.Vram.Mode7CharacterByteCount,
            "Mode 7 characters");
        byte[] map = Read(
            TitleSequenceRomData.Assets.Mode7MapAddress,
            TitleSequenceRomData.Vram.Mode7MapByteCount,
            "Mode 7 map");
        byte[] objects = Read(
            TitleSequenceRomData.Assets.ObjectCharactersAddress,
            TitleSequenceRomData.Vram.ObjectCharacterByteCount,
            "OBJ characters");
        byte[] baby = Read(
            TitleSequenceRomData.Assets.BabyMetroidCharactersAddress,
            TitleSequenceRomData.Vram.BabyCharacterByteCount,
            "Baby Metroid characters");

        byte[] mode7Pixels = SnesGraphics.DecodeMode7Tiles(mode7, 16, out int mode7Width, out int mode7Height);
        byte[] objectPixels = SnesGraphics.DecodePlanarTiles(objects, 4, 32, out int objectWidth, out int objectHeight);
        byte[] babyPixels = SnesGraphics.DecodeMode7Tiles(baby, 4, out int babyWidth, out int babyHeight);
        if (mode7Width != TitleGraphicsFormat.Mode7Width || mode7Height != TitleGraphicsFormat.Mode7Height ||
            objectWidth != TitleGraphicsFormat.ObjectWidth || objectHeight != TitleGraphicsFormat.ObjectHeight ||
            babyWidth != TitleGraphicsFormat.BabyWidth || babyHeight != TitleGraphicsFormat.BabyHeight)
            throw new InvalidDataException("Title graphics decoded to unexpected atlas dimensions.");

        var files = new Dictionary<string, byte[]>(StringComparer.Ordinal)
        {
            [TitleGraphicsFormat.Mode7TilesFile] = Png(mode7Width, mode7Height, mode7Pixels, 256),
            [TitleGraphicsFormat.ObjectTilesFile] = Png(objectWidth, objectHeight, objectPixels, 16),
            [TitleGraphicsFormat.BabyTilesFile] = Png(babyWidth, babyHeight, babyPixels, 256),
        };
        using var mapJson = new MemoryStream();
        TitleGraphicsPresentation.WriteMap(mapJson, new TitleMode7MapDocument
        {
            Version = TitleGraphicsFormat.Version,
            Width = TitleGraphicsFormat.MapWidth,
            Height = TitleGraphicsFormat.MapHeight,
            Tiles = map.Select(value => (int)value).ToArray(),
            Sprites = TitleSpriteExtractor.Extract(bus),
        });
        files.Add(TitleGraphicsFormat.Mode7MapFile, mapJson.ToArray());
        return files;

        byte[] Read(int address, int count, string name)
        {
            byte[] decompressed = RomDataReader.Decompress(bus, address);
            if (decompressed.Length < count)
                throw new InvalidDataException($"Title {name} stream is shorter than {count} bytes.");
            return decompressed.AsSpan(0, count).ToArray();
        }

        static byte[] Png(int width, int height, byte[] pixels, int colors)
        {
            using var output = new MemoryStream();
            IndexedPng.Write(output, width, height, pixels, SnesGraphics.DiagnosticPalette(colors));
            return output.ToArray();
        }
    }
}

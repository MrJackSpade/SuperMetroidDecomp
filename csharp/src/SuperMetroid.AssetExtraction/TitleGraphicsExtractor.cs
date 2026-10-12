using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction;

/// <summary>Expands title graphics and OBJ compositions into indexed PNG/JSON assets.</summary>
[CpuBusBoundary("codec: ROM compressed title graphics and OBJ compositions → title artwork")]
internal static class TitleGraphicsExtractor
{
    public static IReadOnlyDictionary<string, byte[]> Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
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

        byte[] mode7Pixels = SnesGraphics.DecodeMode7Tiles(mode7, 16, out Extent16<ImagePixels> mode7Size);
        byte[] objectPixels = SnesGraphics.DecodePlanarTiles(objects, 4, 32, out Extent16<ImagePixels> objectSize);
        byte[] babyPixels = SnesGraphics.DecodeMode7Tiles(baby, 4, out Extent16<ImagePixels> babySize);
        if (mode7Size.X != TitleGraphicsFormat.Mode7Width || mode7Size.Y != TitleGraphicsFormat.Mode7Height ||
            objectSize.X != TitleGraphicsFormat.ObjectWidth || objectSize.Y != TitleGraphicsFormat.ObjectHeight ||
            babySize.X != TitleGraphicsFormat.BabyWidth || babySize.Y != TitleGraphicsFormat.BabyHeight)
            throw new InvalidDataException("Title graphics decoded to unexpected atlas dimensions.");

        var files = new Dictionary<string, byte[]>(StringComparer.Ordinal)
        {
            [TitleGraphicsFormat.Mode7TilesFile] = Png(mode7Size, mode7Pixels, 256),
            [TitleGraphicsFormat.ObjectTilesFile] = Png(objectSize, objectPixels, 16),
            [TitleGraphicsFormat.BabyTilesFile] = Png(babySize, babyPixels, 256),
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
            byte[] decompressed = RomDataReader.Decompress(CartridgeImportSource.Require(bus), address);
            if (decompressed.Length < count)
                throw new InvalidDataException($"Title {name} stream is shorter than {count} bytes.");
            return decompressed.AsSpan(0, count).ToArray();
        }

        static byte[] Png(Extent16<ImagePixels> size, byte[] pixels, int colors)
        {
            using var output = new MemoryStream();
            IndexedPng.Write(output, size, pixels, SnesGraphics.DiagnosticPalette(colors));
            return output.ToArray();
        }
    }
}

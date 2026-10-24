using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Frontend.Title;

/// <summary>Expands title graphics and OBJ compositions into indexed PNG/JSON assets.</summary>
[CpuBusBoundary("codec: ROM compressed title graphics and OBJ compositions → title artwork")]
internal static class TitleGraphicsExtractor
{
    /// <summary>Decompresses title character streams and map data into indexed PNGs and a validated map document.</summary>
    public static IReadOnlyDictionary<string, byte[]> Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        byte[] mode7 = Read(
            TitleSequenceRomData.Assets.FloorCharactersAddress,
            TitleSequenceRomData.Transfers.FloorCharacterByteCount,
            "Mode 7 characters");
        byte[] map = Read(
            TitleSequenceRomData.Assets.FloorMapAddress,
            TitleSequenceRomData.Transfers.FloorMapByteCount,
            "Mode 7 map");
        byte[] objects = Read(
            TitleSequenceRomData.Assets.ObjectCharactersAddress,
            TitleSequenceRomData.Transfers.ObjectCharacterByteCount,
            "OBJ characters");
        byte[] baby = Read(
            TitleSequenceRomData.Assets.BabyMetroidCharactersAddress,
            TitleSequenceRomData.Transfers.BabyCharacterByteCount,
            "Baby Metroid characters");

        byte[] mode7Pixels = SnesGraphics.DecodeMode7Tiles(mode7, 16, out Extent16<ImagePixels> mode7Size);
        byte[] objectPixels = SnesGraphics.DecodePlanarTiles(objects, 4, 32, out Extent16<ImagePixels> objectSize);
        byte[] babyPixels = SnesGraphics.DecodeMode7Tiles(baby, 4, out Extent16<ImagePixels> babySize);
        if (mode7Size != new Extent16<ImagePixels>(TitleGraphicsFormat.FloorWidth, TitleGraphicsFormat.FloorHeight) ||
            objectSize != new Extent16<ImagePixels>(TitleGraphicsFormat.ObjectWidth, TitleGraphicsFormat.ObjectHeight) ||
            babySize != new Extent16<ImagePixels>(TitleGraphicsFormat.BabyWidth, TitleGraphicsFormat.BabyHeight))
            throw new InvalidDataException("Title graphics decoded to unexpected atlas dimensions.");

        var files = new Dictionary<string, byte[]>(StringComparer.Ordinal)
        {
            [TitleGraphicsFormat.FloorTilesFile] = Png(mode7Size, mode7Pixels, 256),
            [TitleGraphicsFormat.ObjectTilesFile] = Png(objectSize, objectPixels, 16),
            [TitleGraphicsFormat.BabyTilesFile] = Png(babySize, babyPixels, 256),
        };
        using var mapJson = new MemoryStream();
        TitleGraphicsPresentation.WriteMap(mapJson, new TitleFloorMapDocument
        {
            Version = TitleGraphicsFormat.Version,
            Width = TitleGraphicsFormat.MapWidth,
            Height = TitleGraphicsFormat.MapHeight,
            Tiles = map.Select(value => (int)value).ToArray(),
            Sprites = TitleSpriteExtractor.Extract(bus),
        });
        files.Add(TitleGraphicsFormat.FloorMapFile, mapJson.ToArray());
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

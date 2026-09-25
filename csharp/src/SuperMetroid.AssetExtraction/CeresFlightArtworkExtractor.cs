using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports the Ceres approach's consumed Mode-7 and OBJ visual streams.</summary>
internal static class CeresFlightArtworkExtractor
{
    public static IReadOnlyDictionary<string, byte[]> Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        byte[] characters = Read(CeresFlightRomData.Assets.Mode7Characters,
            CeresFlightArtworkFormat.Mode7ByteCount, "Mode-7 characters");
        byte[] objectCharacters = Read(CeresFlightRomData.Assets.ObjectCharacters,
            CeresFlightArtworkFormat.ObjectByteCount, "OBJ characters");
        byte[] decompressedMap = RomDataReader.Decompress(bus, CeresFlightRomData.Assets.Mode7Maps,
            maximumOutputBytes: 0x1000);
        if (decompressedMap.Length < CeresFlightRomData.Vram.Mode7MapByteCount)
            throw new InvalidDataException("Ceres flight lacks its front and rear Mode-7 map slices.");
        byte[] map = decompressedMap.AsSpan(0, CeresFlightRomData.Vram.Mode7MapByteCount).ToArray();

        byte[] mode7Pixels = SnesGraphics.DecodeMode7Tiles(characters, 16,
            out int mode7Width, out int mode7Height);
        byte[] objPixels = SnesGraphics.DecodePlanarTiles(objectCharacters, 4, 32,
            out int objectWidth, out int objectHeight);
        if (mode7Width != CeresFlightArtworkFormat.Mode7Width ||
            mode7Height != CeresFlightArtworkFormat.Mode7Height ||
            objectWidth != CeresFlightArtworkFormat.ObjectWidth ||
            objectHeight != CeresFlightArtworkFormat.ObjectHeight)
            throw new InvalidDataException("Ceres flight character streams have unexpected dimensions.");

        byte[] mode7Png = Png(mode7Width, mode7Height, mode7Pixels, 256);
        byte[] objectPng = Png(objectWidth, objectHeight, objPixels, 16);
        using var mapJson = new MemoryStream();
        CeresFlightArtworkCatalog.WriteMap(mapJson, new CeresFlightMapDocument
        {
            Version = CeresFlightArtworkFormat.Version,
            Width = CeresFlightArtworkFormat.MapWidth,
            Height = CeresFlightArtworkFormat.MapHeight,
            FrontTiles = map.AsSpan(0, CeresFlightArtworkFormat.MapCellsPerView)
                .ToArray().Select(value => (int)value).ToArray(),
            RearTiles = map.AsSpan(CeresFlightArtworkFormat.MapCellsPerView,
                CeresFlightArtworkFormat.MapCellsPerView)
                .ToArray().Select(value => (int)value).ToArray(),
        });
        byte[] mapFile = mapJson.ToArray();
        byte[] nativePalette = RomDataReader.ReadFixedBank(bus,
            CeresFlightRomData.Assets.Palette, SnesCgram.ByteCount);
        var colors = new PaletteRgb5[SnesCgram.ColorCount];
        for (int index = 0; index < colors.Length; index++)
        {
            ushort word = BinaryPrimitives.ReadUInt16LittleEndian(
                nativePalette.AsSpan(index * sizeof(ushort)));
            if ((word & 0x8000) != 0)
                throw new InvalidDataException(
                    $"Ceres flight palette color {index} has an unrepresentable high bit.");
            colors[index] = new PaletteRgb5
            {
                Red = word & 31,
                Green = word >> 5 & 31,
                Blue = word >> 10 & 31,
            };
        }
        using var paletteJson = new MemoryStream();
        CeresFlightPalette.Write(paletteJson, new CeresFlightPaletteDocument
        {
            Version = CeresFlightPaletteFormat.Version,
            Colors = colors,
        });
        byte[] paletteFile = paletteJson.ToArray();
        CeresFlightArtworkCatalog roundTrip = CeresFlightArtworkCatalog.Load(
            new MemoryStream(mode7Png, writable: false),
            new MemoryStream(mapFile, writable: false),
            new MemoryStream(objectPng, writable: false),
            new MemoryStream(paletteFile, writable: false));
        if (!roundTrip.Mode7Characters.Span.SequenceEqual(characters) ||
            !roundTrip.Mode7Maps.Span.SequenceEqual(map) ||
            !roundTrip.ObjectCharacters.Span.SequenceEqual(objectCharacters) ||
            !roundTrip.Palette.Transfer.Span.SequenceEqual(nativePalette))
            throw new InvalidDataException("Ceres flight PNG/JSON export did not round-trip cartridge bytes.");
        return new Dictionary<string, byte[]>(StringComparer.Ordinal)
        {
            [CeresFlightArtworkFormat.Mode7FileName] = mode7Png,
            [CeresFlightArtworkFormat.MapFileName] = mapFile,
            [CeresFlightArtworkFormat.ObjectFileName] = objectPng,
            [CeresFlightPaletteFormat.FileName] = paletteFile,
        };

        byte[] Read(int address, int expected, string name)
        {
            byte[] decompressed = RomDataReader.Decompress(bus, address,
                maximumOutputBytes: expected);
            if (decompressed.Length != expected)
                throw new InvalidDataException($"Ceres flight {name} has {decompressed.Length} bytes, expected {expected}.");
            return decompressed;
        }

        static byte[] Png(int width, int height, byte[] pixels, int colors)
        {
            using var output = new MemoryStream();
            IndexedPng.Write(output, width, height, pixels, SnesGraphics.DiagnosticPalette(colors));
            return output.ToArray();
        }
    }
}

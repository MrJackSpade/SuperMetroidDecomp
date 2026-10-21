using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core;
using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>Exports the Ceres approach's consumed Mode-7 and OBJ visual streams.</summary>
[CpuBusBoundary("codec: ROM Ceres approach Mode-7 and OBJ graphics streams → Ceres flight artwork")]
internal static class CeresFlightArtworkExtractor
{
    /// <summary>Exports Ceres flight's Mode-7 tiles/maps, OBJ sheets and frames, palette, and actor placements, then round-trips their native bytes.</summary>
    [PpuLayoutBoundary("codec: native 512-byte CGRAM image of the Ceres flight palette")]
    public static IReadOnlyDictionary<string, byte[]> Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        byte[] characters = Read(CeresFlightRomData.Assets.Mode7Characters,
            CeresFlightArtworkFormat.Mode7ByteCount, "Mode-7 characters");
        byte[] objectCharacters = Read(CeresFlightRomData.Assets.ObjectCharacters,
            CeresFlightArtworkFormat.ObjectByteCount, "OBJ characters");
        byte[] decompressedMap = RomDataReader.Decompress(CartridgeImportSource.Require(bus), CeresFlightRomData.Assets.Mode7Maps,
            maximumOutputBytes: 4096);
        if (decompressedMap.Length < CeresFlightRomData.Vram.Mode7MapByteCount)
            throw new InvalidDataException("Ceres flight lacks its front and rear Mode-7 map slices.");
        byte[] map = decompressedMap.AsSpan(0, CeresFlightRomData.Vram.Mode7MapByteCount).ToArray();

        byte[] mode7Pixels = SnesGraphics.DecodeMode7Tiles(characters, 16,
            out Extent16<ImagePixels> mode7Size);
        byte[] objPixels = SnesGraphics.DecodePlanarTiles(objectCharacters, 4, 32,
            out Extent16<ImagePixels> objectSize);
        if (mode7Size != new Extent16<ImagePixels>(CeresFlightArtworkFormat.Mode7Width, CeresFlightArtworkFormat.Mode7Height) ||
            objectSize != new Extent16<ImagePixels>(CeresFlightArtworkFormat.ObjectWidth, CeresFlightArtworkFormat.ObjectHeight))
            throw new InvalidDataException("Ceres flight character streams have unexpected dimensions.");

        byte[] mode7Png = Png(mode7Size, mode7Pixels, 256);
        byte[] objectPng = Png(objectSize, objPixels, 16);
        using var mapJson = new MemoryStream();
        CeresFlightArtworkCatalog.WriteMap(mapJson, new CeresFlightMapDocument
        {
            Version = CeresFlightArtworkFormat.Version,
            Width = CeresFlightArtworkFormat.MapWidth,
            Height = CeresFlightArtworkFormat.MapHeight,
            FrontTiles = map.AsSpan(0, CeresFlightArtworkFormat.MapCellsPerView)
                .SelectToArray(value => (int)value),
            RearTiles = map.AsSpan(CeresFlightArtworkFormat.MapCellsPerView,
                CeresFlightArtworkFormat.MapCellsPerView)
                .SelectToArray(value => (int)value),
        });
        byte[] mapFile = mapJson.ToArray();
        byte[] nativePalette = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
            CeresFlightRomData.Assets.Palette, CgramPalettes.ByteCount);
        var colors = new PaletteRgb5[CgramPalettes.ColorCount];
        for (int index = 0; index < colors.Length; index++)
        {
            ushort word = BinaryPrimitives.ReadUInt16LittleEndian(
                nativePalette.AsSpan(index * sizeof(ushort)));
            if (!Bgr555.IsColorWord(word))
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
        var spriteFrames = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal);
        foreach (CinematicSpriteFrameDefinition definition in CeresFlightSpriteDefinitions.Frames)
            spriteFrames.Add(definition.Name, IntroCinematicSpriteFrameExtractor.Extract(
                bus, CinematicSpritemapAddresses.Of(definition.Pose), definition.StockPartCount, definition.Name));
        using var spritesJson = new MemoryStream();
        CeresFlightSpritePresentation.Write(spritesJson, new CeresFlightSpriteDocument
        {
            Version = CeresFlightSpriteFormat.Version,
            Frames = spriteFrames,
        });
        byte[] spritesFile = spritesJson.ToArray();
        // These positions are presentation operands in the five native initializers.
        // Keep their X/Y words editable while retaining each actor's motion in code.
        var placements = new CeresFlightActorPlacement[
            CeresFlightActorDefinitions.RearViewActorCount];
        for (int index = 0; index < placements.Length; index++)
        {
            var source = CeresFlightActorDefinitions.RearViewPlacementSource(index);
            ushort x = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                CeresFlightActorDefinitions.NativeBank | source.XAddress);
            ushort y = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                CeresFlightActorDefinitions.NativeBank | source.YAddress);
            CeresFlightActorDefinition translated =
                CeresFlightActorDefinitions.RearViewActor(index);
            if (x != translated.Position.X || y != translated.Position.Y)
                throw new InvalidDataException(
                    $"Ceres flight actor {source.Id} differs from its native initializer operands.");
            placements[index] = new CeresFlightActorPlacement
            {
                Id = source.Id, X = x, Y = y,
            };
        }
        using var actorsJson = new MemoryStream();
        CeresFlightActorLayout.Write(actorsJson, new CeresFlightActorLayoutDocument
        {
            Version = CeresFlightActorLayoutFormat.Version,
            Actors = placements,
        });
        byte[] actorsFile = actorsJson.ToArray();
        CeresFlightArtworkCatalog roundTrip = CeresFlightArtworkCatalog.Load(
            new MemoryStream(mode7Png, writable: false),
            new MemoryStream(mapFile, writable: false),
            new MemoryStream(objectPng, writable: false),
            new MemoryStream(paletteFile, writable: false),
            new MemoryStream(spritesFile, writable: false),
            new MemoryStream(actorsFile, writable: false));
        if (!roundTrip.Mode7Characters.Span.SequenceEqual(characters) ||
            !roundTrip.Mode7Maps.Span.SequenceEqual(map) ||
            !roundTrip.ObjectCharacters.Span.SequenceEqual(objectCharacters))
            throw new InvalidDataException("Ceres flight PNG/JSON export did not round-trip cartridge bytes.");
        for (int index = 0; index < CgramPalettes.ColorCount; index++)
            if (roundTrip.Palette.ColorAt(index) != Bgr555.FromCgramPortWord(BinaryPrimitives.ReadUInt16LittleEndian(nativePalette.AsSpan(index * Bgr555.ByteCount))))
                throw new InvalidDataException("Ceres flight palette export did not round-trip cartridge colors.");
        return new Dictionary<string, byte[]>(StringComparer.Ordinal)
        {
            [CeresFlightArtworkFormat.Mode7FileName] = mode7Png,
            [CeresFlightArtworkFormat.MapFileName] = mapFile,
            [CeresFlightArtworkFormat.ObjectFileName] = objectPng,
            [CeresFlightPaletteFormat.FileName] = paletteFile,
            [CeresFlightSpriteFormat.FileName] = spritesFile,
            [CeresFlightActorLayoutFormat.FileName] = actorsFile,
        };

        byte[] Read(int address, int expected, string name)
        {
            byte[] decompressed = RomDataReader.Decompress(CartridgeImportSource.Require(bus), address,
                maximumOutputBytes: expected);
            if (decompressed.Length != expected)
                throw new InvalidDataException($"Ceres flight {name} has {decompressed.Length} bytes, expected {expected}.");
            return decompressed;
        }

        static byte[] Png(Extent16<ImagePixels> size, byte[] pixels, int colors)
        {
            using var output = new MemoryStream();
            IndexedPng.Write(output, size, pixels, SnesGraphics.DiagnosticPalette(colors));
            return output.ToArray();
        }
    }
}

using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using SuperMetroid.Core;
using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;

using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;
namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>Exports the Ceres approach's consumed Mode-7 and OBJ visual streams.</summary>
internal static class CeresFlightArtworkExtractor
{
    /// <summary>Exports Ceres flight's Mode-7 tiles/maps, OBJ sheets and frames, palette, and actor placements.</summary>
    public static IReadOnlyDictionary<string, byte[]> Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        byte[] characters = Read(CeresCinematicSourceDefinitions.FlightFloorCharacters,
            CeresFlightArtworkFileFormat.FloorByteCount, "Mode-7 characters");
        byte[] objectCharacters = Read(CeresCinematicSourceDefinitions.FlightObjectCharacters,
            CeresFlightArtworkFileFormat.ObjectByteCount, "OBJ characters");
        byte[] decompressedMap = RomDataReader.Decompress(CartridgeImportSource.Require(bus), CeresCinematicSourceDefinitions.CeresTilemaps,
            maximumOutputBytes: 4096);
        if (decompressedMap.Length < CeresFlightArtworkFileFormat.MapCellCount)
            throw new InvalidDataException("Ceres flight lacks its front and rear Mode-7 map slices.");
        byte[] map = decompressedMap.AsSpan(0, CeresFlightArtworkFileFormat.MapCellCount).ToArray();

        byte[] mode7Pixels = SnesGraphics.DecodeMode7Tiles(characters, 16,
            out Extent16<ImagePixels> mode7Size);
        byte[] objPixels = SnesGraphics.DecodePlanarTiles(objectCharacters, 4, 32,
            out Extent16<ImagePixels> objectSize);
        if (mode7Size != new Extent16<ImagePixels>(CeresFlightArtworkFileFormat.FloorWidth, CeresFlightArtworkFileFormat.FloorHeight) ||
            objectSize != new Extent16<ImagePixels>(CeresFlightArtworkFileFormat.ObjectWidth, CeresFlightArtworkFileFormat.ObjectHeight))
            throw new InvalidDataException("Ceres flight character streams have unexpected dimensions.");

        byte[] floorPng = Png(mode7Size, mode7Pixels, 256);
        byte[] objectPng = Png(objectSize, objPixels, 16);
        using var mapJson = new MemoryStream();
        CeresFlightCharacters.WriteMap(mapJson, new CeresFlightMapDocument
        {
            Version = CeresFlightArtworkFileFormat.Version,
            Width = CeresFlightArtworkFileFormat.MapWidth,
            Height = CeresFlightArtworkFileFormat.MapHeight,
            FrontViewTiles = map.AsSpan(0, CeresFlightArtworkFileFormat.MapCellsPerView)
                .SelectToArray(value => (int)value),
            RearViewTiles = map.AsSpan(CeresFlightArtworkFileFormat.MapCellsPerView,
                CeresFlightArtworkFileFormat.MapCellsPerView)
                .SelectToArray(value => (int)value),
        });
        byte[] mapFile = mapJson.ToArray();
        byte[] nativePalette = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
            CeresCinematicSourceDefinitions.FlightPalette, CgramPalettes.ByteCount);
        var colors = new PaletteRgb5[CgramPalettes.ColorCount];
        for (int index = 0; index < colors.Length; index++)
        {
            int word = BinaryPrimitives.ReadUInt16LittleEndian(
                nativePalette.AsSpan(index * sizeof(ushort)));
            if (!RgbColor.IsColorWord(checked((ushort)word)))
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
        CeresFlightPaletteFile.Write(paletteJson, new CeresFlightPaletteDocument
        {
            Version = CeresFlightPaletteFormat.Version,
            Colors = colors,
        });
        byte[] paletteFile = paletteJson.ToArray();
        var spriteFrames = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal);
        foreach (CinematicSpriteFrameDefinition definition in CeresFlightSpriteDefinitions.Frames)
            spriteFrames.Add(definition.Name, IntroCinematicSpriteFrameExtractor.Extract(
                bus, CinematicSpritemapSources.Of(definition.Pose), definition.StockPartCount, definition.Name));
        using var spritesJson = new MemoryStream();
        CeresFlightSpritePresentation.Write(spritesJson, new CeresFlightSpriteDocument
        {
            Version = CeresFlightSpriteFormat.Version,
            Frames = spriteFrames,
        });
        byte[] spritesFile = spritesJson.ToArray();
        // These positions are presentation operands in the five native initializers.
        // Keep their X/Y words editable while retaining each actor's motion in code.
        CeresFlightActorPlacement[] placements =
        [
            .. CeresCinematicSourceDefinitions.RearViewPlacements.Select(source => new CeresFlightActorPlacement
            {
                Role = source.Role,
                X = CoordinateAxis.AsSigned(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                    CeresCinematicSourceDefinitions.PlacementBank | source.FirstOperand)),
                Y = CoordinateAxis.AsSigned(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                    CeresCinematicSourceDefinitions.PlacementBank | source.SecondOperand)),
            }),
        ];
        using var actorsJson = new MemoryStream();
        CeresFlightActorLayoutFile.Write(actorsJson, new CeresFlightActorLayoutDocument
        {
            Version = CeresFlightActorLayoutFormat.Version,
            Actors = placements,
        });
        byte[] actorsFile = actorsJson.ToArray();
        return new Dictionary<string, byte[]>(StringComparer.Ordinal)
        {
            [CeresFlightArtworkFileFormat.FloorFileName] = floorPng,
            [CeresFlightArtworkFileFormat.MapFileName] = mapFile,
            [CeresFlightArtworkFileFormat.ObjectFileName] = objectPng,
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

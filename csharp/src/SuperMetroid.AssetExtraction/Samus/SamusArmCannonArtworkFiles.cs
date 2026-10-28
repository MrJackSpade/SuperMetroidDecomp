using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Exports the native cannon-cover pose/OAM selectors and twelve character tiles.</summary>
public static class SamusArmCannonArtworkFiles
{
    /// <summary>Companion file recording cartridge provenance and hashes of the stock placement and tile assets.</summary>
    private const string ManifestFileName = "samus-arm-cannon-manifest.json";
    /// <summary>Strict camel-case JSON options shared by the arm-cannon manifest reader and writer.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectRequiredConstructorParameters = true,
        WriteIndented = true,
    };

    /// <summary>Exports 253 pose selectors, bounded bank-$90 drawing descriptors, ten direction styles/tile lists, and twelve cannon-cover characters, then verifies planar PNG round-trip.</summary>
    /// <param name="bus">Non-null import-capable cartridge source for cover placement/OBJ attributes and bank-$9A character pixels.</param>
    /// <param name="directory">Artwork directory, created if absent; existing placement JSON, 96-by-8 indexed PNG, and companion manifest are overwritten.</param>
    /// <param name="sourceCartridgeSha256">Caller-supplied provenance hash recorded verbatim; the post-write load requires the supported cartridge identity.</param>
    /// <remarks>The sixteen-color PNG palette is diagnostic. Writes are separate and not rolled back if provenance, placement, or round-trip validation later fails.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory is blank or the address space lacks cartridge import access.</exception>
    /// <exception cref="InvalidDataException">The generated artwork, provenance, or decoded transfer bytes fail validation.</exception>
    /// <exception cref="IOException">Filesystem output or post-write input fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        Directory.CreateDirectory(directory);
        int[] descriptors = Enumerable.Range(0, SamusBodyArtworkCatalog.PoseCount)
            .Select(pose => RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                SamusSourceDefinitions.PoseDrawingDataPointers.Value + pose * 2) - SamusSourceDefinitions.ArmCannonDrawingData).ToArray();
        int[] drawingData = Enumerable.Range(0, SamusArmCannonArtworkFormat.DrawingDataByteCount)
            .Select(index => (int)bus.ReadCartridgeByte(SamusSourceDefinitions.MovementBank.Value |
                (SamusSourceDefinitions.ArmCannonDrawingData + index))).ToArray();
        SpriteVisualPart[] appearances = Enumerable.Range(0, SamusDrawingDefinitions.ArmCannon.DirectionCount)
            .Select(direction => SpriteVisualPart.FromSpritePart(new SnesObjAttributeWord(RomDataReader.ReadWordFixedBank(
                CartridgeImportSource.Require(bus), SamusSourceDefinitions.ArmCannonSpriteAttributes.Value + direction * 2)).ToSinglePart(),
                SpriteArtwork.CharactersPerRow)).ToArray();
        int[][] coverTiles = Enumerable.Range(0, SamusDrawingDefinitions.ArmCannon.DirectionCount)
            .Select(direction =>
            {
                int list = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                    SamusSourceDefinitions.TileListPointers.Value + direction * 2);
                ushort[] sources = [.. Enumerable.Range(0, SamusArmCannonArtworkFormat.FramesPerDirection)
                    .Select(frame => RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                        SamusSourceDefinitions.MovementBank.Value | (list + frame * 2)))];
                // The closed frame uploads nothing; each opening frame names one installed cover tile.
                if (sources[0] != 0)
                    throw new InvalidDataException($"Arm-cannon direction {direction} uploads a closed-cover tile.");
                return sources[1..].Select(source => ArmCannonTileSources.CoverTileDmaSources.IndexOf(source) is int tile and >= 0
                    ? tile
                    : throw new InvalidDataException($"Arm-cannon direction {direction} names cover source ${source:X4}, which is no installed tile.")).ToArray();
            }).ToArray();
        var document = new SamusArmCannonArtworkDocument
        {
            Version = SamusArmCannonArtworkFormat.Version,
            PoseDescriptors = descriptors,
            DrawingData = drawingData,
            CoverAppearances = appearances,
            CoverTiles = coverTiles,
        };
        byte[] planar = new byte[SamusArmCannonArtworkFormat.CoverTileCount *
            SamusDrawingDefinitions.ArmCannon.TileByteCount];
        for (int tile = 0; tile < SamusArmCannonArtworkFormat.CoverTileCount; tile++)
            for (int offset = 0; offset < SamusDrawingDefinitions.ArmCannon.TileByteCount; offset++)
                planar[tile * SamusDrawingDefinitions.ArmCannon.TileByteCount + offset] =
                    bus.ReadCartridgeByte(SamusSourceDefinitions.CharacterDataBank.Value |
                        (ArmCannonTileSources.CoverTileDmaSources[tile] + offset));
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4,
            SamusArmCannonArtworkFormat.CoverTileCount,
            out Extent16<ImagePixels> size);
        using var pngStream = new MemoryStream();
        IndexedPng.Write(pngStream, size, pixels, SnesGraphics.DiagnosticPalette(16));
        byte[] png = pngStream.ToArray();
        byte[] json = SamusArmCannonArtworkCatalog.Write(document);
        File.WriteAllBytes(Path.Combine(directory, SamusArmCannonArtworkFormat.TileFileName), png);
        File.WriteAllBytes(Path.Combine(directory, SamusArmCannonArtworkFormat.JsonFileName), json);
        File.WriteAllBytes(Path.Combine(directory, ManifestFileName),
            JsonSerializer.SerializeToUtf8Bytes(new Manifest(
                SamusArmCannonArtworkFormat.Version, sourceCartridgeSha256,
                Convert.ToHexString(SHA256.HashData(json)),
                Convert.ToHexString(SHA256.HashData(png))), JsonOptions));
        SamusArmCannonArtworkCatalog roundTrip = Load(directory, null);
        for (int tile = 0; tile < SamusArmCannonArtworkFormat.CoverTileCount; tile++)
        {
            if (!roundTrip.TryResolveTile(SamusSourceDefinitions.CharacterDataBank.Value |
                    ArmCannonTileSources.CoverTileDmaSources[tile],
                    SamusDrawingDefinitions.ArmCannon.TileByteCount, out var decoded) ||
                !decoded.Span.SequenceEqual(planar.AsSpan(
                    tile * SamusDrawingDefinitions.ArmCannon.TileByteCount,
                    SamusDrawingDefinitions.ArmCannon.TileByteCount)))
                throw new InvalidDataException($"Arm-cannon tile {tile} changed in PNG round-trip.");
        }
    }

    /// <summary>Validates stock cannon-cover placement and pixels, then independently selects optional replacement JSON and PNG files.</summary>
    /// <param name="stockDirectory">Artwork directory containing the required placement JSON, indexed PNG, and pinned-cartridge manifest.</param>
    /// <param name="overrideDirectory">Optional artwork directory; each missing replacement file independently falls back to validated stock.</param>
    /// <returns>A ROM-independent catalog of owned cover placement/attribute selectors and encoded character pixels.</returns>
    /// <remarks>Stock provenance, both byte hashes, and production codec admission are mandatory even with overrides. Overrides need no manifest; strict placement JSON retains bounded pose/descriptor/direction/source domains, while the PNG supplies twelve four-bit characters.</remarks>
    /// <exception cref="InvalidDataException">Manifest compatibility, stock hashes, strict JSON, selectors, or indexed artwork are invalid; admission errors identify the offending filename.</exception>
    /// <exception cref="IOException">A required stock or selected override file cannot be read.</exception>
    public static SamusArmCannonArtworkCatalog Load(string stockDirectory, string? overrideDirectory)
    {
        SamusArtworkFile manifestFile = SamusArtworkFile.Read(Path.Combine(stockDirectory, ManifestFileName));
        Manifest manifest = manifestFile.Json<Manifest>(JsonOptions);
        return manifestFile.WithContext(() => LoadArtwork());

        SamusArmCannonArtworkCatalog LoadArtwork()
        {
            if (manifest.Version != SamusArmCannonArtworkFormat.Version ||
                !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Arm-cannon manifest does not match the pinned cartridge.");
            SamusArtworkFile stockJson = SamusArtworkFile.Stock(Path.Combine(stockDirectory,
                SamusArmCannonArtworkFormat.JsonFileName), manifest.JsonSha256);
            SamusArtworkFile stockPng = SamusArtworkFile.Stock(Path.Combine(stockDirectory,
                SamusArmCannonArtworkFormat.TileFileName), manifest.PngSha256);
            _ = Compile(stockJson, stockPng);
            return Compile(stockJson.Select(overrideDirectory), stockPng.Select(overrideDirectory));
        }

        static SamusArmCannonArtworkCatalog Compile(SamusArtworkFile json, SamusArtworkFile png)
        {
            var placement = json.Compile(SamusArmCannonArtworkCatalog.LoadPlacement);
            var tiles = png.Compile(stream => RoomCharacterAtlasPng.Read(stream,
                SamusArmCannonArtworkFormat.CoverTileCount *
                    SamusDrawingDefinitions.ArmCannon.TileByteCount));
            return SamusArmCannonArtworkCatalog.FromPlacement(placement, tiles);
        }
    }

    /// <summary>Expected schema, cartridge revision, placement JSON hash, and indexed PNG hash for stock art.</summary>
    private sealed record Manifest(int Version, string SourceCartridgeSha256,
        string JsonSha256, string PngSha256);
}

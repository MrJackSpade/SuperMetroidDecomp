using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Poses;
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

    /// <summary>Exports each pose's selector by the pose's name, bounded bank-$90 drawing descriptors, ten direction styles/tile lists, and twelve cannon-cover characters.</summary>
    /// <param name="bus">Non-null import-capable cartridge source for cover placement/OBJ attributes and bank-$9A character pixels.</param>
    /// <param name="directory">Artwork directory, created if absent; existing placement JSON, 96-by-8 indexed PNG, and companion manifest are overwritten.</param>
    /// <param name="sourceCartridgeSha256">Caller-supplied provenance hash recorded verbatim.</param>
    /// <remarks>The sixteen-color PNG palette is diagnostic. Writes are separate and not rolled back if a later write fails.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory is blank or the address space lacks cartridge import access.</exception>
    /// <exception cref="InvalidDataException">A direction's cartridge tile list uploads a closed-cover tile or names a source that is no installed cover tile.</exception>
    /// <exception cref="IOException">Filesystem output fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        Directory.CreateDirectory(directory);
        // The pose table is read by the cartridge's pose numbers and installed by pose name (#1595).
        Dictionary<SamusPoseId, int> descriptors = SamusPoseNumbers.RecordedPoses.ToDictionary(pose => pose,
            pose => RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                SamusSourceDefinitions.PoseDrawingDataPointers.Value + SamusPoseNumbers.Number(pose) * 2) - SamusSourceDefinitions.ArmCannonDrawingData);
        int[] drawingData = Enumerable.Range(0, SamusArmCannonArtworkFormat.DrawingDataByteCount)
            .Select(index => (int)bus.ReadCartridgeByte(SamusSourceDefinitions.MovementBank.Value |
                (SamusSourceDefinitions.ArmCannonDrawingData + index))).ToArray();
        SpriteVisualPart[] appearances = Enumerable.Range(0, SamusProjectileDirectionCodec.Count)
            .Select(direction => SpriteVisualPart.FromSpritePart(new SnesObjAttributeWord(RomDataReader.ReadWordFixedBank(
                CartridgeImportSource.Require(bus), SamusSourceDefinitions.ArmCannonSpriteAttributes.Value + direction * 2)).ToSinglePart(),
                SpriteArtwork.CharactersPerRow)).ToArray();
        int[][] coverTiles = Enumerable.Range(0, SamusProjectileDirectionCodec.Count)
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
            SamusArmCannonArtworkFormat.CoverTileByteCount];
        for (int tile = 0; tile < SamusArmCannonArtworkFormat.CoverTileCount; tile++)
            for (int offset = 0; offset < SamusArmCannonArtworkFormat.CoverTileByteCount; offset++)
                planar[tile * SamusArmCannonArtworkFormat.CoverTileByteCount + offset] =
                    bus.ReadCartridgeByte(SamusSourceDefinitions.CharacterDataBank.Value |
                        (ArmCannonTileSources.CoverTileDmaSources[tile] + offset));
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4,
            SamusArmCannonArtworkFormat.CoverTileCount,
            out Extent16<ImagePixels> size);
        using var pngStream = new MemoryStream();
        IndexedPng.Write(pngStream, size, pixels, SnesGraphics.DiagnosticPalette(16));
        byte[] png = pngStream.ToArray();
        byte[] json = SamusArmCannonSourceArtwork.Write(document);
        File.WriteAllBytes(Path.Combine(directory, SamusArmCannonArtworkFormat.TileFileName), png);
        File.WriteAllBytes(Path.Combine(directory, SamusArmCannonArtworkFormat.JsonFileName), json);
        File.WriteAllBytes(Path.Combine(directory, ManifestFileName),
            JsonSerializer.SerializeToUtf8Bytes(new Manifest(
                SamusArmCannonArtworkFormat.Version, sourceCartridgeSha256), JsonOptions));
    }

    /// <summary>Validates the manifest's provenance and loads the installed cannon-cover placement and pixels, as the player may have edited them.</summary>
    /// <param name="directory">Artwork directory containing the required placement JSON, indexed PNG, and pinned-cartridge manifest.</param>
    /// <returns>A ROM-independent catalog of owned cover placement/attribute selectors and encoded character pixels.</returns>
    /// <remarks>Strict placement JSON retains bounded pose/descriptor/direction/source domains, while the PNG supplies twelve four-bit characters.</remarks>
    /// <exception cref="InvalidDataException">Manifest compatibility, strict JSON, selectors, or indexed artwork are invalid; admission errors identify the offending filename.</exception>
    /// <exception cref="IOException">A required installed file cannot be read.</exception>
    public static SamusArmCannonSourceArtwork Load(string directory)
    {
        SamusArtworkFile manifestFile = SamusArtworkFile.Read(Path.Combine(directory, ManifestFileName));
        Manifest manifest = manifestFile.Json<Manifest>(JsonOptions);
        return manifestFile.WithContext(() => LoadArtwork());

        SamusArmCannonSourceArtwork LoadArtwork()
        {
            if (manifest.Version != SamusArmCannonArtworkFormat.Version ||
                !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Arm-cannon manifest does not match the pinned cartridge.");
            return Compile(SamusArtworkFile.Read(Path.Combine(directory, SamusArmCannonArtworkFormat.JsonFileName)),
                SamusArtworkFile.Read(Path.Combine(directory, SamusArmCannonArtworkFormat.TileFileName)));
        }

        static SamusArmCannonSourceArtwork Compile(SamusArtworkFile json, SamusArtworkFile png)
        {
            var placement = json.Compile(SamusArmCannonSourceArtwork.LoadPlacement);
            var tiles = png.Compile(stream => RoomCharacterAtlasPng.Read(stream,
                SamusArmCannonArtworkFormat.CoverTileCount *
                    SamusArmCannonArtworkFormat.CoverTileByteCount));
            return SamusArmCannonSourceArtwork.FromPlacement(placement, tiles);
        }
    }

    /// <summary>Expected schema, cartridge revision, placement JSON hash, and indexed PNG hash for stock art.</summary>
    private sealed record Manifest(int Version, string SourceCartridgeSha256);
}

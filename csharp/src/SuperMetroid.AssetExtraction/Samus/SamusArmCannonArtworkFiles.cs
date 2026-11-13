using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Exports the native cannon-cover pose/OAM selectors and twelve character tiles: the installation holds the layout the
/// game binds, and the extracted document and the cover page are import data (#1636, #1638).
/// </summary>
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
    /// <param name="directory">Artwork directory, created if absent; existing layout JSON and companion manifest are overwritten.</param>
    /// <param name="importData">The import-data directory receiving the extracted document (#1638) and the 96-by-8 indexed cover page (#1636), which only the import reads.</param>
    /// <param name="sourceCartridgeSha256">Caller-supplied provenance hash recorded verbatim.</param>
    /// <remarks>The sixteen-color PNG palette is diagnostic. Writes are separate and not rolled back if a later write fails.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory is blank or the address space lacks cartridge import access.</exception>
    /// <exception cref="InvalidDataException">A direction's cartridge tile list uploads a closed-cover tile or names a source that is no installed cover tile.</exception>
    /// <exception cref="IOException">Filesystem output fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory, string importData, string sourceCartridgeSha256)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(importData);
        Directory.CreateDirectory(directory);
        Directory.CreateDirectory(importData);
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
        // The game binds the layout alone; the appearances and tile selections stay with the import (#1638).
        byte[] layout = SamusArmCannonLayout.Write(new SamusArmCannonLayoutDocument
        {
            Version = SamusArmCannonArtworkFormat.Version,
            PoseDescriptors = descriptors,
            DrawingData = drawingData,
        });
        File.WriteAllBytes(Path.Combine(importData, SamusArmCannonArtworkFormat.TileFileName), png);
        File.WriteAllBytes(Path.Combine(importData, SamusArmCannonArtworkFormat.ArtworkFileName), json);
        File.WriteAllBytes(Path.Combine(directory, SamusArmCannonArtworkFormat.JsonFileName), layout);
        File.WriteAllBytes(Path.Combine(directory, ManifestFileName),
            JsonSerializer.SerializeToUtf8Bytes(new Manifest(
                SamusArmCannonArtworkFormat.Version, sourceCartridgeSha256), JsonOptions));
    }

    /// <summary>
    /// Validates the manifest's provenance and loads what the game binds of the installed arm cannon, as the player may have
    /// edited it: each pose's drawing descriptor and the drawing window (#1638). No appearance or character page is read.
    /// </summary>
    /// <param name="directory">Artwork directory containing the required layout JSON and pinned-cartridge manifest.</param>
    /// <exception cref="InvalidDataException">Manifest compatibility, strict JSON or the tables are invalid; admission errors identify the offending filename.</exception>
    /// <exception cref="IOException">A required installed file cannot be read.</exception>
    public static SamusArmCannonLayout Load(string directory)
    {
        RequireManifest(directory);
        return SamusArtworkFile.Read(Path.Combine(directory, SamusArmCannonArtworkFormat.JsonFileName)).Compile(SamusArmCannonLayout.Read);
    }

    /// <summary>
    /// Loads the arm cannon as the import reads it (#1638): the extracted document, with its cover appearances and tile
    /// selections, from the import data <paramref name="importData"/>, under the manifest in <paramref name="directory"/>.
    /// </summary>
    /// <param name="directory">Artwork directory containing the pinned-cartridge manifest.</param>
    /// <param name="importData">The component's import-data directory, holding the extracted document.</param>
    /// <exception cref="InvalidDataException">Manifest compatibility, strict JSON, selectors or appearances are invalid; admission errors identify the offending filename.</exception>
    /// <exception cref="IOException">A required file cannot be read.</exception>
    public static SamusArmCannonSourceArtwork LoadForImport(string directory, string importData)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(importData);
        RequireManifest(directory);
        return SamusArmCannonSourceArtwork.FromPlacement(SamusArtworkFile.Read(Path.Combine(importData, SamusArmCannonArtworkFormat.ArtworkFileName))
            .Compile(SamusArmCannonSourceArtwork.LoadPlacement));
    }

    /// <summary>Requires the manifest in <paramref name="directory"/> to name this format and the pinned cartridge.</summary>
    /// <exception cref="InvalidDataException">The manifest names another format or source; the error names its file.</exception>
    private static void RequireManifest(string directory)
    {
        SamusArtworkFile manifestFile = SamusArtworkFile.Read(Path.Combine(directory, ManifestFileName));
        Manifest manifest = manifestFile.Json<Manifest>(JsonOptions);
        manifestFile.WithContext(() =>
        {
            if (manifest.Version != SamusArmCannonArtworkFormat.Version ||
                !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Arm-cannon manifest does not match the pinned cartridge.");
            return manifest;
        });
    }

    /// <summary>Loads the cover characters of <paramref name="cannon"/> from the import's page in <paramref name="importData"/> (#1636).</summary>
    /// <param name="cannon">The arm cannon as the import reads it, whose tile selections name the characters.</param>
    /// <param name="importData">The component's import-data directory, holding the 96-by-8 indexed cover page.</param>
    /// <exception cref="InvalidDataException">The page is invalid; the error names its file.</exception>
    public static SamusArmCannonCoverCharacters LoadCoverCharacters(SamusArmCannonSourceArtwork cannon, string importData)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(importData);
        return new SamusArmCannonCoverCharacters(cannon, SamusArtworkFile.Read(Path.Combine(importData, SamusArmCannonArtworkFormat.TileFileName))
            .Compile(stream => RoomCharacterAtlasPng.Read(stream,
                SamusArmCannonArtworkFormat.CoverTileCount * SamusArmCannonArtworkFormat.CoverTileByteCount)));
    }

    /// <summary>Expected schema, cartridge revision, placement JSON hash, and indexed PNG hash for stock art.</summary>
    private sealed record Manifest(int Version, string SourceCartridgeSha256);
}

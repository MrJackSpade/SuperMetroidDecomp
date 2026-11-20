using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using SuperMetroid.Core;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// Extracts the ending/reward character pages (#1636) and the maps and sprite compositions, whose entries are the original's
/// packed tile and sprite placements (#1638), and their manifest (#1693), into the component's import data, which only the
/// import reads; the installation holds only the pictures composed from them.
/// </summary>
public static class EndingObjectArtworkFiles
{
    /// <summary>Creates ending/reward PNG transports, BG2 map JSON, sprite-composition JSON, and a hashed manifest.</summary>
    /// <param name="bus">Cartridge import source for compressed character/map streams and native cloud, explosion, text, reward, and logo spritemaps.</param>
    /// <param name="directory">The component's directory; every file goes to its import-data directory, created if absent, and must not already exist there.</param>
    /// <param name="sourceCartridgeSha256">Source identity recorded in the manifest; installation loading requires the supported cartridge identity.</param>
    /// <exception cref="InvalidDataException">A native source is too short or a sprite composition is invalid.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        string importData = SuperMetroid.AssetExtraction.Installation.ImportDataDirectory.Of(directory);
        Directory.CreateDirectory(importData);
        var hashes = new Dictionary<string, string>(StringComparer.Ordinal);
        Export(EndingObjectArtworkFormat.CloudFileName,
            EndingSourceDefinitions.Artwork.EscapeCloudCharacters,
            EndingObjectArtworkFormat.CloudByteCount);
        Export(EndingObjectArtworkFormat.ExplosionFileName,
            EndingSourceDefinitions.Artwork.ExplosionObjectCharacters,
            EndingObjectArtworkFormat.ExplosionByteCount);
        foreach (EndingExplosionPicture picture in Ensure.EveryMemberOnce(EndingObjectArtworkFormat.ExplosionPictures))
            ExportMap(EndingObjectArtworkFormat.FragmentFileName(Ensure.Listed(EndingObjectArtworkFormat.FragmentNumbers, picture)),
                EndingArtworkSources.ObjectFragment(picture), EndingObjectArtworkFormat.FragmentByteCount);
        Export(EndingObjectArtworkFormat.WaitingSamusFileName,
            EndingSourceDefinitions.Artwork.WaitingForCreditsCharacters,
            EndingObjectArtworkFormat.RewardByteCount);
        Export(EndingObjectArtworkFormat.ShootingScreenFileName,
            EndingSourceDefinitions.Artwork.ShootingScreenCharacters,
            EndingObjectArtworkFormat.RewardByteCount);
        Export(EndingObjectArtworkFormat.SuitlessSamusFileName,
            EndingSourceDefinitions.Artwork.SuitlessSamusCharacters,
            EndingObjectArtworkFormat.RewardByteCount);
        ExportMap(EndingObjectArtworkFormat.WaitingTilemapFileName,
            EndingSourceDefinitions.Artwork.WaitingForCreditsTilemap,
            EndingObjectArtworkFormat.WaitingTilemapByteCount);
        Export(EndingObjectArtworkFormat.PostCreditsFragmentAFileName,
            EndingSourceDefinitions.Artwork.TransformationCharacters,
            EndingObjectArtworkFormat.PostCreditsFragmentAByteCount);
        ExportMap(EndingObjectArtworkFormat.PostCreditsFragmentBFileName,
            EndingSourceDefinitions.Artwork.TransformationTilemap,
            EndingObjectArtworkFormat.PostCreditsFragmentBByteCount);
        Export(EndingObjectArtworkFormat.PostShotLogoTileFileName,
            EndingSourceDefinitions.Artwork.PostShotLogoTiles,
            EndingObjectArtworkFormat.PostShotLogoTileByteCount);
        ExportMap(EndingObjectArtworkFormat.PostShotLogoMapFileName,
            EndingSourceDefinitions.Artwork.PostShotLogoMap,
            EndingObjectArtworkFormat.PostShotLogoMapByteCount);

        // Each sprite family's frames are its definitions' spritemaps; the completion text's typed steps come first, then the
        // digits and the colon, as the editable file lists them.
        WriteSprites(EndingCloudSpriteFormat.FileName, stream => EndingCloudSpritePresentation.Write(stream,
            new EndingCloudSpriteDocument { Version = EndingCloudSpriteFormat.Version, Frames = Frames(EndingCloudSpriteDefinitions.Frames) }));
        WriteSprites(EndingExplosionSpriteFormat.FileName, stream => EndingExplosionSpritePresentation.Write(stream,
            new EndingExplosionSpriteDocument { Version = EndingExplosionSpriteFormat.Version, Frames = Frames(EndingExplosionSpriteDefinitions.Frames) }));
        WriteSprites(EndingCompletionTextSpriteFormat.FileName, stream => EndingCompletionTextSpritePresentation.Write(stream,
            new EndingCompletionTextSpriteDocument
            {
                Version = EndingCompletionTextSpriteFormat.Version,
                Frames = TypedFrames(EndingCompletionTextSpriteDefinitions.Typings, EndingCompletionTextSpriteDefinitions.Frames),
            }));
        WriteSprites(EndingRewardSpriteFormat.FileName, stream => EndingRewardSpritePresentation.Write(stream,
            new EndingRewardSpriteDocument { Version = EndingRewardSpriteFormat.Version, Frames = Frames(EndingRewardSpriteDefinitions.Frames) }));
        WriteSprites(EndingLogoSpriteFormat.FileName, stream => EndingLogoSpritePresentation.Write(stream,
            new EndingLogoSpriteDocument { Version = EndingLogoSpriteFormat.Version, Frames = Frames(EndingLogoSpriteDefinitions.Frames) }));

        using var manifest = new FileStream(Path.Combine(importData,
            EndingObjectArtworkFormat.ManifestFileName), FileMode.CreateNew,
            FileAccess.Write);
        JsonSerializer.Serialize(manifest,
            new Manifest(EndingObjectArtworkFormat.ManifestVersion,
                sourceCartridgeSha256, hashes), JsonOptions);

        void Export(string name, int source, int count)
        {
            byte[] decompressed = RomDataReader.Decompress(CartridgeImportSource.Require(bus), source,
                EndingSourceDefinitions.Artwork.DecompressionLimit);
            if (decompressed.Length < count)
                throw new InvalidDataException($"Ending OBJ source ${source:X6} is shorter than {name}'s native DMA.");
            byte[] native = decompressed.AsSpan(0, count).ToArray();
            byte[] pixels = SnesGraphics.DecodePlanarTiles(native, 4,
                IntroCinematicArtworkFormat.TileColumns, out Extent16<ImagePixels> size);
            using var png = new MemoryStream();
            IndexedPng.Write(png, size, pixels, SnesGraphics.DiagnosticPalette(16));
            WriteFile(name, png.ToArray());
        }

        void ExportMap(string name, int source, int count)
        {
            byte[] decoded = RomDataReader.Decompress(CartridgeImportSource.Require(bus), source,
                EndingSourceDefinitions.Artwork.DecompressionLimit);
            if (decoded.Length < count)
                throw new InvalidDataException($"Ending map source ${source:X6} is shorter than {name}'s native DMA.");
            byte[] native = decoded.AsSpan(0, count).ToArray();
            WriteFile(name, RoomBackgroundTilemapExtractor.Encode(native));
        }

        // Each frame of a sprite family, in order, from the spritemap its definition's pose names.
        Dictionary<string, SpriteVisualPart[]> Frames(IEnumerable<CinematicSpriteFrameDefinition> definitions) =>
            TypedFrames([], definitions);

        // Each typed step's frame, then each other frame of the family, in order, from the spritemaps they name.
        Dictionary<string, SpriteVisualPart[]> TypedFrames(IEnumerable<EndingTypedStepDefinition> typings,
            IEnumerable<CinematicSpriteFrameDefinition> definitions)
        {
            var frames = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal);
            foreach (EndingTypedStepDefinition step in typings)
                frames.Add(step.Name, IntroCinematicSpriteFrameExtractor.Extract(
                    bus, CinematicSpritemapSources.Of(step.Typed), step.StockPartCount, step.Name));
            foreach (CinematicSpriteFrameDefinition definition in definitions)
                frames.Add(definition.Name, IntroCinematicSpriteFrameExtractor.Extract(
                    bus, CinematicSpritemapSources.Of(definition.Pose), definition.StockPartCount, definition.Name));
            return frames;
        }

        // A sprite family's document as <paramref name="write"/> serializes it.
        void WriteSprites(string name, Action<Stream> write)
        {
            using var sprites = new MemoryStream();
            write(sprites);
            WriteFile(name, sprites.ToArray());
        }

        // Writes a new import-data file and records its digest.
        void WriteFile(string name, byte[] file)
        {
            using (var output = new FileStream(Path.Combine(importData, name), FileMode.CreateNew, FileAccess.Write))
                output.Write(file);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(file)));
        }
    }

    /// <summary>
    /// Checks the manifest's provenance and coverage, then compiles the ending's maps and sprite compositions with its
    /// object characters from the component's import data <paramref name="importData"/> (#1636, #1693).
    /// </summary>
    /// <param name="importData">The component's import-data directory, holding the manifest, the character pages, maps and sprite JSON.</param>
    /// <exception cref="InvalidDataException">Manifest provenance or coverage, or a page, map or sprite composition is invalid.</exception>
    public static ImportEndingObjectArtwork LoadForImport(string importData)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(importData);
        EndingObjectArtworkParts parts = ReadParts(importData);
        return new ImportEndingObjectArtwork(parts, new EndingObjectCharacters(
            LoadSheet(EndingObjectArtworkFormat.CloudFileName, EndingObjectArtworkFormat.CloudByteCount),
            LoadSheet(EndingObjectArtworkFormat.ExplosionFileName, EndingObjectArtworkFormat.ExplosionByteCount),
            LoadSheet(EndingObjectArtworkFormat.WaitingSamusFileName, EndingObjectArtworkFormat.RewardByteCount),
            LoadSheet(EndingObjectArtworkFormat.ShootingScreenFileName, EndingObjectArtworkFormat.RewardByteCount),
            LoadSheet(EndingObjectArtworkFormat.SuitlessSamusFileName, EndingObjectArtworkFormat.RewardByteCount),
            LoadSheet(EndingObjectArtworkFormat.PostCreditsFragmentAFileName, EndingObjectArtworkFormat.PostCreditsFragmentAByteCount),
            LoadSheet(EndingObjectArtworkFormat.PostShotLogoTileFileName, EndingObjectArtworkFormat.PostShotLogoTileByteCount)));

        RoomCharacterAtlas LoadSheet(string name, int expectedBytes)
        {
            string path = Path.Combine(importData, name);
            try
            {
                return RoomCharacterAtlasPng.Read(new MemoryStream(File.ReadAllBytes(path), writable: false), expectedBytes);
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid ending OBJ page {path}: {error.Message}", error);
            }
        }
    }

    /// <summary>
    /// Checks the manifest's provenance and coverage, then reads the maps and sprite compositions, all from the import data
    /// <paramref name="importData"/> (#1638, #1693).
    /// </summary>
    /// <exception cref="InvalidDataException">Manifest provenance or coverage, or a map or sprite composition is invalid.</exception>
    private static EndingObjectArtworkParts ReadParts(string importData)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(importData);
        string manifestPath = Path.Combine(importData,
            EndingObjectArtworkFormat.ManifestFileName);
        Manifest manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<Manifest>(File.ReadAllBytes(manifestPath),
                JsonOptions) ?? throw new InvalidDataException("Ending OBJ manifest is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid ending OBJ manifest {manifestPath}.", error);
        }
        string[] names =
        [
            EndingObjectArtworkFormat.CloudFileName,
            EndingObjectArtworkFormat.ExplosionFileName,
            .. Enumerable.Range(0, EndingObjectArtworkFormat.FragmentCount)
                .Select(EndingObjectArtworkFormat.FragmentFileName),
            EndingObjectArtworkFormat.WaitingSamusFileName,
            EndingObjectArtworkFormat.ShootingScreenFileName,
            EndingObjectArtworkFormat.SuitlessSamusFileName,
            EndingObjectArtworkFormat.WaitingTilemapFileName,
            EndingObjectArtworkFormat.PostCreditsFragmentAFileName,
            EndingObjectArtworkFormat.PostCreditsFragmentBFileName,
            EndingObjectArtworkFormat.PostShotLogoTileFileName,
            EndingObjectArtworkFormat.PostShotLogoMapFileName,
            EndingCloudSpriteFormat.FileName,
            EndingExplosionSpriteFormat.FileName,
            EndingCompletionTextSpriteFormat.FileName,
            EndingRewardSpriteFormat.FileName,
            EndingLogoSpriteFormat.FileName,
        ];
        if (manifest.Version != EndingObjectArtworkFormat.ManifestVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase) ||
            manifest.StockSha256 is null || manifest.StockSha256.Count != names.Length ||
            names.Any(name => !manifest.StockSha256.ContainsKey(name)))
            throw new InvalidDataException($"Ending OBJ manifest {manifestPath} does not describe this installation.");

        return new EndingObjectArtworkParts { Fragments = Enumerable.Range(0, EndingObjectArtworkFormat.FragmentCount)
                .Select(index => LoadMap(EndingObjectArtworkFormat.FragmentFileName(index),
                    EndingObjectArtworkFormat.FragmentByteCount))
                .ToArray(), WaitingTilemap = LoadMap(EndingObjectArtworkFormat.WaitingTilemapFileName,
                EndingObjectArtworkFormat.WaitingTilemapByteCount), PostCreditsFragmentB = LoadMap(EndingObjectArtworkFormat.PostCreditsFragmentBFileName,
                EndingObjectArtworkFormat.PostCreditsFragmentBByteCount), PostShotLogoMap = LoadMap(EndingObjectArtworkFormat.PostShotLogoMapFileName,
                EndingObjectArtworkFormat.PostShotLogoMapByteCount), CloudSprites = LoadSprites(EndingCloudSpriteFormat.FileName, "cloud", EndingCloudSpritePresentation.Load),
            ExplosionSprites = LoadSprites(EndingExplosionSpriteFormat.FileName, "explosion", EndingExplosionSpritePresentation.Load),
            CompletionTextSprites = LoadSprites(EndingCompletionTextSpriteFormat.FileName, "completion text", EndingCompletionTextSpritePresentation.Load),
            RewardSprites = LoadSprites(EndingRewardSpriteFormat.FileName, "reward", EndingRewardSpritePresentation.Load),
            LogoSprites = LoadSprites(EndingLogoSpriteFormat.FileName, "logo", EndingLogoSpritePresentation.Load) };

        // A sprite family's file, read by the family's presentation and named in a failure as <paramref name="what"/>.
        T LoadSprites<T>(string name, string what, Func<Stream, T> load)
        {
            string installedPath = Path.Combine(importData, name);
            try
            {
                return load(new MemoryStream(File.ReadAllBytes(installedPath), writable: false));
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid ending {what} sprites {installedPath}: {error.Message}", error);
            }
        }

        RoomBackgroundTilemapAtlas LoadMap(string name, int expectedBytes)
        {
            string installedPath = Path.Combine(importData, name);
            try
            {
                return RoomBackgroundTilemapAtlas.Load(new MemoryStream(
                    File.ReadAllBytes(installedPath),
                    writable: false), expectedBytes);
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid ending map {installedPath}: {error.Message}", error);
            }
        }
    }

    /// <summary>Stable manifest serialization options shared by extraction and installed-artwork loading.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    /// <summary>Records schema and source-ROM identity plus SHA-256 digests for every installed stock artwork file.</summary>
    private sealed record Manifest(int Version, string SourceCartridgeSha256,
        Dictionary<string, string> StockSha256);
}

using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using SuperMetroid.Core;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>Extracts ending/reward character PNGs and the waiting-scene BG2 tilemap.</summary>
public static class EndingObjectArtworkFiles
{
    /// <summary>Creates ending/reward PNG transports, BG2 map JSON, sprite-composition JSON, and a hashed manifest, checking character and map transfers round-trip to native bytes.</summary>
    /// <param name="bus">Cartridge import source for compressed character/map streams and native cloud, explosion, text, reward, and logo spritemaps.</param>
    /// <param name="directory">Destination directory, created if absent; artwork files and manifest must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Source identity recorded in the manifest; installation loading requires the supported cartridge identity.</param>
    /// <exception cref="InvalidDataException">A native source is too short, a PNG/map transfer changes its native bytes, or a sprite composition is invalid.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);
        var hashes = new Dictionary<string, string>(StringComparer.Ordinal);
        Export(EndingObjectArtworkFormat.CloudFileName,
            EndingSourceDefinitions.Artwork.EscapeCloudCharacters,
            EndingObjectArtworkFormat.CloudByteCount);
        Export(EndingObjectArtworkFormat.ExplosionFileName,
            EndingSourceDefinitions.Artwork.ExplosionObjectCharacters,
            EndingObjectArtworkFormat.ExplosionByteCount);
        for (int index = 0; index < EndingObjectArtworkFormat.FragmentCount; index++)
            ExportMap(EndingObjectArtworkFormat.FragmentFileName(index),
                EndingArtworkSources.ObjectFragment((EndingObjectFragmentId)index), EndingObjectArtworkFormat.FragmentByteCount);
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

        var cloudFrames = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal);
        foreach (CinematicSpriteFrameDefinition definition in EndingCloudSpriteDefinitions.Frames)
            cloudFrames.Add(definition.Name, IntroCinematicSpriteFrameExtractor.Extract(
                bus, CinematicSpritemapSources.Of(definition.Pose), definition.StockPartCount, definition.Name));
        using (var sprites = new MemoryStream())
        {
            EndingCloudSpritePresentation.Write(sprites, new EndingCloudSpriteDocument
            {
                Version = EndingCloudSpriteFormat.Version,
                Frames = cloudFrames,
            });
            byte[] file = sprites.ToArray();
            string name = EndingCloudSpriteFormat.FileName;
            using (var output = new FileStream(Path.Combine(directory, name),
                       FileMode.CreateNew, FileAccess.Write))
                output.Write(file);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(file)));
        }
        var explosionFrames = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal);
        foreach (CinematicSpriteFrameDefinition definition in
            EndingExplosionSpriteDefinitions.Frames)
            explosionFrames.Add(definition.Name, IntroCinematicSpriteFrameExtractor.Extract(
                bus, CinematicSpritemapSources.Of(definition.Pose), definition.StockPartCount, definition.Name));
        using (var sprites = new MemoryStream())
        {
            EndingExplosionSpritePresentation.Write(sprites,
                new EndingExplosionSpriteDocument
                {
                    Version = EndingExplosionSpriteFormat.Version,
                    Frames = explosionFrames,
                });
            byte[] file = sprites.ToArray();
            string name = EndingExplosionSpriteFormat.FileName;
            using (var output = new FileStream(Path.Combine(directory, name),
                       FileMode.CreateNew, FileAccess.Write))
                output.Write(file);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(file)));
        }
        var completionFrames = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal);
        foreach (CinematicSpriteFrameDefinition definition in
            EndingCompletionTextSpriteDefinitions.Frames)
            completionFrames.Add(definition.Name, IntroCinematicSpriteFrameExtractor.Extract(
                bus, CinematicSpritemapSources.Of(definition.Pose), definition.StockPartCount, definition.Name));
        using (var sprites = new MemoryStream())
        {
            EndingCompletionTextSpritePresentation.Write(sprites,
                new EndingCompletionTextSpriteDocument
                {
                    Version = EndingCompletionTextSpriteFormat.Version,
                    Frames = completionFrames,
                });
            byte[] file = sprites.ToArray();
            string name = EndingCompletionTextSpriteFormat.FileName;
            using (var output = new FileStream(Path.Combine(directory, name),
                       FileMode.CreateNew, FileAccess.Write))
                output.Write(file);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(file)));
        }
        var rewardFrames = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal);
        foreach (CinematicSpriteFrameDefinition definition in EndingRewardSpriteDefinitions.Frames)
            rewardFrames.Add(definition.Name, IntroCinematicSpriteFrameExtractor.Extract(
                bus, CinematicSpritemapSources.Of(definition.Pose), definition.StockPartCount, definition.Name));
        using (var sprites = new MemoryStream())
        {
            EndingRewardSpritePresentation.Write(sprites,
                new EndingRewardSpriteDocument
                {
                    Version = EndingRewardSpriteFormat.Version,
                    Frames = rewardFrames,
                });
            byte[] file = sprites.ToArray();
            string name = EndingRewardSpriteFormat.FileName;
            using (var output = new FileStream(Path.Combine(directory, name),
                       FileMode.CreateNew, FileAccess.Write))
                output.Write(file);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(file)));
        }
        var logoFrames = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal);
        foreach (CinematicSpriteFrameDefinition definition in EndingLogoSpriteDefinitions.Frames)
            logoFrames.Add(definition.Name, IntroCinematicSpriteFrameExtractor.Extract(
                bus, CinematicSpritemapSources.Of(definition.Pose), definition.StockPartCount, definition.Name));
        using (var sprites = new MemoryStream())
        {
            EndingLogoSpritePresentation.Write(sprites,
                new EndingLogoSpriteDocument
                {
                    Version = EndingLogoSpriteFormat.Version,
                    Frames = logoFrames,
                });
            byte[] file = sprites.ToArray();
            string name = EndingLogoSpriteFormat.FileName;
            using (var output = new FileStream(Path.Combine(directory, name),
                       FileMode.CreateNew, FileAccess.Write))
                output.Write(file);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(file)));
        }

        using var manifest = new FileStream(Path.Combine(directory,
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
            byte[] file = png.ToArray();
            RoomCharacterAtlas compiled = RoomCharacterAtlasPng.Read(
                new MemoryStream(file, writable: false), count);
            if (!compiled.Characters.Span.SequenceEqual(native))
                throw new InvalidDataException($"Ending OBJ PNG {name} changed native character bytes.");
            using (var output = new FileStream(Path.Combine(directory, name),
                       FileMode.CreateNew, FileAccess.Write))
                output.Write(file);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(file)));
        }

        void ExportMap(string name, int source, int count)
        {
            byte[] decoded = RomDataReader.Decompress(CartridgeImportSource.Require(bus), source,
                EndingSourceDefinitions.Artwork.DecompressionLimit);
            if (decoded.Length < count)
                throw new InvalidDataException($"Ending map source ${source:X6} is shorter than {name}'s native DMA.");
            byte[] native = decoded.AsSpan(0, count).ToArray();
            byte[] json = RoomBackgroundTilemapExtractor.Encode(native);
            RoomBackgroundTilemapAtlas compiled = RoomBackgroundTilemapAtlas.Load(
                new MemoryStream(json, writable: false), count);
            if (!BackgroundTileWords.ToBytes(compiled.Tiles.Span).SequenceEqual(native))
                throw new InvalidDataException($"Ending map JSON {name} changed native tilemap words.");
            using (var output = new FileStream(Path.Combine(directory, name),
                       FileMode.CreateNew, FileAccess.Write))
                output.Write(json);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(json)));
        }
    }

    /// <summary>Checks the complete stock manifest and file digests, then compiles per-file ending artwork and sprite-composition selections without cartridge access.</summary>
    /// <param name="stockDirectory">Stock directory containing the ending/reward sheets, raw-fragment PNG transports, BG2 maps, sprite JSON, and provenance/digest manifest.</param>
    /// <param name="overrideDirectory">Optional directory with independently selected replacement files; every absent file falls back to verified stock.</param>
    /// <returns>The selected artwork and OAM-composition catalog, retaining fixed transfer sizes while actor instructions, timing, and palette selection remain engine-owned.</returns>
    /// <exception cref="InvalidDataException">Manifest provenance or coverage, a stock digest, or a selected sheet, map, or sprite composition is invalid.</exception>
    public static EndingObjectArtwork Load(string stockDirectory,
        string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory,
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

        return new EndingObjectSourceArtwork(new EndingObjectArtworkParts { Clouds = LoadSheet(EndingObjectArtworkFormat.CloudFileName,
                EndingObjectArtworkFormat.CloudByteCount), Explosion = LoadSheet(EndingObjectArtworkFormat.ExplosionFileName,
                EndingObjectArtworkFormat.ExplosionByteCount), Fragments = Enumerable.Range(0, EndingObjectArtworkFormat.FragmentCount)
                .Select(index => LoadMap(EndingObjectArtworkFormat.FragmentFileName(index),
                    EndingObjectArtworkFormat.FragmentByteCount))
                .ToArray(), WaitingSamus = LoadSheet(EndingObjectArtworkFormat.WaitingSamusFileName,
                EndingObjectArtworkFormat.RewardByteCount), ShootingScreen = LoadSheet(EndingObjectArtworkFormat.ShootingScreenFileName,
                EndingObjectArtworkFormat.RewardByteCount), SuitlessSamus = LoadSheet(EndingObjectArtworkFormat.SuitlessSamusFileName,
                EndingObjectArtworkFormat.RewardByteCount), WaitingTilemap = LoadMap(EndingObjectArtworkFormat.WaitingTilemapFileName,
                EndingObjectArtworkFormat.WaitingTilemapByteCount), PostCreditsFragmentA = LoadSheet(EndingObjectArtworkFormat.PostCreditsFragmentAFileName,
                EndingObjectArtworkFormat.PostCreditsFragmentAByteCount), PostCreditsFragmentB = LoadMap(EndingObjectArtworkFormat.PostCreditsFragmentBFileName,
                EndingObjectArtworkFormat.PostCreditsFragmentBByteCount), PostShotLogoTiles = LoadSheet(EndingObjectArtworkFormat.PostShotLogoTileFileName,
                EndingObjectArtworkFormat.PostShotLogoTileByteCount), PostShotLogoMap = LoadMap(EndingObjectArtworkFormat.PostShotLogoMapFileName,
                EndingObjectArtworkFormat.PostShotLogoMapByteCount), CloudSprites = LoadCloudSprites(), ExplosionSprites = LoadExplosionSprites(), CompletionTextSprites = LoadCompletionTextSprites(), RewardSprites = LoadRewardSprites(), LogoSprites = LoadLogoSprites() });

        EndingCloudSpritePresentation LoadCloudSprites()
        {
            string name = EndingCloudSpriteFormat.FileName;
            string stockPath = Path.Combine(stockDirectory, name);
            byte[] stock = File.ReadAllBytes(stockPath);
            if (!string.Equals(Convert.ToHexString(SHA256.HashData(stock)),
                    manifest.StockSha256[name], StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(
                    $"Stock ending cloud sprites {stockPath} failed its manifest hash.");
            string? overridePath = overrideDirectory is null ? null :
                Path.Combine(overrideDirectory, name);
            string selectedPath = overridePath is not null && File.Exists(overridePath)
                ? overridePath : stockPath;
            try
            {
                return EndingCloudSpritePresentation.Load(new MemoryStream(
                    selectedPath == stockPath ? stock : File.ReadAllBytes(selectedPath),
                    writable: false));
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException(
                    $"Invalid ending cloud sprites {selectedPath}: {error.Message}", error);
            }
        }

        EndingExplosionSpritePresentation LoadExplosionSprites()
        {
            string name = EndingExplosionSpriteFormat.FileName;
            string stockPath = Path.Combine(stockDirectory, name);
            byte[] stock = File.ReadAllBytes(stockPath);
            if (!string.Equals(Convert.ToHexString(SHA256.HashData(stock)),
                    manifest.StockSha256[name], StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(
                    $"Stock ending explosion sprites {stockPath} failed its manifest hash.");
            string? overridePath = overrideDirectory is null ? null :
                Path.Combine(overrideDirectory, name);
            string selectedPath = overridePath is not null && File.Exists(overridePath)
                ? overridePath : stockPath;
            try
            {
                return EndingExplosionSpritePresentation.Load(new MemoryStream(
                    selectedPath == stockPath ? stock : File.ReadAllBytes(selectedPath),
                    writable: false));
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException(
                    $"Invalid ending explosion sprites {selectedPath}: {error.Message}", error);
            }
        }

        EndingCompletionTextSpritePresentation LoadCompletionTextSprites()
        {
            string name = EndingCompletionTextSpriteFormat.FileName;
            string stockPath = Path.Combine(stockDirectory, name);
            byte[] stock = File.ReadAllBytes(stockPath);
            if (!string.Equals(Convert.ToHexString(SHA256.HashData(stock)),
                    manifest.StockSha256[name], StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(
                    $"Stock ending completion text sprites {stockPath} failed its manifest hash.");
            string? overridePath = overrideDirectory is null ? null :
                Path.Combine(overrideDirectory, name);
            string selectedPath = overridePath is not null && File.Exists(overridePath)
                ? overridePath : stockPath;
            try
            {
                return EndingCompletionTextSpritePresentation.Load(new MemoryStream(
                    selectedPath == stockPath ? stock : File.ReadAllBytes(selectedPath),
                    writable: false));
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException(
                    $"Invalid ending completion text sprites {selectedPath}: {error.Message}", error);
            }
        }

        EndingRewardSpritePresentation LoadRewardSprites()
        {
            string name = EndingRewardSpriteFormat.FileName;
            string stockPath = Path.Combine(stockDirectory, name);
            byte[] stock = File.ReadAllBytes(stockPath);
            if (!string.Equals(Convert.ToHexString(SHA256.HashData(stock)),
                    manifest.StockSha256[name], StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(
                    $"Stock ending reward sprites {stockPath} failed its manifest hash.");
            string? overridePath = overrideDirectory is null ? null :
                Path.Combine(overrideDirectory, name);
            string selectedPath = overridePath is not null && File.Exists(overridePath)
                ? overridePath : stockPath;
            try
            {
                return EndingRewardSpritePresentation.Load(new MemoryStream(
                    selectedPath == stockPath ? stock : File.ReadAllBytes(selectedPath),
                    writable: false));
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException(
                    $"Invalid ending reward sprites {selectedPath}: {error.Message}", error);
            }
        }

        EndingLogoSpritePresentation LoadLogoSprites()
        {
            string name = EndingLogoSpriteFormat.FileName;
            string stockPath = Path.Combine(stockDirectory, name);
            byte[] stock = File.ReadAllBytes(stockPath);
            if (!string.Equals(Convert.ToHexString(SHA256.HashData(stock)),
                    manifest.StockSha256[name], StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(
                    $"Stock ending logo sprites {stockPath} failed its manifest hash.");
            string? overridePath = overrideDirectory is null ? null :
                Path.Combine(overrideDirectory, name);
            string selectedPath = overridePath is not null && File.Exists(overridePath)
                ? overridePath : stockPath;
            try
            {
                return EndingLogoSpritePresentation.Load(new MemoryStream(
                    selectedPath == stockPath ? stock : File.ReadAllBytes(selectedPath),
                    writable: false));
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException(
                    $"Invalid ending logo sprites {selectedPath}: {error.Message}", error);
            }
        }

        RoomCharacterAtlas LoadSheet(string name, int expectedBytes)
        {
            string stockPath = Path.Combine(stockDirectory, name);
            byte[] stock = File.ReadAllBytes(stockPath);
            if (!string.Equals(Convert.ToHexString(SHA256.HashData(stock)),
                    manifest.StockSha256[name], StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Stock ending OBJ PNG {stockPath} failed its manifest hash.");
            string? overridePath = overrideDirectory is null ? null :
                Path.Combine(overrideDirectory, name);
            string selectedPath = overridePath is not null && File.Exists(overridePath)
                ? overridePath : stockPath;
            try
            {
                return RoomCharacterAtlasPng.Read(new MemoryStream(
                    selectedPath == stockPath ? stock : File.ReadAllBytes(selectedPath),
                    writable: false), expectedBytes);
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid ending OBJ PNG {selectedPath}: {error.Message}", error);
            }
        }

        RoomBackgroundTilemapAtlas LoadMap(string name, int expectedBytes)
        {
            string stockPath = Path.Combine(stockDirectory, name);
            byte[] stock = File.ReadAllBytes(stockPath);
            if (!string.Equals(Convert.ToHexString(SHA256.HashData(stock)),
                    manifest.StockSha256[name], StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Stock ending map {stockPath} failed its manifest hash.");
            string? overridePath = overrideDirectory is null ? null :
                Path.Combine(overrideDirectory, name);
            string selectedPath = overridePath is not null && File.Exists(overridePath)
                ? overridePath : stockPath;
            try
            {
                return RoomBackgroundTilemapAtlas.Load(new MemoryStream(
                    selectedPath == stockPath ? stock : File.ReadAllBytes(selectedPath),
                    writable: false), expectedBytes);
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid ending map {selectedPath}: {error.Message}", error);
            }
        }
    }

    /// <summary>Validates stock provenance, complete file coverage and digests, and compilation of every ending/reward sheet, map, and sprite composition; ignores overrides.</summary>
    /// <param name="directory">Installed stock ending object-artwork directory.</param>
    public static void ValidateStock(string directory) => _ = Load(directory, null);

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

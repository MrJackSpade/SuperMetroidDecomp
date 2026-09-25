using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts ending/reward character PNGs and the waiting-scene BG2 tilemap.</summary>
public static class EndingObjectArtworkFiles
{
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);
        var hashes = new Dictionary<string, string>(StringComparer.Ordinal);
        Export(EndingObjectArtworkFormat.CloudFileName,
            EndingCreditsRomData.Assets.EscapeCloudCharacters,
            EndingObjectArtworkFormat.CloudByteCount);
        Export(EndingObjectArtworkFormat.ExplosionFileName,
            EndingCreditsRomData.Assets.EndingObjectCharacters,
            EndingObjectArtworkFormat.ExplosionByteCount);
        int[] fragmentSources =
        [
            EndingCreditsRomData.Assets.EndingObjectCharacters70,
            EndingCreditsRomData.Assets.EndingObjectCharacters74,
            EndingCreditsRomData.Assets.EndingObjectCharacters78,
            EndingCreditsRomData.Assets.EndingObjectCharacters7C,
        ];
        for (int index = 0; index < fragmentSources.Length; index++)
            Export(EndingObjectArtworkFormat.FragmentFileName(index),
                fragmentSources[index], EndingObjectArtworkFormat.FragmentByteCount);
        Export(EndingObjectArtworkFormat.WaitingSamusFileName,
            EndingCreditsRomData.Assets.WaitingForCreditsCharacters,
            EndingObjectArtworkFormat.RewardByteCount);
        Export(EndingObjectArtworkFormat.ShootingScreenFileName,
            EndingCreditsRomData.Assets.ShootingScreenCharacters,
            EndingObjectArtworkFormat.RewardByteCount);
        Export(EndingObjectArtworkFormat.SuitlessSamusFileName,
            EndingCreditsRomData.Assets.SuitlessSamusCharacters,
            EndingObjectArtworkFormat.RewardByteCount);
        ExportMap(EndingObjectArtworkFormat.WaitingTilemapFileName,
            EndingCreditsRomData.Assets.WaitingForCreditsTilemap,
            EndingObjectArtworkFormat.WaitingTilemapByteCount);
        Export(EndingObjectArtworkFormat.PostCreditsFragmentAFileName,
            EndingCreditsRomData.Assets.PostCreditsTileFragmentA,
            EndingObjectArtworkFormat.PostCreditsFragmentAByteCount);
        Export(EndingObjectArtworkFormat.PostCreditsFragmentBFileName,
            EndingCreditsRomData.Assets.PostCreditsTileFragmentB,
            EndingObjectArtworkFormat.PostCreditsFragmentBByteCount);
        Export(EndingObjectArtworkFormat.PostShotLogoTileFileName,
            EndingPostShotDefinitions.LogoTiles,
            EndingObjectArtworkFormat.PostShotLogoTileByteCount);
        ExportMap(EndingObjectArtworkFormat.PostShotLogoMapFileName,
            EndingPostShotDefinitions.LogoMap,
            EndingObjectArtworkFormat.PostShotLogoMapByteCount);

        var cloudFrames = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal);
        foreach (EndingCloudSpriteFrameDefinition definition in EndingCloudSpriteDefinitions.Frames)
            cloudFrames.Add(definition.Name, IntroCinematicSpriteFrameExtractor.Extract(
                bus, definition.Pointer, definition.StockPartCount, definition.Name));
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

        using var manifest = new FileStream(Path.Combine(directory,
            EndingObjectArtworkFormat.ManifestFileName), FileMode.CreateNew,
            FileAccess.Write);
        JsonSerializer.Serialize(manifest,
            new Manifest(EndingObjectArtworkFormat.ManifestVersion,
                sourceCartridgeSha256, hashes), JsonOptions);

        void Export(string name, int source, int count)
        {
            byte[] decompressed = RomDataReader.Decompress(bus, source,
                EndingCreditsRomData.Rendering.DecompressionLimit);
            if (decompressed.Length < count)
                throw new InvalidDataException($"Ending OBJ source ${source:X6} is shorter than {name}'s native DMA.");
            byte[] native = decompressed.AsSpan(0, count).ToArray();
            byte[] pixels = SnesGraphics.DecodePlanarTiles(native, 4,
                IntroCinematicArtworkFormat.TileColumns, out int width, out int height);
            using var png = new MemoryStream();
            IndexedPng.Write(png, width, height, pixels, SnesGraphics.DiagnosticPalette(16));
            byte[] file = png.ToArray();
            RoomCharacterAtlas compiled = RoomCharacterAtlas.Load(
                new MemoryStream(file, writable: false), count);
            if (!compiled.Transfer.Span.SequenceEqual(native))
                throw new InvalidDataException($"Ending OBJ PNG {name} changed native character bytes.");
            using (var output = new FileStream(Path.Combine(directory, name),
                       FileMode.CreateNew, FileAccess.Write))
                output.Write(file);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(file)));
        }

        void ExportMap(string name, int source, int count)
        {
            byte[] decoded = RomDataReader.Decompress(bus, source,
                EndingCreditsRomData.Rendering.DecompressionLimit);
            if (decoded.Length < count)
                throw new InvalidDataException($"Ending map source ${source:X6} is shorter than {name}'s native DMA.");
            byte[] native = decoded.AsSpan(0, count).ToArray();
            byte[] json = RoomBackgroundTilemapExtractor.Encode(native);
            RoomBackgroundTilemapAtlas compiled = RoomBackgroundTilemapAtlas.Load(
                new MemoryStream(json, writable: false), count);
            if (!compiled.Transfer.Span.SequenceEqual(native))
                throw new InvalidDataException($"Ending map JSON {name} changed native BG2 words.");
            using (var output = new FileStream(Path.Combine(directory, name),
                       FileMode.CreateNew, FileAccess.Write))
                output.Write(json);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(json)));
        }
    }

    public static EndingObjectArtworkCatalog Load(string stockDirectory,
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
        ];
        if (manifest.Version != EndingObjectArtworkFormat.ManifestVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase) ||
            manifest.StockSha256 is null || manifest.StockSha256.Count != names.Length ||
            names.Any(name => !manifest.StockSha256.ContainsKey(name)))
            throw new InvalidDataException($"Ending OBJ manifest {manifestPath} does not describe this installation.");

        return new EndingObjectArtworkCatalog(
            LoadSheet(EndingObjectArtworkFormat.CloudFileName,
                EndingObjectArtworkFormat.CloudByteCount),
            LoadSheet(EndingObjectArtworkFormat.ExplosionFileName,
                EndingObjectArtworkFormat.ExplosionByteCount),
            Enumerable.Range(0, EndingObjectArtworkFormat.FragmentCount)
                .Select(index => LoadSheet(EndingObjectArtworkFormat.FragmentFileName(index),
                    EndingObjectArtworkFormat.FragmentByteCount))
                .ToArray(),
            LoadSheet(EndingObjectArtworkFormat.WaitingSamusFileName,
                EndingObjectArtworkFormat.RewardByteCount),
            LoadSheet(EndingObjectArtworkFormat.ShootingScreenFileName,
                EndingObjectArtworkFormat.RewardByteCount),
            LoadSheet(EndingObjectArtworkFormat.SuitlessSamusFileName,
                EndingObjectArtworkFormat.RewardByteCount),
            LoadMap(EndingObjectArtworkFormat.WaitingTilemapFileName,
                EndingObjectArtworkFormat.WaitingTilemapByteCount),
            LoadSheet(EndingObjectArtworkFormat.PostCreditsFragmentAFileName,
                EndingObjectArtworkFormat.PostCreditsFragmentAByteCount),
            LoadSheet(EndingObjectArtworkFormat.PostCreditsFragmentBFileName,
                EndingObjectArtworkFormat.PostCreditsFragmentBByteCount),
            LoadSheet(EndingObjectArtworkFormat.PostShotLogoTileFileName,
                EndingObjectArtworkFormat.PostShotLogoTileByteCount),
            LoadMap(EndingObjectArtworkFormat.PostShotLogoMapFileName,
                EndingObjectArtworkFormat.PostShotLogoMapByteCount),
            LoadCloudSprites());

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
                return RoomCharacterAtlas.Load(new MemoryStream(
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

    public static void ValidateStock(string directory) => _ = Load(directory, null);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    private sealed record Manifest(int Version, string SourceCartridgeSha256,
        Dictionary<string, string> StockSha256);
}

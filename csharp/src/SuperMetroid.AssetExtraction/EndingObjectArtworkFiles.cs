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
        byte[] waitingMap = RomDataReader.Decompress(bus,
            EndingCreditsRomData.Assets.WaitingForCreditsTilemap,
            EndingCreditsRomData.Rendering.ObjectFragmentLimit);
        if (waitingMap.Length < EndingObjectArtworkFormat.WaitingTilemapByteCount)
            throw new InvalidDataException("Waiting-Samus map does not fill its native 32x32 BG2 page.");
        byte[] nativeMap = waitingMap.AsSpan(0,
            EndingObjectArtworkFormat.WaitingTilemapByteCount).ToArray();
        byte[] mapJson = RoomBackgroundTilemapExtractor.Encode(nativeMap);
        RoomBackgroundTilemapAtlas compiledMap = RoomBackgroundTilemapAtlas.Load(
            new MemoryStream(mapJson, writable: false), nativeMap.Length);
        if (!compiledMap.Transfer.Span.SequenceEqual(nativeMap))
            throw new InvalidDataException("Waiting-Samus map JSON changed native BG2 words.");
        using (var output = new FileStream(Path.Combine(directory,
                   EndingObjectArtworkFormat.WaitingTilemapFileName),
                   FileMode.CreateNew, FileAccess.Write))
            output.Write(mapJson);
        hashes.Add(EndingObjectArtworkFormat.WaitingTilemapFileName,
            Convert.ToHexString(SHA256.HashData(mapJson)));
        Export(EndingObjectArtworkFormat.PostCreditsFragmentAFileName,
            EndingCreditsRomData.Assets.PostCreditsTileFragmentA,
            EndingObjectArtworkFormat.PostCreditsFragmentAByteCount);
        Export(EndingObjectArtworkFormat.PostCreditsFragmentBFileName,
            EndingCreditsRomData.Assets.PostCreditsTileFragmentB,
            EndingObjectArtworkFormat.PostCreditsFragmentBByteCount);

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
            LoadMap(),
            LoadSheet(EndingObjectArtworkFormat.PostCreditsFragmentAFileName,
                EndingObjectArtworkFormat.PostCreditsFragmentAByteCount),
            LoadSheet(EndingObjectArtworkFormat.PostCreditsFragmentBFileName,
                EndingObjectArtworkFormat.PostCreditsFragmentBByteCount));

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

        RoomBackgroundTilemapAtlas LoadMap()
        {
            string name = EndingObjectArtworkFormat.WaitingTilemapFileName;
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
                    writable: false), EndingObjectArtworkFormat.WaitingTilemapByteCount);
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

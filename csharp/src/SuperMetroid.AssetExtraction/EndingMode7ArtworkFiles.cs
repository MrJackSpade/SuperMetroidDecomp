using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installs independently editable ending Mode-7 maps and character PNGs.</summary>
public static class EndingMode7ArtworkFiles
{
    private const int ManifestVersion = 2;

    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);
        var hashes = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (EndingMode7SceneId id in Enum.GetValues<EndingMode7SceneId>())
        {
            (int characterSource, int packedMapSource) = Sources(id);
            byte[] characters = RomDataReader.Decompress(bus, characterSource,
                EndingCreditsRomData.Rendering.DecompressionLimit);
            byte[] packedMap = RomDataReader.Decompress(bus, packedMapSource,
                EndingCreditsRomData.Rendering.DecompressionLimit);
            if (characters.Length < EndingMode7ArtworkFormat.CharacterByteCount ||
                packedMap.Length < EndingMode7ArtworkFormat.CharacterByteCount)
                throw new InvalidDataException($"Ending {id} Mode-7 source is shorter than its native DMA.");

            // The original two $4000-byte packed DMAs place the same $2000 low-byte
            // sequence in both map halves. Its odd bytes are subsequently replaced
            // by the character source via the high-byte-only Mode-7 port.
            var lowMap = new byte[EndingMode7ArtworkFormat.MapByteCount];
            for (int cell = 0; cell < lowMap.Length; cell++)
                lowMap[cell] = packedMap[cell * 2];
            using var mapJson = new MemoryStream();
            EndingMode7SceneArtwork.WriteMap(mapJson, new EndingMode7MapDocument
            {
                Version = EndingMode7ArtworkFormat.Version,
                Width = EndingMode7ArtworkFormat.MapWidth,
                Height = EndingMode7ArtworkFormat.MapHeight,
                Tiles = lowMap.Select(value => (int)value).ToArray(),
            });

            byte[] pixels = SnesGraphics.DecodeMode7Tiles(
                characters.AsSpan(0, EndingMode7ArtworkFormat.CharacterByteCount).ToArray(),
                16, out int width, out int height);
            if (width != EndingMode7ArtworkFormat.CharacterWidth ||
                height != EndingMode7ArtworkFormat.CharacterHeight)
                throw new InvalidDataException($"Ending {id} Mode-7 characters have unexpected dimensions.");
            using var png = new MemoryStream();
            IndexedPng.Write(png, width, height, pixels, SnesGraphics.DiagnosticPalette(256));
            byte[] mapFile = mapJson.ToArray();
            byte[] pngFile = png.ToArray();
            EndingMode7SceneArtwork compiled = EndingMode7SceneArtwork.Load(
                new MemoryStream(mapFile, writable: false),
                new MemoryStream(pngFile, writable: false));
            if (!compiled.Map.Span.SequenceEqual(lowMap) ||
                !compiled.Characters.Span.SequenceEqual(characters.AsSpan(0,
                    EndingMode7ArtworkFormat.CharacterByteCount)))
                throw new InvalidDataException($"Ending {id} PNG/JSON did not round-trip its native DMA.");
            Write(EndingMode7ArtworkFormat.MapFileName(id), mapFile);
            Write(EndingMode7ArtworkFormat.CharacterFileName(id), pngFile);
        }
        byte[] reward = RomDataReader.Decompress(bus,
            EndingCreditsRomData.Assets.PostCreditsMode7Characters,
            EndingCreditsRomData.Rendering.DecompressionLimit);
        if (reward.Length < EndingRewardIconArtworkFormat.TransferBytes)
            throw new InvalidDataException("Reward icon source is shorter than its sixteen native DMAs.");
        var rewardMap = new int[EndingRewardIconArtworkFormat.MapBytes];
        var rewardCharacters = new byte[EndingRewardIconArtworkFormat.MapBytes];
        for (int index = 0; index < rewardMap.Length; index++)
        {
            rewardMap[index] = reward[index * 2];
            rewardCharacters[index] = reward[index * 2 + 1];
        }
        using var rewardJson = new MemoryStream();
        EndingRewardIconArtwork.WriteMap(rewardJson, new EndingMode7MapDocument
        {
            Version = EndingMode7ArtworkFormat.Version,
            Width = EndingRewardIconArtworkFormat.MapWidth,
            Height = EndingRewardIconArtworkFormat.MapHeight,
            Tiles = rewardMap,
        });
        byte[] rewardPixels = SnesGraphics.DecodeMode7Tiles(rewardCharacters, 16,
            out int rewardWidth, out int rewardHeight);
        using var rewardPng = new MemoryStream();
        IndexedPng.Write(rewardPng, rewardWidth, rewardHeight, rewardPixels,
            SnesGraphics.DiagnosticPalette(256));
        byte[] rewardMapFile = rewardJson.ToArray();
        byte[] rewardCharacterFile = rewardPng.ToArray();
        EndingRewardIconArtwork compiledReward = EndingRewardIconArtwork.Load(
            new MemoryStream(rewardMapFile, writable: false),
            new MemoryStream(rewardCharacterFile, writable: false));
        if (!compiledReward.Transfer.Span.SequenceEqual(reward.AsSpan(0,
                EndingRewardIconArtworkFormat.TransferBytes)))
            throw new InvalidDataException("Reward icon PNG/JSON changed native interleaved DMA bytes.");
        Write(EndingRewardIconArtworkFormat.MapFileName, rewardMapFile);
        Write(EndingRewardIconArtworkFormat.CharacterFileName, rewardCharacterFile);
        using var manifest = new FileStream(Path.Combine(directory,
            EndingMode7ArtworkFormat.ManifestFileName), FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(manifest,
            new Manifest(ManifestVersion, sourceCartridgeSha256, hashes), JsonOptions);

        void Write(string name, byte[] bytes)
        {
            using (var output = new FileStream(Path.Combine(directory, name),
                       FileMode.CreateNew, FileAccess.Write))
                output.Write(bytes);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(bytes)));
        }
    }

    public static EndingMode7ArtworkCatalog Load(string stockDirectory,
        string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory,
            EndingMode7ArtworkFormat.ManifestFileName);
        Manifest manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<Manifest>(File.ReadAllBytes(manifestPath),
                JsonOptions) ?? throw new InvalidDataException("Ending art manifest is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid ending art manifest {manifestPath}.", error);
        }
        string[] names =
        [
            .. Enum.GetValues<EndingMode7SceneId>()
            .SelectMany(id => new[]
            {
                EndingMode7ArtworkFormat.MapFileName(id),
                EndingMode7ArtworkFormat.CharacterFileName(id),
            }),
            EndingRewardIconArtworkFormat.MapFileName,
            EndingRewardIconArtworkFormat.CharacterFileName,
        ];
        if (manifest.Version != ManifestVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase) ||
            manifest.StockSha256 is null || manifest.StockSha256.Count != names.Length ||
            names.Any(name => !manifest.StockSha256.ContainsKey(name)))
            throw new InvalidDataException($"Ending art manifest {manifestPath} does not describe this installation.");

        return new EndingMode7ArtworkCatalog(
            LoadScene(EndingMode7SceneId.EscapeA),
            LoadScene(EndingMode7SceneId.EscapeB),
            LoadScene(EndingMode7SceneId.PlanetExplosion),
            LoadRewardIcon());

        EndingRewardIconArtwork LoadRewardIcon()
        {
            (string mapPath, byte[] map) = ReadSelected(EndingRewardIconArtworkFormat.MapFileName);
            (string pngPath, byte[] png) = ReadSelected(EndingRewardIconArtworkFormat.CharacterFileName);
            try
            {
                return EndingRewardIconArtwork.Load(
                    new MemoryStream(map, writable: false),
                    new MemoryStream(png, writable: false));
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException(
                    $"Invalid reward icon art ({mapPath}, {pngPath}): {error.Message}", error);
            }
        }

        EndingMode7SceneArtwork LoadScene(EndingMode7SceneId id)
        {
            (string mapPath, byte[] map) = ReadSelected(EndingMode7ArtworkFormat.MapFileName(id));
            (string pngPath, byte[] png) = ReadSelected(EndingMode7ArtworkFormat.CharacterFileName(id));
            try
            {
                return EndingMode7SceneArtwork.Load(
                    new MemoryStream(map, writable: false),
                    new MemoryStream(png, writable: false));
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException(
                    $"Invalid ending Mode-7 art ({mapPath}, {pngPath}): {error.Message}", error);
            }
        }

        (string Path, byte[] Bytes) ReadSelected(string name)
        {
            string stockPath = Path.Combine(stockDirectory, name);
            byte[] stock = File.ReadAllBytes(stockPath);
            if (!string.Equals(Convert.ToHexString(SHA256.HashData(stock)),
                    manifest.StockSha256[name], StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Stock ending art {stockPath} failed its manifest hash.");
            string? overridePath = overrideDirectory is null ? null : Path.Combine(overrideDirectory, name);
            string selectedPath = overridePath is not null && File.Exists(overridePath)
                ? overridePath : stockPath;
            return (selectedPath, selectedPath == stockPath ? stock : File.ReadAllBytes(selectedPath));
        }
    }

    public static void ValidateStock(string directory) => _ = Load(directory, null);

    private static (int Characters, int PackedMap) Sources(EndingMode7SceneId id) => id switch
    {
        EndingMode7SceneId.EscapeA =>
            (EndingCreditsRomData.Assets.EscapeMapA, EndingCreditsRomData.Assets.EscapeCharactersA),
        EndingMode7SceneId.EscapeB =>
            (EndingCreditsRomData.Assets.EscapeMapB, EndingCreditsRomData.Assets.EscapeCharactersB),
        EndingMode7SceneId.PlanetExplosion =>
            (EndingCreditsRomData.Assets.ExplosionMap, EndingCreditsRomData.Assets.ExplosionCharacters),
        _ => throw new ArgumentOutOfRangeException(nameof(id)),
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    private sealed record Manifest(int Version, string SourceCartridgeSha256,
        Dictionary<string, string> StockSha256);
}

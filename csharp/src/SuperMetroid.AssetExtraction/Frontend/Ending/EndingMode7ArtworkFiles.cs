using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>Installs independently editable ending Mode-7 maps and character PNGs.</summary>
[CpuBusBoundary("codec: ROM ending Mode-7 tilemaps and characters → ending Mode-7 artwork files")]
public static class EndingMode7ArtworkFiles
{
    /// <summary>Manifest revision covering the three backdrop pairs and reward-icon pair.</summary>
    private const int ManifestVersion = 2;

    /// <summary>Separates the three ending backdrops and reward icon into editable map JSON and indexed character PNGs, verifies native transfer round-trips, and creates their hashed manifest.</summary>
    /// <param name="bus">Cartridge import source for the compressed Mode-7 map and character streams.</param>
    /// <param name="directory">Destination directory, created if absent; artwork files and manifest must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Source identity recorded in the manifest; installation loading requires the supported cartridge identity.</param>
    /// <exception cref="InvalidDataException">A decompressed source is too short, has unexpected dimensions, or fails the PNG/JSON transfer round-trip.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);
        var hashes = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (EndingFloorSceneId id in Enum.GetValues<EndingFloorSceneId>())
        {
            (int characterSource, int packedMapSource) = EndingCreditsRomData.Assets.FloorSources(id);
            byte[] characters = RomDataReader.Decompress(CartridgeImportSource.Require(bus), characterSource,
                EndingCreditsRomData.Rendering.DecompressionLimit);
            byte[] packedMap = RomDataReader.Decompress(CartridgeImportSource.Require(bus), packedMapSource,
                EndingCreditsRomData.Rendering.DecompressionLimit);
            if (characters.Length < EndingFloorArtworkFormat.CharacterByteCount ||
                packedMap.Length < EndingFloorArtworkFormat.CharacterByteCount)
                throw new InvalidDataException($"Ending {id} Mode-7 source is shorter than its native DMA.");

            // The original two $4000-byte packed DMAs place the same $2000 low-byte
            // sequence in both map halves. Its odd bytes are subsequently replaced
            // by the character source via the high-byte-only Mode-7 port.
            var lowMap = new byte[EndingFloorArtworkFormat.MapByteCount];
            for (int cell = 0; cell < lowMap.Length; cell++)
                lowMap[cell] = packedMap[cell * 2];
            using var mapJson = new MemoryStream();
            EndingFloorSceneArtwork.WriteMap(mapJson, new EndingFloorMapDocument
            {
                Version = EndingFloorArtworkFormat.Version,
                Width = EndingFloorArtworkFormat.MapWidth,
                Height = EndingFloorArtworkFormat.MapHeight,
                Tiles = lowMap.Select(value => (int)value).ToArray(),
            });

            byte[] pixels = SnesGraphics.DecodeMode7Tiles(
                characters.AsSpan(0, EndingFloorArtworkFormat.CharacterByteCount).ToArray(),
                16, out Extent16<ImagePixels> size);
            if (size.X != EndingFloorArtworkFormat.CharacterWidth ||
                size.Y != EndingFloorArtworkFormat.CharacterHeight)
                throw new InvalidDataException($"Ending {id} Mode-7 characters have unexpected dimensions.");
            using var png = new MemoryStream();
            IndexedPng.Write(png, size, pixels, SnesGraphics.DiagnosticPalette(256));
            byte[] mapFile = mapJson.ToArray();
            byte[] pngFile = png.ToArray();
            EndingFloorSceneCharacters compiled = EndingFloorSceneCharacters.Read(
                new MemoryStream(mapFile, writable: false),
                new MemoryStream(pngFile, writable: false));
            if (!compiled.Map.Span.SequenceEqual(lowMap) ||
                !compiled.Characters.Span.SequenceEqual(characters.AsSpan(0,
                    EndingFloorArtworkFormat.CharacterByteCount)))
                throw new InvalidDataException($"Ending {id} PNG/JSON did not round-trip its native DMA.");
            Write(EndingFloorArtworkFormat.MapFileName(id), mapFile);
            Write(EndingFloorArtworkFormat.CharacterFileName(id), pngFile);
        }
        byte[] reward = RomDataReader.Decompress(CartridgeImportSource.Require(bus),
            EndingCreditsRomData.Assets.PostCreditsFloorCharacters,
            EndingCreditsRomData.Rendering.DecompressionLimit);
        if (reward.Length < EndingRewardIconArtworkFormat.ByteCount)
            throw new InvalidDataException("Reward icon source is shorter than its sixteen native DMAs.");
        var rewardMap = new int[EndingRewardIconArtworkFormat.MapBytes];
        var rewardCharacters = new byte[EndingRewardIconArtworkFormat.MapBytes];
        for (int index = 0; index < rewardMap.Length; index++)
        {
            rewardMap[index] = reward[index * 2];
            rewardCharacters[index] = reward[index * 2 + 1];
        }
        using var rewardJson = new MemoryStream();
        EndingRewardIconArtwork.WriteMap(rewardJson, new EndingFloorMapDocument
        {
            Version = EndingFloorArtworkFormat.Version,
            Width = EndingRewardIconArtworkFormat.MapWidth,
            Height = EndingRewardIconArtworkFormat.MapHeight,
            Tiles = rewardMap,
        });
        byte[] rewardPixels = SnesGraphics.DecodeMode7Tiles(rewardCharacters, 16,
            out Extent16<ImagePixels> rewardSize);
        using var rewardPng = new MemoryStream();
        IndexedPng.Write(rewardPng, rewardSize, rewardPixels,
            SnesGraphics.DiagnosticPalette(256));
        byte[] rewardMapFile = rewardJson.ToArray();
        byte[] rewardCharacterFile = rewardPng.ToArray();
        EndingRewardIconCharacters compiledReward = EndingRewardIconCharacters.Read(
            new MemoryStream(rewardMapFile, writable: false),
            new MemoryStream(rewardCharacterFile, writable: false));
        if (!compiledReward.Bytes.Span.SequenceEqual(reward.AsSpan(0,
                EndingRewardIconArtworkFormat.ByteCount)))
            throw new InvalidDataException("Reward icon PNG/JSON changed native interleaved DMA bytes.");
        Write(EndingRewardIconArtworkFormat.MapFileName, rewardMapFile);
        Write(EndingRewardIconArtworkFormat.CharacterFileName, rewardCharacterFile);
        using var manifest = new FileStream(Path.Combine(directory,
            EndingFloorArtworkFormat.ManifestFileName), FileMode.CreateNew, FileAccess.Write);
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

    /// <summary>Checks the complete stock manifest and file digests, then compiles independently selected ending map and character files without cartridge access.</summary>
    /// <param name="stockDirectory">Stock directory containing all three backdrop pairs, the reward-icon pair, and their provenance/digest manifest.</param>
    /// <param name="overrideDirectory">Optional directory with per-file replacements; missing map or PNG files independently fall back to verified stock.</param>
    /// <returns>The selected three-backdrop and reward-icon artwork catalog with owned decoded transfer data.</returns>
    /// <remarks>Backdrop maps are 128x64 tile-index half-maps; the reward icon uses a full 128x128 map, with map and character lanes compiled separately.</remarks>
    /// <exception cref="InvalidDataException">Manifest provenance or coverage, a stock digest, or selected map/PNG geometry and content is invalid.</exception>
    public static EndingFloorArtworkCatalog Load(string stockDirectory,
        string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory,
            EndingFloorArtworkFormat.ManifestFileName);
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
            .. Enum.GetValues<EndingFloorSceneId>()
            .SelectMany(id => new[]
            {
                EndingFloorArtworkFormat.MapFileName(id),
                EndingFloorArtworkFormat.CharacterFileName(id),
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

        return new EndingFloorCharacters(
            LoadScene(EndingFloorSceneId.EscapeA),
            LoadScene(EndingFloorSceneId.EscapeB),
            LoadScene(EndingFloorSceneId.PlanetExplosion),
            LoadRewardIcon());

        EndingRewardIconArtwork LoadRewardIcon()
        {
            (string mapPath, byte[] map) = ReadSelected(EndingRewardIconArtworkFormat.MapFileName);
            (string pngPath, byte[] png) = ReadSelected(EndingRewardIconArtworkFormat.CharacterFileName);
            try
            {
                return EndingRewardIconCharacters.Read(
                    new MemoryStream(map, writable: false),
                    new MemoryStream(png, writable: false));
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException(
                    $"Invalid reward icon art ({mapPath}, {pngPath}): {error.Message}", error);
            }
        }

        EndingFloorSceneArtwork LoadScene(EndingFloorSceneId id)
        {
            (string mapPath, byte[] map) = ReadSelected(EndingFloorArtworkFormat.MapFileName(id));
            (string pngPath, byte[] png) = ReadSelected(EndingFloorArtworkFormat.CharacterFileName(id));
            try
            {
                return EndingFloorSceneCharacters.Read(
                    new MemoryStream(map, writable: false),
                    new MemoryStream(png, writable: false));
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException(
                    $"Invalid ending Mode-7 art ({mapPath}, {pngPath}): {error.Message}", error);
            }
        }

        SelectedArtworkFile ReadSelected(string name)
        {
            string stockPath = Path.Combine(stockDirectory, name);
            byte[] stock = File.ReadAllBytes(stockPath);
            if (!string.Equals(Convert.ToHexString(SHA256.HashData(stock)),
                    manifest.StockSha256[name], StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Stock ending art {stockPath} failed its manifest hash.");
            string? overridePath = overrideDirectory is null ? null : Path.Combine(overrideDirectory, name);
            string selectedPath = overridePath is not null && File.Exists(overridePath)
                ? overridePath : stockPath;
            return new SelectedArtworkFile(selectedPath, selectedPath == stockPath ? stock : File.ReadAllBytes(selectedPath));
        }
    }

    /// <summary>Checks all stock Mode-7 file digests and compiles every backdrop and reward-icon pair against its required geometry, ignoring overrides.</summary>
    /// <param name="directory">Installed stock ending Mode-7 directory.</param>
    public static void ValidateStock(string directory) => _ = Load(directory, null);

    /// <summary>Camel-case JSON options shared by extraction and stock loading.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    /// <summary>Installed source identity and stock digest for every ending Mode-7 map and character file.</summary>
    private sealed record Manifest(int Version, string SourceCartridgeSha256,
        Dictionary<string, string> StockSha256);
}

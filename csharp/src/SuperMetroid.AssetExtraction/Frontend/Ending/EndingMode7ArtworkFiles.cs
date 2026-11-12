using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.Core;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// Installs independently editable ending Mode-7 maps; their character pages are import data (#1636), which the extraction
/// writes to the component's import-data directory and only the import reads. The game draws the floors' sheets and reads
/// neither.
/// </summary>
public static class EndingMode7ArtworkFiles
{
    /// <summary>Manifest revision covering the three backdrop pairs and reward-icon pair; 3 installs no character page (#1636).</summary>
    private const int ManifestVersion = 3;

    /// <summary>Separates the three ending backdrops and reward icon into editable map JSON and indexed character PNGs, and creates their hashed manifest.</summary>
    /// <param name="bus">Cartridge import source for the compressed Mode-7 map and character streams.</param>
    /// <param name="directory">Destination directory, created if absent; artwork files and manifest must not already exist. The character pages go to its import-data directory.</param>
    /// <param name="sourceCartridgeSha256">Source identity recorded in the manifest; installation loading requires the supported cartridge identity.</param>
    /// <exception cref="InvalidDataException">A decompressed source is too short or has unexpected dimensions.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);
        string importData = SuperMetroid.AssetExtraction.Installation.ImportDataDirectory.Of(directory);
        Directory.CreateDirectory(importData);
        var hashes = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (EndingFloorSceneId id in Enum.GetValues<EndingFloorSceneId>())
        {
            (int characterSource, int packedMapSource) = EndingArtworkSources.Floor(id);
            byte[] characters = RomDataReader.Decompress(CartridgeImportSource.Require(bus), characterSource,
                EndingSourceDefinitions.Artwork.DecompressionLimit);
            byte[] packedMap = RomDataReader.Decompress(CartridgeImportSource.Require(bus), packedMapSource,
                EndingSourceDefinitions.Artwork.DecompressionLimit);
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
            EndingFloorSceneCharacters.WriteMap(mapJson, new EndingFloorMapDocument
            {
                Version = EndingFloorArtworkFormat.Version,
                Width = EndingFloorArtworkFormat.MapWidth,
                Height = lowMap.Length / EndingFloorArtworkFormat.MapWidth,
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
            Write(directory, EndingFloorArtworkFormat.MapFileName(id), mapFile);
            Write(importData, EndingFloorArtworkFormat.CharacterFileName(id), pngFile);
        }
        byte[] reward = RomDataReader.Decompress(CartridgeImportSource.Require(bus),
            EndingSourceDefinitions.Artwork.PostCreditsFloorCharacters,
            EndingSourceDefinitions.Artwork.DecompressionLimit);
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
            Height = rewardMap.Length / EndingRewardIconArtworkFormat.MapWidth,
            Tiles = rewardMap,
        });
        byte[] rewardPixels = SnesGraphics.DecodeMode7Tiles(rewardCharacters, 16,
            out Extent16<ImagePixels> rewardSize);
        using var rewardPng = new MemoryStream();
        IndexedPng.Write(rewardPng, rewardSize, rewardPixels,
            SnesGraphics.DiagnosticPalette(256));
        byte[] rewardMapFile = rewardJson.ToArray();
        byte[] rewardCharacterFile = rewardPng.ToArray();
        Write(directory, EndingRewardIconArtworkFormat.MapFileName, rewardMapFile);
        Write(importData, EndingRewardIconArtworkFormat.CharacterFileName, rewardCharacterFile);
        using var manifest = new FileStream(Path.Combine(directory,
            EndingFloorArtworkFormat.ManifestFileName), FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(manifest,
            new Manifest(ManifestVersion, sourceCartridgeSha256, hashes), JsonOptions);

        void Write(string destination, string name, byte[] bytes)
        {
            using (var output = new FileStream(Path.Combine(destination, name),
                       FileMode.CreateNew, FileAccess.Write))
                output.Write(bytes);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(bytes)));
        }
    }

    /// <summary>
    /// Checks the manifest's provenance and coverage, then compiles the extracted ending maps and the character pages of
    /// the import data (#1636) without cartridge access.
    /// </summary>
    /// <param name="directory">The component's directory, holding the three backdrop maps, the reward-icon map and their provenance/digest manifest.</param>
    /// <param name="importData">The component's import-data directory, holding the character pages.</param>
    /// <returns>The three-backdrop and reward-icon artwork catalog with owned decoded transfer data.</returns>
    /// <remarks>Backdrop maps are 128x64 tile-index half-maps; the reward icon uses a full 128x128 map, with map and character lanes compiled separately.</remarks>
    /// <exception cref="InvalidDataException">Manifest provenance or coverage, or an installed map/PNG geometry and content is invalid.</exception>
    public static EndingFloorCharacters LoadForImport(string directory, string importData)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(importData);
        string manifestPath = Path.Combine(directory,
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
            (string mapPath, byte[] map) = ReadInstalled(directory, EndingRewardIconArtworkFormat.MapFileName);
            (string pngPath, byte[] png) = ReadInstalled(importData, EndingRewardIconArtworkFormat.CharacterFileName);
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
            (string mapPath, byte[] map) = ReadInstalled(directory, EndingFloorArtworkFormat.MapFileName(id));
            (string pngPath, byte[] png) = ReadInstalled(importData, EndingFloorArtworkFormat.CharacterFileName(id));
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

        static SelectedArtworkFile ReadInstalled(string source, string name)
        {
            string installedPath = Path.Combine(source, name);
            return new SelectedArtworkFile(installedPath, File.ReadAllBytes(installedPath));
        }
    }

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

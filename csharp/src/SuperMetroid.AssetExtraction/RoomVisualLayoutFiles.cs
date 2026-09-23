using System.Buffers.Binary;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Extracts initial room visual-block references without exporting collision types or BTS.
/// A user's JSON override may alter only bank-$80's initial tilemap source; gameplay keeps
/// the original decompressed room allocation and all later PLM writes.
/// </summary>
public static class RoomVisualLayoutFiles
{
    public const string ManifestFileName = "room-layouts.json";
    private const int FormatVersion = 1;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    /// <summary>Distinct visual sources selected by retail room states and their row strides.</summary>
    public static IReadOnlyDictionary<int, int> RetailSources { get; } = BuildRetailSources();

    public static string SourceFileName(int sourceAddress) => $"level-{sourceAddress:X6}.json";

    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);
        var entries = new Dictionary<string, LayoutFileEntry>();
        foreach ((int sourceAddress, int widthInBlocks) in RetailSources.OrderBy(item => item.Key))
        {
            RoomVisualLayoutDocument document = Decode(bus, sourceAddress, widthInBlocks);
            byte[] json = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
            string name = SourceFileName(sourceAddress);
            using (var file = new FileStream(Path.Combine(directory, name), FileMode.CreateNew,
                       FileAccess.Write))
                file.Write(json);
            entries.Add(name, new LayoutFileEntry(sourceAddress, widthInBlocks,
                document.HeightInBlocks, Convert.ToHexString(SHA256.HashData(json))));
        }
        using var manifest = new FileStream(Path.Combine(directory, ManifestFileName),
            FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(manifest,
            new LayoutFileManifest(FormatVersion, sourceCartridgeSha256, entries), JsonOptions);
    }

    public static RoomVisualLayoutCatalog Load(string stockDirectory, string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, ManifestFileName);
        LayoutFileManifest manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<LayoutFileManifest>(
                File.ReadAllBytes(manifestPath), JsonOptions)
                ?? throw new InvalidDataException("Room-layout manifest is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid room-layout manifest {manifestPath}.", error);
        }
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase) ||
            manifest.Entries is null || manifest.Entries.Count != RetailSources.Count)
            throw new InvalidDataException(
                $"Room-layout manifest {manifestPath} does not cover the pinned retail sources.");

        var selected = new Dictionary<int, RoomVisualLayout>();
        foreach ((string name, LayoutFileEntry entry) in manifest.Entries)
        {
            if (entry is null || name != SourceFileName(entry.SourceAddress) ||
                !RetailSources.TryGetValue(entry.SourceAddress, out int width) ||
                width != entry.WidthInBlocks || entry.HeightInBlocks <= 0)
                throw new InvalidDataException(
                    $"Room-layout manifest {manifestPath} has an invalid source entry.");
            string stockPath = Path.Combine(stockDirectory, name);
            byte[] stock = File.ReadAllBytes(stockPath);
            if (!string.Equals(Convert.ToHexString(SHA256.HashData(stock)), entry.Sha256,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Stock room layout {stockPath} failed its manifest hash.");
            string? overridePath = overrideDirectory is null ? null : Path.Combine(overrideDirectory, name);
            string selectedPath = overridePath is not null && File.Exists(overridePath)
                ? overridePath : stockPath;
            byte[] json = selectedPath == stockPath ? stock : File.ReadAllBytes(selectedPath);
            RoomVisualLayoutDocument document;
            try
            {
                document = JsonSerializer.Deserialize<RoomVisualLayoutDocument>(json, JsonOptions)
                    ?? throw new InvalidDataException("Room layout document is empty.");
            }
            catch (JsonException error)
            {
                throw new InvalidDataException($"Invalid room layout {selectedPath}.", error);
            }
            if (document.FormatVersion != FormatVersion ||
                document.SourceAddress != entry.SourceAddress ||
                document.WidthInBlocks != width ||
                document.HeightInBlocks != entry.HeightInBlocks ||
                document.ForegroundVisualWords is null ||
                document.BackgroundVisualWords is null)
                throw new InvalidDataException($"Room layout {selectedPath} has incompatible dimensions or identity.");
            RoomVisualLayout layout;
            try
            {
                layout = new RoomVisualLayout(document.SourceAddress,
                    document.WidthInBlocks, document.HeightInBlocks,
                    document.ForegroundVisualWords, document.BackgroundVisualWords);
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid room layout {selectedPath}: {error.Message}", error);
            }
            if (!selected.TryAdd(entry.SourceAddress, layout))
                throw new InvalidDataException($"Room-layout manifest {manifestPath} repeats a source.");
        }
        return new RoomVisualLayoutCatalog(selected);
    }

    public static void ValidateStock(string directory) => _ = Load(directory, null);

    private static IReadOnlyDictionary<int, int> BuildRetailSources()
    {
        var sources = new Dictionary<int, int>();
        foreach (RoomHeaderDefinition header in RoomHeaderDefinitions.All)
        {
            int width = checked(header.WidthInScreens * 16);
            foreach (ushort pointer in RoomStateSelectionDefinitions.GetStatePointers(header.Pointer))
            {
                int source = RoomStateDefinitions.Get(pointer).CompressedLevelDataAddress;
                if (sources.TryGetValue(source, out int existing) && existing != width)
                    throw new InvalidDataException(
                        $"Room level ${source:X6} is shared by {existing}- and {width}-block row strides.");
                sources[source] = width;
            }
        }
        return new ReadOnlyDictionary<int, int>(sources);
    }

    private static RoomVisualLayoutDocument Decode(ISnesAddressSpace bus,
        int sourceAddress, int widthInBlocks)
    {
        byte[] stream = RomDataReader.Decompress(bus, sourceAddress);
        if (stream.Length < 2)
            throw new InvalidDataException($"Room level ${sourceAddress:X6} has no size word.");
        int layerBytes = BinaryPrimitives.ReadUInt16LittleEndian(stream);
        int blockCount = layerBytes / 2;
        if ((layerBytes & 1) != 0 || blockCount == 0 || blockCount % widthInBlocks != 0 ||
            stream.Length < 2 + layerBytes + blockCount)
            throw new InvalidDataException(
                $"Room level ${sourceAddress:X6} has an invalid BG1/BTS allocation.");
        var foreground = new ushort[blockCount];
        var background = new ushort[blockCount];
        int backgroundOffset = 2 + layerBytes + blockCount;
        int availableBackgroundBytes = Math.Min(layerBytes, stream.Length - backgroundOffset);
        if ((availableBackgroundBytes & 1) != 0)
            throw new InvalidDataException($"Room level ${sourceAddress:X6} ends inside a BG2 word.");
        for (int index = 0; index < blockCount; index++)
        {
            foreground[index] = unchecked((ushort)(
                BinaryPrimitives.ReadUInt16LittleEndian(stream.AsSpan(2 + index * 2)) & 0x0fff));
            int offset = backgroundOffset + index * 2;
            if (offset + 2 <= backgroundOffset + availableBackgroundBytes)
                background[index] = unchecked((ushort)(
                    BinaryPrimitives.ReadUInt16LittleEndian(stream.AsSpan(offset)) & 0x0fff));
        }
        return new RoomVisualLayoutDocument(FormatVersion, sourceAddress,
            widthInBlocks, blockCount / widthInBlocks, foreground, background);
    }

    private sealed record LayoutFileEntry(int SourceAddress, int WidthInBlocks,
        int HeightInBlocks, string Sha256);
    private sealed record LayoutFileManifest(int Version, string SourceCartridgeSha256,
        Dictionary<string, LayoutFileEntry> Entries);
    private sealed record RoomVisualLayoutDocument(int FormatVersion, int SourceAddress,
        int WidthInBlocks, int HeightInBlocks, ushort[] ForegroundVisualWords,
        ushort[] BackgroundVisualWords);
}

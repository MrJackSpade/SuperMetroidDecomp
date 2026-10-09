using System.Buffers.Binary;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
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
    /// <summary>Stock manifest filename recording version-one provenance and each retail source's address, block dimensions, filename, and JSON byte hash.</summary>
    public const string ManifestFileName = "room-layouts.json";
    /// <summary>Current manifest and per-source visual-layout document schema version.</summary>
    private const int FormatVersion = 1;

    /// <summary>Strict camel-case JSON settings shared by stock manifests and editable layout documents.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectRequiredConstructorParameters = true,
        WriteIndented = true,
    };

    /// <summary>Distinct visual sources selected by retail room states and their row strides.</summary>
    public static IReadOnlyDictionary<int, int> RetailSources { get; } = BuildRetailSources();

    /// <summary>Formats the stable per-source stock/override JSON filename without reading or validating the source.</summary>
    /// <param name="sourceAddress">Native 24-bit compressed level-data identity, shared by all room states using that source; membership validation occurs during loading.</param>
    /// <returns><c>level-</c> followed by the uppercase hexadecimal address padded to at least six digits and <c>.json</c>.</returns>
    public static string SourceFileName(int sourceAddress) => $"level-{sourceAddress:X6}.json";

    /// <summary>Decompresses every distinct retail room visual source and exports row-major foreground/background metatile-and-flip words without collision types or BTS.</summary>
    /// <param name="bus">Non-null import-capable cartridge source for the compressed level streams identified by <see cref="RetailSources"/>.</param>
    /// <param name="directory">Family directory, created if absent; receives new per-source JSON files and the required manifest.</param>
    /// <param name="sourceCartridgeSha256">Nonblank caller-supplied provenance hash, recorded without recomputation and checked against the supported identity on stock load.</param>
    /// <remarks>Widths and heights use 16-pixel blocks. Foreground allocation determines height; absent trailing background words become zero, and every exported word keeps only bits 0..11. Create-new writes refuse existing files and do not roll back earlier outputs.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory/source hash is blank or the address space lacks cartridge import access.</exception>
    /// <exception cref="InvalidDataException">A compressed stream, foreground/BTS allocation, row stride, or trailing background-word boundary is invalid.</exception>
    /// <exception cref="IOException">An output exists or filesystem access fails.</exception>
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

    /// <summary>Checks the complete retail-source manifest and stock hashes, then selects independently replaceable initial room visual layouts.</summary>
    /// <param name="stockDirectory">Family directory containing all per-source stock JSON files and the required provenance/dimension manifest.</param>
    /// <param name="overrideDirectory">Optional family directory; each absent source JSON independently falls back to stock.</param>
    /// <returns>A ROM-independent catalog of selected initial foreground/background visual words, preserving gameplay allocation, collision/BTS data, and later PLM writes.</returns>
    /// <remarks>Stock provenance and every byte hash remain mandatory with overrides. Strict selected JSON must retain source identity, original block dimensions, array extents, and twelve-bit words; no override manifest is needed. An overridden stock document is hash-checked but not separately deserialized or compared with cartridge bytes.</remarks>
    /// <exception cref="ArgumentException"><paramref name="stockDirectory"/> is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">Provenance, coverage, integrity, strict JSON, source identity, dimensions, or visual word arrays are invalid.</exception>
    /// <exception cref="IOException">A required stock or selected override file cannot be read.</exception>
    public static RoomVisualLayoutCatalog Load(string stockDirectory, string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, ManifestFileName);
        using var manifestStream = File.OpenRead(manifestPath);
        LayoutFileManifest manifest = JsonAssetDocument.Read<LayoutFileManifest>(
            manifestStream, JsonOptions, $"room-layout manifest {manifestPath}");
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
            using PooledFileBytes stock = PooledFileBytes.Read(stockPath);
            if (!string.Equals(Convert.ToHexString(SHA256.HashData(stock.Span)), entry.Sha256,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Stock room layout {stockPath} failed its manifest hash.");
            string? overridePath = overrideDirectory is null ? null : Path.Combine(overrideDirectory, name);
            string selectedPath = overridePath is not null && File.Exists(overridePath)
                ? overridePath : stockPath;
            using PooledFileBytes? edited = selectedPath == stockPath ? null : PooledFileBytes.Read(selectedPath);
            PooledFileBytes json = edited ?? stock;
            using MemoryStream jsonStream = json.OpenRead();
            RoomVisualLayoutDocument document = JsonAssetDocument.Read<RoomVisualLayoutDocument>(
                jsonStream, JsonOptions, $"room layout {selectedPath}");
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

    /// <summary>Loads and validates every stock source through <see cref="Load"/> with no overrides, cartridge reads, or file writes.</summary>
    /// <param name="directory">Room-layout family directory containing the manifest and all referenced stock JSON files.</param>
    /// <exception cref="InvalidDataException">Stock provenance, hashes, schema, coverage, or layouts are invalid.</exception>
    public static void ValidateStock(string directory) => _ = Load(directory, null);

    /// <summary>Collects every distinct retail compressed level source and proves that shared sources use one row stride.</summary>
    /// <returns>Read-only source-address to width-in-blocks mapping.</returns>
    private static ReadOnlyDictionary<int, int> BuildRetailSources()
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

    /// <summary>Decompresses one native level allocation and separates its initial BG1 and optional BG2 visual words from collision/BTS data.</summary>
    /// <param name="bus">Import address space containing the compressed level stream.</param>
    /// <param name="sourceAddress">SNES CPU address of the compressed stream.</param>
    /// <param name="widthInBlocks">Retail room row stride in 16-pixel blocks.</param>
    /// <returns>A versioned document with row-major twelve-bit foreground and background visual words.</returns>
    private static RoomVisualLayoutDocument Decode(ISnesAddressSpace bus,
        int sourceAddress, int widthInBlocks)
    {
        byte[] stream = RomDataReader.Decompress(CartridgeImportSource.Require(bus), sourceAddress);
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

    /// <summary>Manifest identity, dimensions, and stock byte hash for one compressed level source.</summary>
    /// <param name="SourceAddress">Native 24-bit compressed level-data address.</param>
    /// <param name="WidthInBlocks">Retail row stride in 16-pixel blocks.</param>
    /// <param name="HeightInBlocks">Height derived from the native BG1 allocation.</param>
    /// <param name="Sha256">Uppercase SHA-256 of the exact stock layout JSON bytes.</param>
    private sealed record LayoutFileEntry(int SourceAddress, int WidthInBlocks,
        int HeightInBlocks, string Sha256);
    /// <summary>Stock provenance and complete per-file inventory for all retail room visual layouts.</summary>
    /// <param name="Version">Manifest schema version.</param>
    /// <param name="SourceCartridgeSha256">Cartridge identity from which stock layouts were extracted.</param>
    /// <param name="Entries">Filename-keyed source identities, dimensions, and byte hashes.</param>
    private sealed record LayoutFileManifest(int Version, string SourceCartridgeSha256,
        Dictionary<string, LayoutFileEntry> Entries);
    /// <summary>Editable initial BG visual allocation for one distinct native room-level source.</summary>
    /// <param name="FormatVersion">Layout document schema version.</param>
    /// <param name="SourceAddress">Immutable native source identity.</param>
    /// <param name="WidthInBlocks">Row width in 16-pixel blocks.</param>
    /// <param name="HeightInBlocks">Row count implied by the native BG1 allocation.</param>
    /// <param name="ForegroundVisualWords">Row-major twelve-bit BG1 metatile/flip words.</param>
    /// <param name="BackgroundVisualWords">Row-major twelve-bit BG2 words, zero-filled where the native stream omits BG2 data.</param>
    private sealed record RoomVisualLayoutDocument(int FormatVersion, int SourceAddress,
        int WidthInBlocks, int HeightInBlocks, ushort[] ForegroundVisualWords,
        ushort[] BackgroundVisualWords);
}

using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Installs named elevator-platform appearances. Native draw geometry and physical level
/// words are checked against the compiled cartridge definitions.
/// </summary>
public static class RoomPlmElevatorPlatformVisualFiles
{
    /// <summary>Family-relative stock/override JSON filename for the elevator platform's three six-cell artwork frames.</summary>
    public const string VisualFileName = "elevator-platforms.json";
    /// <summary>Stock manifest filename containing version-one format, cartridge provenance, and the visual JSON's SHA-256.</summary>
    public const string ManifestFileName = "manifest.json";
    /// <summary>Schema version accepted for elevator-platform visual documents and manifests.</summary>
    private const int FormatVersion = 1;

    /// <summary>Shared camel-case JSON settings used for elevator-platform visual files.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    /// <summary>Verifies the three elevator-platform draws at $84:AA97–AADE and exports their metatile/flip words, not their solid collision bits.</summary>
    /// <param name="bus">Cartridge source whose three-run frame shapes, full level words, and continuation offsets must match compiled definitions.</param>
    /// <param name="directory">Family directory, created before native checks; receives new visual JSON and manifest files.</param>
    /// <param name="sourceCartridgeSha256">Nonblank importer-supplied provenance hash, recorded without recomputation and checked against the supported identity on stock load.</param>
    /// <remarks>Each frame retains native upper-edge/upper-edge/lower-row run order. Existing outputs are refused; failure of the later manifest write does not remove the earlier JSON.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory or source hash is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">Native draw geometry or physical words differ from compiled data.</exception>
    /// <exception cref="IOException">An output exists or filesystem access fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);
        RoomPlmElevatorPlatformVisualEntry[] entries = ReadAndVerifyNative(bus);
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(
            new VisualDocument(FormatVersion, entries), JsonOptions);
        using (var output = new FileStream(Path.Combine(directory, VisualFileName),
                   FileMode.CreateNew, FileAccess.Write))
            output.Write(json);
        using var manifest = new FileStream(Path.Combine(directory, ManifestFileName),
            FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(manifest,
            new VisualManifest(FormatVersion, sourceCartridgeSha256,
                Convert.ToHexString(SHA256.HashData(json))), JsonOptions);
    }

    /// <summary>Validates stock platform artwork before selecting an optional complete three-frame visual replacement.</summary>
    /// <param name="stockDirectory">Family directory containing version-one stock JSON and its required provenance/hash manifest.</param>
    /// <param name="overrideDirectory">Optional family directory; null or missing visual JSON uses validated stock.</param>
    /// <returns>A ROM-independent catalog with copied authored visual words; solid geometry and the four-update first/second/third/second loop stay compiled.</returns>
    /// <remarks>Stock must match supported provenance, its byte hash, and compiled visuals even when overridden. Replacement JSON needs all three identities with original run lengths and twelve-bit words, but no manifest.</remarks>
    /// <exception cref="ArgumentException"><paramref name="stockDirectory"/> is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">Provenance, integrity, schema, identities, coverage, shapes, or visual bits are invalid.</exception>
    /// <exception cref="IOException">Required stock or selected override files cannot be read.</exception>
    public static RoomPlmElevatorPlatformVisualCatalog Load(
        string stockDirectory, string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, ManifestFileName);
        VisualManifest manifest = ReadJson<VisualManifest>(manifestPath);
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Elevator-platform visual manifest {manifestPath} is incompatible.");
        string stockPath = Path.Combine(stockDirectory, VisualFileName);
        byte[] stockBytes = File.ReadAllBytes(stockPath);
        if (!string.Equals(Convert.ToHexString(SHA256.HashData(stockBytes)),
                manifest.VisualSha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Stock elevator-platform visuals {stockPath} failed their manifest hash.");
        RoomPlmElevatorPlatformVisualCatalog stock = CreateCatalog(
            ReadJson<VisualDocument>(stockBytes, stockPath), stockPath);
        VerifyStockMatchesCompiled(stock, stockPath);

        string? overridePath = overrideDirectory is null ? null :
            Path.Combine(overrideDirectory, VisualFileName);
        return overridePath is null || !File.Exists(overridePath)
            ? stock
            : CreateCatalog(ReadJson<VisualDocument>(overridePath), overridePath);
    }

    /// <summary>Performs the stock checks from <see cref="Load"/> without overrides, cartridge reads, or file writes.</summary>
    /// <param name="directory">Elevator-platform family directory containing stock JSON and manifest.</param>
    /// <exception cref="InvalidDataException">Stock provenance, integrity, schema, or compiled appearance checks fail.</exception>
    public static void ValidateStock(string directory) => _ = Load(directory, null);

    /// <summary>Verifies every compiled elevator-platform draw record and extracts its editable visual words.</summary>
    /// <param name="bus">Cartridge address space containing the bank-$84 platform draws.</param>
    /// <returns>Named entries preserving every compiled run and word position.</returns>
    private static RoomPlmElevatorPlatformVisualEntry[] ReadAndVerifyNative(ISnesAddressSpace bus)
    {
        var entries = new List<RoomPlmElevatorPlatformVisualEntry>();
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in
                 ElevatorPlatformPlmDefinitions.DrawLists.OrderBy(item => item.Pointer))
        {
            ushort cursor = list.Pointer;
            var visuals = new List<ushort[]>();
            foreach (RoomPlmShotBlockDrawDefinitions.Run run in list.Runs.Span)
            {
                if (ReadWord(bus, cursor) != run.DirectionAndCount)
                    throw new InvalidDataException(
                        $"Elevator-platform draw list ${list.Pointer:X4} differs in source cartridge shape.");
                cursor = unchecked((ushort)(cursor + 2));
                var visualWords = new ushort[run.LevelWords.Length];
                for (int index = 0; index < visualWords.Length; index++)
                {
                    ushort source = ReadWord(bus, cursor);
                    if (source != run.LevelWords.Span[index])
                        throw new InvalidDataException(
                            $"Elevator-platform draw list ${list.Pointer:X4} differs at ${cursor:X4}.");
                    visualWords[index] = new RoomLevelWord(source).VisualWord;
                    cursor = unchecked((ushort)(cursor + 2));
                }

                if (bus.ReadCartridgeByte(0x840000 | cursor) != unchecked((byte)run.NextX) ||
                    bus.ReadCartridgeByte(0x840000 | unchecked((ushort)(cursor + 1))) !=
                        unchecked((byte)run.NextY))
                    throw new InvalidDataException(
                        $"Elevator-platform draw list ${list.Pointer:X4} differs in next-record offset.");
                cursor = unchecked((ushort)(cursor + 2));
                visuals.Add(visualWords);
            }

            entries.Add(new RoomPlmElevatorPlatformVisualEntry(
                ElevatorPlatformPlmDefinitions.VisualId(list.Pointer), visuals.ToArray()));
        }

        return entries.ToArray();
    }

    /// <summary>Requires stock visual words to equal the visual portion of every compiled platform word.</summary>
    /// <param name="catalog">Validated stock catalog to compare.</param>
    /// <param name="path">Stock filename included in mismatch errors.</param>
    private static void VerifyStockMatchesCompiled(RoomPlmElevatorPlatformVisualCatalog catalog,
        string path)
    {
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in
                 ElevatorPlatformPlmDefinitions.DrawLists)
        for (int run = 0; run < list.Runs.Length; run++)
        for (int word = 0; word < list.Runs.Span[run].LevelWords.Length; word++)
        {
            ushort expected = new RoomLevelWord(
                list.Runs.Span[run].LevelWords.Span[word]).VisualWord;
            if (catalog.GetWord(list.Pointer, run, word) != expected)
                throw new InvalidDataException(
                    $"Stock elevator-platform visuals {path} differ from compiled frame " +
                    ElevatorPlatformPlmDefinitions.VisualId(list.Pointer) + ".");
        }
    }

    /// <summary>Validates a decoded elevator-platform document and constructs its immutable catalog.</summary>
    /// <param name="document">Decoded document with versioned entries.</param>
    /// <param name="path">Source filename included in validation errors.</param>
    /// <returns>The validated catalog.</returns>
    private static RoomPlmElevatorPlatformVisualCatalog CreateCatalog(
        VisualDocument document, string path)
    {
        if (document.Version != FormatVersion || document.Entries is null)
            throw new InvalidDataException($"Elevator-platform visuals {path} have an incompatible format.");
        try { return new RoomPlmElevatorPlatformVisualCatalog(document.Entries); }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid elevator-platform visuals {path}: {error.Message}", error);
        }
    }

    /// <summary>Reads and decodes a required elevator-platform JSON file.</summary>
    /// <typeparam name="T">Document type to deserialize.</typeparam>
    /// <param name="path">File to read and identify in errors.</param>
    /// <returns>The decoded non-null document.</returns>
    private static T ReadJson<T>(string path) => ReadJson<T>(File.ReadAllBytes(path), path);
    /// <summary>Decodes elevator-platform JSON bytes and translates malformed or empty content into data errors.</summary>
    /// <typeparam name="T">Document type to deserialize.</typeparam>
    /// <param name="bytes">UTF-8 JSON payload.</param>
    /// <param name="path">Logical source filename included in errors.</param>
    /// <returns>The decoded non-null document.</returns>
    private static T ReadJson<T>(byte[] bytes, string path)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(bytes, JsonOptions)
                ?? throw new InvalidDataException($"Elevator-platform JSON {path} is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid elevator-platform JSON {path}.", error);
        }
    }

    /// <summary>Reads a little-endian word from a bank-$84 draw-list offset.</summary>
    /// <param name="bus">Cartridge address space to read.</param>
    /// <param name="pointer">Sixteen-bit bank-relative address.</param>
    /// <returns>The decoded word.</returns>
    private static ushort ReadWord(ISnesAddressSpace bus, ushort pointer) =>
        unchecked((ushort)(bus.ReadCartridgeByte(0x840000 | pointer) |
            bus.ReadCartridgeByte(0x840000 | unchecked((ushort)(pointer + 1))) << 8));

    /// <summary>Records the schema, source cartridge identity, and stock visual payload hash.</summary>
    /// <param name="Version">Manifest schema version.</param>
    /// <param name="SourceCartridgeSha256">SHA-256 identity of the extraction cartridge.</param>
    /// <param name="VisualSha256">SHA-256 of the exact stock visual JSON bytes.</param>
    private sealed record VisualManifest(int Version, string SourceCartridgeSha256,
        string VisualSha256);
    /// <summary>Represents the complete editable elevator-platform visual document.</summary>
    /// <param name="Version">Visual document schema version.</param>
    /// <param name="Entries">Named entries for all three compiled platform frames.</param>
    private sealed record VisualDocument(int Version, RoomPlmElevatorPlatformVisualEntry[] Entries);
}

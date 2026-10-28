using SuperMetroid.Core;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Installs named gate-block appearances. Native draw geometry and physical level
/// words are checked against the compiled cartridge definitions.
/// </summary>
public static class RoomPlmDownwardGateVisualFiles
{
    /// <summary>Family-relative stock/override JSON filename for six downward-gate column frames and eight left/right trigger layouts.</summary>
    public const string VisualFileName = "downward-gates.json";
    /// <summary>Stock provenance manifest filename; records format version one, cartridge SHA-256, and the visual JSON's byte hash.</summary>
    public const string ManifestFileName = "manifest.json";
    /// <summary>Schema revision shared by the exported visual document and stock provenance manifest.</summary>
    private const int FormatVersion = 1;

    /// <summary>Serializes family documents with the published camel-case JSON field names.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    /// <summary>Checks bank-$84 downward-gate draw shapes, physical words, and continuation offsets before exporting metatile/flip words in native run order.</summary>
    /// <param name="bus">Cartridge source for column draws $84:A517–A56A and trigger draws $84:A5D7–A626; upward gates are not included.</param>
    /// <param name="directory">Family directory, created before native validation; receives new visual JSON and manifest files.</param>
    /// <param name="sourceCartridgeSha256">Nonblank caller-supplied provenance hash, recorded without recomputation; stock loading requires the supported cartridge identity.</param>
    /// <remarks>Words contain only bits 0..11, not collision types. Create-new outputs are separate writes and are not rolled back together on failure.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory or source hash is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">Native draw data differs from the compiled gate definitions.</exception>
    /// <exception cref="IOException">An output exists or filesystem access fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);
        RoomPlmVisualRunEntry[] entries = ReadAndVerifyNative(bus);
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

    /// <summary>Validates stock downward-gate artwork and selects an optional complete fourteen-layout visual replacement.</summary>
    /// <param name="stockDirectory">Family directory containing version-one stock JSON and its required manifest.</param>
    /// <param name="overrideDirectory">Optional family directory; null or an absent visual JSON selects validated stock.</param>
    /// <returns>A ROM-independent catalog with copied authored words and compiled collision, draw placement, and gate timing unchanged.</returns>
    /// <remarks>Stock provenance, file hash, and compiled visual equality are checked before overrides. Overrides require every published identity and original run shape with twelve-bit visual words, but no manifest; legacy trigger color labels remain unchanged.</remarks>
    /// <exception cref="ArgumentException"><paramref name="stockDirectory"/> is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">Provenance, integrity, schema, identities, frame coverage, shapes, or visual bits are invalid.</exception>
    /// <exception cref="IOException">Required stock or selected override files cannot be read.</exception>
    public static InstalledRoomPlmDownwardGateVisuals Load(
        string stockDirectory, string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, ManifestFileName);
        VisualManifest manifest = ReadJson<VisualManifest>(manifestPath);
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Downward gate visual manifest {manifestPath} is incompatible.");
        string stockPath = Path.Combine(stockDirectory, VisualFileName);
        byte[] stockBytes = File.ReadAllBytes(stockPath);
        if (!string.Equals(Convert.ToHexString(SHA256.HashData(stockBytes)),
                manifest.VisualSha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Stock downward gate visuals {stockPath} failed their manifest hash.");
        InstalledRoomPlmDownwardGateVisuals stock = CreateCatalog(
            ReadJson<VisualDocument>(stockBytes, stockPath), stockPath);
        VerifyStockMatchesCompiled(stock, stockPath);

        string? overridePath = overrideDirectory is null ? null :
            Path.Combine(overrideDirectory, VisualFileName);
        return overridePath is null || !File.Exists(overridePath)
            ? stock
            : CreateCatalog(ReadJson<VisualDocument>(overridePath), overridePath);
    }

    /// <summary>Runs stock manifest, schema, integrity, and compiled-appearance checks through <see cref="Load"/> without overrides, cartridge reads, or writes.</summary>
    /// <param name="directory">Downward-gate family directory containing both required stock files.</param>
    /// <exception cref="InvalidDataException">Stock validation fails.</exception>
    public static void ValidateStock(string directory) => _ = Load(directory, null);

    /// <summary>Reads each downward-gate draw list, verifying native shape and level words before retaining only visual bits.</summary>
    private static RoomPlmVisualRunEntry[] ReadAndVerifyNative(ISnesAddressSpace bus)
    {
        var entries = new List<RoomPlmVisualRunEntry>();
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in
                 DownwardGatePlmDraws.All.OrderBy(item => PlmDrawAddressCodec.Of(item.Draw)))
        {
            int cursor = PlmDrawAddressCodec.Of(list.Draw);
            var visuals = new List<ushort[]>();
            foreach (RoomPlmShotBlockDrawDefinitions.Run run in list.Runs.Span)
            {
                if (CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(CartridgeBankWindow.At(RoomPlmSourceDefinitions.PlmBank, cursor)) != PlmRunHeaderCodec.Encode(run))
                    throw new InvalidDataException(
                        $"Downward gate draw list {list.Draw} differs in source cartridge shape.");
                cursor = unchecked((ushort)(cursor + 2));
                var visualWords = new ushort[run.LevelWords.Length];
                for (int index = 0; index < visualWords.Length; index++)
                {
                    int source = CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(CartridgeBankWindow.At(RoomPlmSourceDefinitions.PlmBank, cursor));
                    if (source != run.LevelWords.Span[index])
                        throw new InvalidDataException(
                            $"Downward gate draw list {list.Draw} differs at ${cursor:X4}.");
                    visualWords[index] = new RoomLevelWord(checked((ushort)source)).VisualWord;
                    cursor = unchecked((ushort)(cursor + 2));
                }

                if (bus.ReadCartridgeByte(CartridgeBankWindow.At(RoomPlmSourceDefinitions.PlmBank, cursor)) != unchecked((byte)run.Next.X) ||
                    bus.ReadCartridgeByte(CartridgeBankWindow.At(RoomPlmSourceDefinitions.PlmBank, unchecked((ushort)(cursor + 1)))) !=
                        unchecked((byte)run.Next.Y))
                    throw new InvalidDataException(
                        $"Downward gate draw list {list.Draw} differs in next-record offset.");
                cursor = unchecked((ushort)(cursor + 2));
                visuals.Add(visualWords);
            }

            entries.Add(new RoomPlmVisualRunEntry(
                DownwardGatePlmDraws.VisualId(list.Draw), visuals.ToArray()));
        }

        return entries.ToArray();
    }

    /// <summary>Rejects stock artwork whose selected visual words differ from the compiled cartridge definitions.</summary>
    private static void VerifyStockMatchesCompiled(InstalledRoomPlmDownwardGateVisuals catalog,
        string path)
    {
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in
                 DownwardGatePlmDraws.All)
        for (int run = 0; run < list.Runs.Length; run++)
        for (int word = 0; word < list.Runs.Span[run].LevelWords.Length; word++)
        {
            int expected = new RoomLevelWord(
                list.Runs.Span[run].LevelWords.Span[word]).VisualWord;
            if (catalog.GetWord(list.Draw, run, word) != expected)
                throw new InvalidDataException(
                    $"Stock downward gate visuals {path} differ from compiled frame " +
                    DownwardGatePlmDraws.VisualId(list.Draw) + ".");
        }
    }

    /// <summary>Checks document revision and wraps entry validation errors with the source path.</summary>
    private static InstalledRoomPlmDownwardGateVisuals CreateCatalog(
        VisualDocument document, string path)
    {
        if (document.Version != FormatVersion || document.Entries is null)
            throw new InvalidDataException($"Downward gate visuals {path} have an incompatible format.");
        try { return new InstalledRoomPlmDownwardGateVisuals(document.Entries); }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid downward gate visuals {path}: {error.Message}", error);
        }
    }

    /// <summary>Reads a family JSON file and reports failures using its path.</summary>
    private static T ReadJson<T>(string path) => ReadJson<T>(File.ReadAllBytes(path), path);
    /// <summary>Deserializes a nonempty family document, converting malformed JSON to a path-specific data error.</summary>
    private static T ReadJson<T>(byte[] bytes, string path)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(bytes, JsonOptions)
                ?? throw new InvalidDataException($"Downward gate JSON {path} is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid downward gate JSON {path}.", error);
        }
    }

    /// <summary>Stock provenance and byte-integrity record for the downward-gate visual document.</summary>
    private sealed record VisualManifest(int Version, string SourceCartridgeSha256,
        string VisualSha256);
    /// <summary>Versioned set of named downward-gate visual runs stored in JSON.</summary>
    private sealed record VisualDocument(int Version, RoomPlmVisualRunEntry[] Entries);
}

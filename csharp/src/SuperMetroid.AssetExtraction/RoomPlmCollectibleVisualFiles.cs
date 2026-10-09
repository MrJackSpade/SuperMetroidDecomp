using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Installs named collectible block art while checking all 24 native draw records
/// and the eight dynamic-slot selectors against compiled physical definitions.
/// </summary>
public static class RoomPlmCollectibleVisualFiles
{
    /// <summary>Editable JSON filename containing the complete twenty-four-frame collectible visual set.</summary>
    public const string VisualFileName = "collectibles.json";
    /// <summary>Stock provenance manifest filename containing the format, source-cartridge identity, and visual-file digest.</summary>
    public const string ManifestFileName = "manifest.json";
    /// <summary>Current manifest and collectible visual-document schema version.</summary>
    private const int FormatVersion = 1;

    /// <summary>Camel-case JSON settings shared by stock provenance and editable visual entries.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    /// <summary>Verifies the native collectible selectors/draw lists and creates a new stock visual JSON plus provenance manifest.</summary>
    /// <param name="bus">Validated supported-cartridge address space used only for extraction-time bank-$84 comparisons.</param>
    /// <param name="directory">Destination directory; files must not already exist.</param>
    /// <param name="sourceCartridgeSha256">SHA-256 identity recorded in the stock manifest.</param>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">A path or cartridge identity is blank.</exception>
    /// <exception cref="InvalidDataException">Native selectors, draw geometry, or level words differ from the compiled definitions.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);
        RoomPlmCollectibleVisualEntry[] entries = ReadAndVerifyNative(bus);
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

    /// <summary>Validates the stock manifest/digest and compiled stock parity, then selects an optional editable override without cartridge access.</summary>
    /// <param name="stockDirectory">Installed stock collectible directory containing the manifest and visual JSON.</param>
    /// <param name="overrideDirectory">Optional directory whose visual JSON replaces stock when present; its file is schema/coverage validated but is not required to match the stock digest.</param>
    /// <returns>The immutable selected collectible visual catalog.</returns>
    /// <exception cref="ArgumentException"><paramref name="stockDirectory"/> is blank.</exception>
    /// <exception cref="InvalidDataException">The manifest, stock provenance/digest/parity, or selected JSON is invalid.</exception>
    public static RoomPlmCollectibleVisualCatalog Load(
        string stockDirectory, string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, ManifestFileName);
        VisualManifest manifest = ReadJson<VisualManifest>(manifestPath);
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(
                $"Collectible visual manifest {manifestPath} is incompatible.");
        string stockPath = Path.Combine(stockDirectory, VisualFileName);
        byte[] stockBytes = File.ReadAllBytes(stockPath);
        if (!string.Equals(Convert.ToHexString(SHA256.HashData(stockBytes)),
                manifest.VisualSha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(
                $"Stock collectible visuals {stockPath} failed their manifest hash.");
        RoomPlmCollectibleVisualCatalog stock = CreateCatalog(
            ReadJson<VisualDocument>(stockBytes, stockPath), stockPath);
        VerifyStockMatchesCompiled(stock, stockPath);

        string? overridePath = overrideDirectory is null ? null :
            Path.Combine(overrideDirectory, VisualFileName);
        return overridePath is null || !File.Exists(overridePath)
            ? stock
            : CreateCatalog(ReadJson<VisualDocument>(overridePath), overridePath);
    }

    /// <summary>Validates the installed stock manifest, digest, schema, coverage, and equality with compiled collectible visuals.</summary>
    /// <param name="directory">Stock collectible directory; overrides are intentionally ignored.</param>
    public static void ValidateStock(string directory) => _ = Load(directory, null);

    /// <summary>Verifies all dynamic selectors and one-block native draw lists before extracting their twelve-bit visual words.</summary>
    /// <param name="bus">Import address space containing bank-$84 selector and draw-list data.</param>
    /// <returns>Every collectible visual frame in compiled identity order.</returns>
    private static RoomPlmCollectibleVisualEntry[] ReadAndVerifyNative(ISnesAddressSpace bus)
    {
        for (int slot = 0; slot < 4; slot++)
        for (int animation = 0; animation < 2; animation++)
        {
            ushort table = animation == 0
                ? RoomPlmCollectibleDrawDefinitions.DynamicFrame0Table
                : RoomPlmCollectibleDrawDefinitions.DynamicFrame1Table;
            ushort native = ReadWord(bus, checked((ushort)(table + slot * 2)));
            ushort compiled = RoomPlmCollectibleDrawDefinitions.VisibleFrame(
                InWorldCollectibleKind.Bombs, animation, slot);
            if (native != compiled)
                throw new InvalidDataException(
                    $"Dynamic collectible selector $84:{table + slot * 2:X4} differs from the cartridge.");
        }

        var entries = new List<RoomPlmCollectibleVisualEntry>();
        foreach (RoomPlmCollectibleDrawFrame frame in RoomPlmCollectibleDrawDefinitions.All)
        {
            if (ReadWord(bus, frame.Pointer) != 1 ||
                ReadWord(bus, checked((ushort)(frame.Pointer + 2))) != frame.LevelWord ||
                ReadWord(bus, checked((ushort)(frame.Pointer + 4))) != 0)
                throw new InvalidDataException(
                    $"Collectible draw list $84:{frame.Pointer:X4} differs from compiled geometry or level word.");
            entries.Add(new RoomPlmCollectibleVisualEntry(frame.Id,
                new RoomLevelWord(frame.LevelWord).VisualWord));
        }
        return entries.ToArray();
    }

    /// <summary>Ensures each validated stock visual equals the visual portion of its compiled physical level word.</summary>
    /// <param name="catalog">Compiled stock collectible catalog.</param>
    /// <param name="path">Stock visual filename included in failures.</param>
    private static void VerifyStockMatchesCompiled(RoomPlmCollectibleVisualCatalog catalog,
        string path)
    {
        foreach (RoomPlmCollectibleDrawFrame frame in RoomPlmCollectibleDrawDefinitions.All)
            if (catalog.GetWord(frame.Pointer) !=
                new RoomLevelWord(frame.LevelWord).VisualWord)
                throw new InvalidDataException(
                    $"Stock collectible visuals {path} differ from compiled frame {frame.Id}.");
    }

    /// <summary>Checks document compatibility, compiles complete entry coverage, and adds the filename to admission errors.</summary>
    /// <param name="document">Deserialized collectible visual document.</param>
    /// <param name="path">Source filename included in failures.</param>
    /// <returns>The immutable collectible visual catalog.</returns>
    private static RoomPlmCollectibleVisualCatalog CreateCatalog(
        VisualDocument document, string path)
    {
        if (document.Version != FormatVersion || document.Entries is null)
            throw new InvalidDataException(
                $"Collectible visuals {path} have an incompatible format.");
        try { return new RoomPlmCollectibleVisualCatalog(document.Entries); }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid collectible visuals {path}: {error.Message}", error);
        }
    }

    /// <summary>Reads and deserializes a required collectible JSON file.</summary>
    /// <typeparam name="T">Expected manifest or visual-document type.</typeparam>
    /// <param name="path">File to read.</param>
    /// <returns>The non-null deserialized document.</returns>
    private static T ReadJson<T>(string path) => ReadJson<T>(File.ReadAllBytes(path), path);

    /// <summary>Deserializes collectible JSON bytes and normalizes syntax/null failures as invalid asset data.</summary>
    /// <typeparam name="T">Expected manifest or visual-document type.</typeparam>
    /// <param name="bytes">Complete UTF-8 JSON bytes.</param>
    /// <param name="path">Logical source filename included in failures.</param>
    /// <returns>The non-null deserialized document.</returns>
    private static T ReadJson<T>(byte[] bytes, string path)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(bytes, JsonOptions)
                ?? throw new InvalidDataException($"Collectible JSON {path} is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid collectible JSON {path}.", error);
        }
    }

    /// <summary>Reads one little-endian word from a bank-$84 collectible pointer.</summary>
    /// <param name="bus">Import address space containing the native data.</param>
    /// <param name="pointer">Sixteen-bit bank-relative word pointer.</param>
    /// <returns>The native word.</returns>
    private static ushort ReadWord(ISnesAddressSpace bus, ushort pointer) =>
        unchecked((ushort)(bus.ReadCartridgeByte(0x840000 | pointer) |
            bus.ReadCartridgeByte(0x840000 | unchecked((ushort)(pointer + 1))) << 8));

    /// <summary>Stock provenance and exact collectible visual-document byte identity.</summary>
    /// <param name="Version">Manifest schema version.</param>
    /// <param name="SourceCartridgeSha256">Cartridge identity used for extraction.</param>
    /// <param name="VisualSha256">Uppercase SHA-256 of the stock visual JSON bytes.</param>
    private sealed record VisualManifest(int Version, string SourceCartridgeSha256,
        string VisualSha256);
    /// <summary>Versioned complete editable collectible visual entry collection.</summary>
    /// <param name="Version">Visual-document schema version.</param>
    /// <param name="Entries">All twenty-four collectible frames and visual words.</param>
    private sealed record VisualDocument(int Version,
        RoomPlmCollectibleVisualEntry[] Entries);
}

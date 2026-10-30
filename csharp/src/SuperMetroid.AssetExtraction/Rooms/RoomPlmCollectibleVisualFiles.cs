using System.Text.Json.Serialization;
using SuperMetroid.Core;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Rooms;

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
    /// <summary>Schema revision required by stock collectible visuals and their manifest.</summary>
    private const int FormatVersion = 1;

    /// <summary>Reads and writes collectible documents with stable camel-case JSON names.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>Verifies the native collectible selectors/draw lists and creates a new stock visual JSON plus provenance manifest.</summary>
    /// <param name="bus">Validated supported-cartridge address space used only for extraction-time bank-$84 comparisons.</param>
    /// <param name="directory">Destination directory; files must not already exist.</param>
    /// <param name="sourceCartridgeSha256">SHA-256 identity recorded in the stock manifest.</param>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">A path or cartridge identity is blank.</exception>
    /// <exception cref="InvalidDataException">Native selectors, draw geometry, or block differ from the compiled definitions.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);
        RoomPlmVisualBlockEntry[] entries = ReadAndVerifyNative(bus);
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
    public static InstalledRoomPlmCollectibleVisuals Load(
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
        InstalledRoomPlmCollectibleVisuals stock = CreateCatalog(
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

    /// <summary>Checks eight dynamic selectors and all 24 one-cell native draws before extracting appearance.</summary>
    private static RoomPlmVisualBlockEntry[] ReadAndVerifyNative(ISnesAddressSpace bus)
    {
        for (int slot = 0; slot < 4; slot++)
        for (int animation = 0; animation < 2; animation++)
        {
            int table = animation == 0
                ? PlmDrawSourceDefinitions.CollectibleDynamicFrame0Table.Offset
                : PlmDrawSourceDefinitions.CollectibleDynamicFrame1Table.Offset;
            int native = CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(CartridgeBankWindow.At(RoomPlmSourceDefinitions.PlmBank, (checked((ushort)(table + slot * 2)))));
            int compiled = PlmDrawAddressCodec.Of(CollectiblePlmDraws.VisibleFrame(
                InWorldCollectibleKind.Bombs, animation, slot));
            if (native != compiled)
                throw new InvalidDataException(
                    $"Dynamic collectible selector $84:{table + slot * 2:X4} differs from the cartridge.");
        }

        var entries = new List<RoomPlmVisualBlockEntry>();
        foreach (RoomPlmCollectibleDrawFrame frame in CollectiblePlmDraws.All)
        {
            int address = PlmDrawAddressCodec.Of(PlmDraw.Of(frame.Draw));
            if (CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(CartridgeBankWindow.At(RoomPlmSourceDefinitions.PlmBank, address)) != 1 ||
                RoomLevelWordCodec.Decode(CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(CartridgeBankWindow.At(RoomPlmSourceDefinitions.PlmBank, (checked((ushort)(address + 2)))))) != frame.Terrain ||
                CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(CartridgeBankWindow.At(RoomPlmSourceDefinitions.PlmBank, (checked((ushort)(address + 4))))) != 0)
                throw new InvalidDataException(
                    $"Collectible draw {frame.Id} at $84:{address:X4} differs from compiled geometry or level word.");
            entries.Add(new RoomPlmVisualBlockEntry(frame.Id,
                frame.Terrain.Appearance));
        }
        return entries.ToArray();
    }

    /// <summary>Checks that hash-validated stock appearance still match compiled cartridge definitions.</summary>
    private static void VerifyStockMatchesCompiled(InstalledRoomPlmCollectibleVisuals catalog,
        string path)
    {
        foreach (RoomPlmCollectibleDrawFrame frame in CollectiblePlmDraws.All)
            if (catalog.GetAppearance(PlmDraw.Of(frame.Draw)) !=
                frame.Terrain.Appearance)
                throw new InvalidDataException(
                    $"Stock collectible visuals {path} differ from compiled frame {frame.Id}.");
    }

    /// <summary>Validates document version and entries, attaching the source path to any catalog error.</summary>
    private static InstalledRoomPlmCollectibleVisuals CreateCatalog(
        VisualDocument document, string path)
    {
        if (document.Version != FormatVersion || document.Entries is null)
            throw new InvalidDataException(
                $"Collectible visuals {path} have an incompatible format.");
        try { return new InstalledRoomPlmCollectibleVisuals(document.Entries); }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid collectible visuals {path}: {error.Message}", error);
        }
    }

    /// <summary>Reads a collectible document from an installed JSON path.</summary>
    private static T ReadJson<T>(string path) => ReadJson<T>(File.ReadAllBytes(path), path);

    /// <summary>Deserializes a collectible document and identifies malformed or empty content by path.</summary>
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

    /// <summary>Records the supported cartridge revision and digest of the stock visual JSON.</summary>
    private sealed record VisualManifest(int Version, string SourceCartridgeSha256,
        string VisualSha256);
    /// <summary>Stores a versioned, complete set of named collectible visual references.</summary>
    private sealed record VisualDocument(int Version,
        RoomPlmVisualBlockEntry[] Entries);
}

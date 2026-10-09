using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Installs editable Grapple-block visual references while verifying every native
/// source draw record against the immutable collision/shape catalog.
/// </summary>
public static class RoomPlmGrappleBlockVisualFiles
{
    /// <summary>Family-relative stock/override JSON filename for the five initial, breakup, and blank breakable-Grapple frames.</summary>
    public const string VisualFileName = "grapple-blocks.json";
    /// <summary>Stock manifest filename recording format version one, cartridge provenance, and the visual JSON's SHA-256.</summary>
    public const string ManifestFileName = "manifest.json";
    /// <summary>Schema version accepted for Grapple-block visual documents and manifests.</summary>
    private const int FormatVersion = 1;

    /// <summary>Shared camel-case JSON settings used for all Grapple-block visual files.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    /// <summary>Checks every six-byte bank-$84 Grapple-block draw record and exports its pointer-keyed twelve-bit visual word.</summary>
    /// <param name="bus">Cartridge source for the five compiled one-cell draws, including full collision words and terminating offsets.</param>
    /// <param name="directory">Family directory, created before native validation; receives new visual JSON and manifest files.</param>
    /// <param name="sourceCartridgeSha256">Nonblank caller-supplied provenance hash, not recomputed here; stock load requires the supported identity.</param>
    /// <remarks>Only metatile/flip bits are editable. Existing files are not overwritten, and separate JSON/manifest writes can leave a partial installation.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory or source hash is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">Native draw data differs from compiled cartridge definitions.</exception>
    /// <exception cref="IOException">An output exists or filesystem access fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);
        RoomPlmGrappleBlockVisualEntry[] entries = ReadAndVerifyNative(bus);
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

    /// <summary>Validates the five stock Grapple-block frames before selecting an optional complete artwork replacement.</summary>
    /// <param name="stockDirectory">Family directory containing version-one stock JSON and its required manifest.</param>
    /// <param name="overrideDirectory">Optional family directory; null or missing visual JSON selects validated stock.</param>
    /// <returns>A ROM-independent pointer-to-visual-word catalog; Grapple collision, breakup timing, and one-cell geometry remain compiled.</returns>
    /// <remarks>Stock provenance, byte hash, and compiled visual equality are mandatory even with an override. Override JSON needs each compiled draw pointer exactly once and twelve-bit words, but no manifest.</remarks>
    /// <exception cref="ArgumentException"><paramref name="stockDirectory"/> is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">Provenance, integrity, document version, pointer coverage/uniqueness, or visual-only bits are invalid.</exception>
    /// <exception cref="IOException">Required stock or selected override files cannot be read.</exception>
    public static RoomPlmGrappleBlockVisualCatalog Load(
        string stockDirectory, string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, ManifestFileName);
        VisualManifest manifest = ReadJson<VisualManifest>(manifestPath);
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(
                $"Grapple-block visual manifest {manifestPath} is incompatible.");
        string stockPath = Path.Combine(stockDirectory, VisualFileName);
        byte[] stockBytes = File.ReadAllBytes(stockPath);
        if (!string.Equals(Convert.ToHexString(SHA256.HashData(stockBytes)),
                manifest.VisualSha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(
                $"Stock Grapple-block visuals {stockPath} failed their manifest hash.");
        RoomPlmGrappleBlockVisualCatalog stock = CreateCatalog(
            ReadJson<VisualDocument>(stockBytes, stockPath), stockPath);
        foreach (RoomPlmGrappleBlockDrawDefinitions.DrawList draw in
                 RoomPlmGrappleBlockDrawDefinitions.All)
        {
            ushort expected = new RoomLevelWord(draw.LevelWord).VisualWord;
            if (stock.GetWord(draw.Pointer) != expected)
                throw new InvalidDataException(
                    $"Stock Grapple-block visuals {stockPath} differ from ${draw.Pointer:X4}.");
        }

        string? overridePath = overrideDirectory is null ? null :
            Path.Combine(overrideDirectory, VisualFileName);
        return overridePath is null || !File.Exists(overridePath)
            ? stock
            : CreateCatalog(ReadJson<VisualDocument>(overridePath), overridePath);
    }

    /// <summary>Runs stock provenance, hash, schema, and compiled-appearance checks through <see cref="Load"/> without overrides, cartridge access, or writes.</summary>
    /// <param name="directory">Grapple-block family directory containing both required stock files.</param>
    /// <exception cref="InvalidDataException">Stock validation fails.</exception>
    public static void ValidateStock(string directory) => _ = Load(directory, null);

    /// <summary>Reads all compiled Grapple-block draws and rejects any cartridge whose complete one-cell records differ.</summary>
    /// <param name="bus">Cartridge address space containing the bank-$84 draw records.</param>
    /// <returns>Pointer-keyed editable visual words for every compiled Grapple-block frame.</returns>
    private static RoomPlmGrappleBlockVisualEntry[] ReadAndVerifyNative(ISnesAddressSpace bus)
    {
        var entries = new List<RoomPlmGrappleBlockVisualEntry>();
        foreach (RoomPlmGrappleBlockDrawDefinitions.DrawList draw in
                 RoomPlmGrappleBlockDrawDefinitions.All.OrderBy(item => item.Pointer))
        {
            ushort pointer = draw.Pointer;
            if (ReadWord(bus, pointer) !=
                    RoomPlmGrappleBlockDrawDefinitions.DrawList.DirectionAndCount ||
                ReadWord(bus, unchecked((ushort)(pointer + 2))) != draw.LevelWord ||
                bus.ReadCartridgeByte(0x840000 | unchecked((ushort)(pointer + 4))) !=
                    RoomPlmGrappleBlockDrawDefinitions.DrawList.NextX ||
                bus.ReadCartridgeByte(0x840000 | unchecked((ushort)(pointer + 5))) !=
                    RoomPlmGrappleBlockDrawDefinitions.DrawList.NextY)
                throw new InvalidDataException(
                    $"Grapple-block draw list ${pointer:X4} differs from compiled cartridge data.");
            entries.Add(new RoomPlmGrappleBlockVisualEntry(pointer,
                new RoomLevelWord(draw.LevelWord).VisualWord));
        }

        return entries.ToArray();
    }

    /// <summary>Validates one decoded visual document and turns it into an immutable catalog.</summary>
    /// <param name="document">Decoded document whose version and complete entry set are validated.</param>
    /// <param name="path">Source filename included in validation errors.</param>
    /// <returns>The validated pointer-to-visual-word catalog.</returns>
    private static RoomPlmGrappleBlockVisualCatalog CreateCatalog(
        VisualDocument document, string path)
    {
        if (document.Version != FormatVersion || document.Entries is null)
            throw new InvalidDataException(
                $"Grapple-block visuals {path} have an incompatible format.");
        try { return new RoomPlmGrappleBlockVisualCatalog(document.Entries); }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Grapple-block visuals {path}: {error.Message}", error);
        }
    }

    /// <summary>Reads and decodes a required Grapple-block JSON file.</summary>
    /// <typeparam name="T">Document type to deserialize.</typeparam>
    /// <param name="path">File to read and identify in errors.</param>
    /// <returns>The decoded non-null document.</returns>
    private static T ReadJson<T>(string path) => ReadJson<T>(File.ReadAllBytes(path), path);

    /// <summary>Decodes Grapple-block JSON bytes while translating malformed or empty content into data errors.</summary>
    /// <typeparam name="T">Document type to deserialize.</typeparam>
    /// <param name="bytes">UTF-8 JSON payload.</param>
    /// <param name="path">Logical source filename included in errors.</param>
    /// <returns>The decoded non-null document.</returns>
    private static T ReadJson<T>(byte[] bytes, string path)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(bytes, JsonOptions)
                ?? throw new InvalidDataException($"Grapple-block JSON {path} is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid Grapple-block JSON {path}.", error);
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
    /// <param name="SourceCartridgeSha256">SHA-256 identity of the cartridge used for extraction.</param>
    /// <param name="VisualSha256">SHA-256 of the exact stock visual JSON bytes.</param>
    private sealed record VisualManifest(int Version, string SourceCartridgeSha256,
        string VisualSha256);
    /// <summary>Represents the complete editable Grapple-block visual document.</summary>
    /// <param name="Version">Visual document schema version.</param>
    /// <param name="Entries">One entry for every compiled Grapple-block draw pointer.</param>
    private sealed record VisualDocument(int Version, RoomPlmGrappleBlockVisualEntry[] Entries);
}

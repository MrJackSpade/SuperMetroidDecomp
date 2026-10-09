using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts the two authored direct small-OBJ atmospheric lists as editable JSON.</summary>
public static class SamusAtmosphericArtworkFiles
{
    /// <summary>Stock/override JSON filename for the four-frame footstep list and shared four-frame lava/dust direct-OBJ list.</summary>
    public const string ArtworkFileName = "samus-atmosphere.json";
    /// <summary>Stock manifest filename recording version-one format, cartridge provenance, and the atmospheric JSON's SHA-256.</summary>
    public const string ManifestFileName = "samus-atmosphere-manifest.json";
    /// <summary>Schema version for the extracted atmospheric attribute lists.</summary>
    private const int FormatVersion = 1;
    /// <summary>Strict camel-case JSON settings for atmospheric stock and override documents.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectRequiredConstructorParameters = true,
        WriteIndented = true,
    };

    /// <summary>Checks the native atmospheric pointer bindings and exports the two authored four-word direct small-OBJ attribute lists.</summary>
    /// <param name="bus">Cartridge source for type-one footsteps at $90:8C0F and the shared type-4/6/7 lava/dust list at $90:8C17.</param>
    /// <param name="directory">Artwork directory, created if absent; existing artwork JSON and companion manifest are overwritten.</param>
    /// <param name="sourceCartridgeSha256">Caller-supplied provenance recorded verbatim; the post-write load requires the supported cartridge hash.</param>
    /// <remarks>Type two must keep its intentional zero pointer and mutable-memory path; it is not exported. Packed words retain tile, palette, priority, and flips. Separate writes are not rolled back on later validation failure.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">Native pointer bindings, provenance, or generated list structure are incompatible.</exception>
    /// <exception cref="IOException">Filesystem output or post-write input fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        Directory.CreateDirectory(directory);
        int table = SamusMovementRomData.Environment.AtmosphericSpriteAttributeListPointers;
        // Only nonzero, authored pointers become extracted assets. Type two's zero
        // pointer is an intentional mutable-memory read, not a missing art record.
        AssertPointer(bus, table, 1, SamusMovementRomData.Environment.TypeOneAtmosphericAttributes);
        AssertPointer(bus, table, 2, 0);
        foreach (int type in new[] { 4, 6, 7 })
            AssertPointer(bus, table, type,
                SamusMovementRomData.Environment.SharedAtmosphericAttributes);
        var document = new ArtworkDocument(FormatVersion,
            ReadList(bus, SamusMovementRomData.Environment.TypeOneAtmosphericAttributes),
            ReadList(bus, SamusMovementRomData.Environment.SharedAtmosphericAttributes));
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        File.WriteAllBytes(Path.Combine(directory, ArtworkFileName), bytes);
        File.WriteAllBytes(Path.Combine(directory, ManifestFileName),
            JsonSerializer.SerializeToUtf8Bytes(new ArtworkManifest(FormatVersion,
                sourceCartridgeSha256, Convert.ToHexString(SHA256.HashData(bytes))), JsonOptions));
        _ = Load(directory, null);
    }

    /// <summary>Validates stock atmospheric attributes before selecting an optional complete two-list JSON replacement.</summary>
    /// <param name="stockDirectory">Artwork directory containing the required version-one JSON and pinned-cartridge manifest.</param>
    /// <param name="overrideDirectory">Optional artwork directory; null or absent artwork JSON uses validated stock.</param>
    /// <returns>A ROM-independent catalog that captures both lists without retaining deserialized caller arrays.</returns>
    /// <remarks>Stock provenance, byte hash, and strict schema/list-length admission are checked even with overrides. Replacement JSON needs both four-word lists but no manifest; packed attribute values themselves are unrestricted, and type-two/animation mechanics are unchanged.</remarks>
    /// <exception cref="InvalidDataException">Provenance, hash, strict JSON, version, or list dimensions are invalid; admission errors include the source filename.</exception>
    /// <exception cref="IOException">A required stock or selected override file cannot be read.</exception>
    public static SamusAtmosphericArtworkCatalog Load(string stockDirectory, string? overrideDirectory)
    {
        SamusArtworkFile manifestFile = SamusArtworkFile.Read(Path.Combine(stockDirectory, ManifestFileName));
        ArtworkManifest manifest = manifestFile.Json<ArtworkManifest>(JsonOptions);
        return manifestFile.WithContext(() => LoadArtwork());

        SamusAtmosphericArtworkCatalog LoadArtwork()
        {
            if (manifest.Version != FormatVersion ||
                !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Samus atmospheric artwork manifest does not match the pinned cartridge.");
            SamusArtworkFile stock = SamusArtworkFile.Stock(Path.Combine(stockDirectory, ArtworkFileName), manifest.ArtworkSha256);
            _ = stock.WithContext(() => Build(stock.Json<ArtworkDocument>(JsonOptions), stock.Path));
            SamusArtworkFile selected = stock.Select(overrideDirectory);
            return selected.WithContext(() => Build(selected.Json<ArtworkDocument>(JsonOptions), selected.Path));
        }
    }

    /// <summary>Validates the two extracted attribute lists and creates the runtime catalog.</summary>
    /// <param name="document">Parsed atmospheric artwork lists.</param>
    /// <param name="source">Path used to identify invalid document data.</param>
    /// <returns>A catalog containing the type-one and shared type-four lists.</returns>
    private static SamusAtmosphericArtworkCatalog Build(ArtworkDocument document, string source)
    {
        if (document.Version != FormatVersion || document.TypeOne is null ||
            document.SharedTypeFour is null)
            throw new InvalidDataException($"Samus atmospheric artwork {source} has invalid structure.");
        return new SamusAtmosphericArtworkCatalog(document.TypeOne, document.SharedTypeFour);
    }

    /// <summary>Reads the fixed number of packed direct-OBJ attributes at a native pointer.</summary>
    /// <param name="bus">Cartridge address space supplying the bank-$90 words.</param>
    /// <param name="pointer">Bank-local start of the attribute list.</param>
    /// <returns>The packed attributes in frame order.</returns>
    private static ushort[] ReadList(ISnesAddressSpace bus, ushort pointer)
    {
        var words = new ushort[SamusMovementRomData.Environment.DirectAtmosphericFrameCount];
        for (int frame = 0; frame < words.Length; frame++)
            words[frame] = ReadWord(bus, SamusMovementRomData.Banks.Movement | pointer + frame * 2);
        return words;
    }

    /// <summary>Confirms one atmospheric type remains bound to its known native pointer.</summary>
    /// <param name="bus">Cartridge address space containing the pointer table.</param>
    /// <param name="table">Address of the table's first pointer.</param>
    /// <param name="type">Atmospheric type index to check.</param>
    /// <param name="expected">Required pointer value, including intentional zero entries.</param>
    private static void AssertPointer(ISnesAddressSpace bus, int table, int type, ushort expected)
    {
        ushort actual = ReadWord(bus, table + type * 2);
        if (actual != expected)
            throw new InvalidDataException(
                $"Atmospheric type {type} points to ${actual:X4}, expected ${expected:X4}.");
    }

    /// <summary>Reads a little-endian cartridge word.</summary>
    /// <param name="bus">Cartridge address space supplying the bytes.</param>
    /// <param name="address">Address of the low byte.</param>
    /// <returns>The decoded 16-bit word.</returns>
    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        (ushort)(bus.ReadCartridgeByte(address) | bus.ReadCartridgeByte(address + 1) << 8);

    /// <summary>Editable packed attribute lists for type one and the shared type-four family.</summary>
    /// <param name="Version">Atmospheric JSON schema version.</param>
    /// <param name="TypeOne">Four native direct-OBJ attributes for the type-one list.</param>
    /// <param name="SharedTypeFour">Four attributes shared by atmospheric types four, six, and seven.</param>
    private sealed record ArtworkDocument(int Version, ushort[] TypeOne, ushort[] SharedTypeFour);

    /// <summary>Stock provenance and integrity metadata for the atmospheric artwork JSON.</summary>
    /// <param name="Version">Atmospheric JSON schema version.</param>
    /// <param name="SourceCartridgeSha256">SHA-256 of the extraction cartridge revision.</param>
    /// <param name="ArtworkSha256">SHA-256 of the stock artwork document bytes.</param>
    private sealed record ArtworkManifest(int Version, string SourceCartridgeSha256, string ArtworkSha256);
}

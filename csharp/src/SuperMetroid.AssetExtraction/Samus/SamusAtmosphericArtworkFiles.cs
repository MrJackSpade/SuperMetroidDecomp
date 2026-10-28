using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Extracts the two authored direct small-OBJ atmospheric lists as editable JSON.</summary>
public static class SamusAtmosphericArtworkFiles
{
    /// <summary>Stock/override JSON filename for the four-frame footstep list and shared four-frame lava/dust direct-OBJ list.</summary>
    public const string ArtworkFileName = "samus-atmosphere.json";
    /// <summary>Stock manifest filename recording version-one format, cartridge provenance, and the atmospheric JSON's SHA-256.</summary>
    public const string ManifestFileName = "samus-atmosphere-manifest.json";
    /// <summary>Schema revision shared by the extracted artwork and its provenance manifest; version 2 stores sprite parts.</summary>
    private const int FormatVersion = 2;
    /// <summary>Strict camel-case JSON contract for both the stock and replacement lists.</summary>
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
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        Directory.CreateDirectory(directory);
        int table = SamusSourceDefinitions.AtmosphericSpriteAttributeListPointers.Value;
        // Only nonzero, authored pointers become extracted assets. Type two's zero
        // pointer is an intentional mutable-memory read, not a missing art record.
        AssertPointer(bus, table, 1, SamusSourceDefinitions.TypeOneAtmosphericAttributes);
        AssertPointer(bus, table, 2, 0);
        foreach (int type in new[] { 4, 6, 7 })
            AssertPointer(bus, table, type,
                SamusSourceDefinitions.SharedAtmosphericAttributes);
        var document = new ArtworkDocument(FormatVersion,
            ReadList(bus, SamusSourceDefinitions.TypeOneAtmosphericAttributes),
            ReadList(bus, SamusSourceDefinitions.SharedAtmosphericAttributes));
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

    /// <summary>Requires both four-frame single-piece lists before installing a ROM-independent catalog.</summary>
    private static SamusAtmosphericArtworkCatalog Build(ArtworkDocument document, string source)
    {
        if (document.Version != FormatVersion || document.TypeOne is null ||
            document.SharedTypeFour is null)
            throw new InvalidDataException($"Samus atmospheric artwork {source} has invalid structure.");
        return new SamusAtmosphericArtworkCatalog(Pieces(document.TypeOne, source), Pieces(document.SharedTypeFour, source));
    }

    /// <summary>The stored single small pieces of one list, each at the sprite origin.</summary>
    private static SpritePart[] Pieces(SpriteVisualPart[] parts, string source) =>
        [.. parts.Select(part => part is { OffsetX: 0, OffsetY: 0, Size: 8 }
            ? part.ToSpritePart(SpriteArtwork.CharactersPerRow)
            : throw new InvalidDataException($"Samus atmospheric artwork {source} has a part that is not one small piece at the origin."))];

    /// <summary>Imports one native atmospheric list, decoding its four attribute words into single small pieces.</summary>
    private static SpriteVisualPart[] ReadList(ISnesAddressSpace bus, int pointer)
    {
        var parts = new SpriteVisualPart[SamusMovementDefinitions.Environment.DirectAtmosphericFrameCount];
        for (int frame = 0; frame < parts.Length; frame++)
            parts[frame] = SpriteVisualPart.FromSpritePart(new SnesObjAttributeWord(CartridgeImportSource.Require(bus).ReadCartridgeWord(
                SamusSourceDefinitions.MovementBank.Value | pointer + frame * 2)).ToSinglePart(), SpriteArtwork.CharactersPerRow);
        return parts;
    }

    /// <summary>Checks that each atmospheric type still selects its expected authored or zero list pointer.</summary>
    private static void AssertPointer(ISnesAddressSpace bus, int table, int type, int expected)
    {
        int actual = CartridgeImportSource.Require(bus).ReadCartridgeWord((table + type * 2));
        if (actual != expected)
            throw new InvalidDataException(
                $"Atmospheric type {type} points to ${actual:X4}, expected ${expected:X4}.");
    }

    /// <summary>Editable single small pieces for footsteps and the shared lava/dust effect.</summary>
    private sealed record ArtworkDocument(int Version, SpriteVisualPart[] TypeOne, SpriteVisualPart[] SharedTypeFour);
    /// <summary>Stock source revision and artwork-byte hash used before any override is selected.</summary>
    private sealed record ArtworkManifest(int Version, string SourceCartridgeSha256, string ArtworkSha256);
}

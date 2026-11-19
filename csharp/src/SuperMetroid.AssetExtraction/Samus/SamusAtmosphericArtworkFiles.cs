using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Extracts the two authored direct small-OBJ atmospheric lists as JSON into the Samus body component's import data
/// (#1638): their entries are packed sprite attributes only the atmosphere sheet is composed from, so the installation
/// holds the sheet's pictures and none of the lists.
/// </summary>
public static class SamusAtmosphericArtworkFiles
{
    /// <summary>Import-data JSON filename for the four-frame footstep list and shared four-frame lava/dust direct-OBJ list.</summary>
    public const string ArtworkFileName = "samus-atmosphere.json";
    /// <summary>Import-data manifest filename recording the format and cartridge provenance of the atmospheric JSON.</summary>
    public const string ManifestFileName = "samus-atmosphere-manifest.json";
    /// <summary>
    /// Schema revision shared by the extracted artwork and its provenance manifest; version 2 stores sprite parts, and
    /// version 3 is import data the installation never holds (#1638).
    /// </summary>
    private const int FormatVersion = 3;
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
    /// <param name="directory">The Samus body component's import-data directory, created if absent; existing artwork JSON and companion manifest are overwritten.</param>
    /// <param name="sourceCartridgeSha256">Caller-supplied provenance recorded verbatim.</param>
    /// <remarks>Type two must keep its intentional zero pointer and mutable-memory path; it is not exported. Packed words retain tile, palette, priority, and flips. Separate writes are not rolled back if a later write fails.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">Native pointer bindings are incompatible.</exception>
    /// <exception cref="IOException">Filesystem output fails.</exception>
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
                sourceCartridgeSha256), JsonOptions));
    }

    /// <summary>Validates the manifest's provenance and loads the atmospheric attributes the import composes the atmosphere sheet from (#1638).</summary>
    /// <param name="directory">The Samus body component's import-data directory, containing the JSON and its pinned-cartridge manifest.</param>
    /// <returns>A ROM-independent catalog that captures both lists without retaining deserialized caller arrays.</returns>
    /// <remarks>The JSON needs both four-word lists; packed attribute values themselves are unrestricted, and type-two/animation mechanics are unchanged.</remarks>
    /// <exception cref="InvalidDataException">Provenance, strict JSON, version, or list dimensions are invalid; admission errors include the source filename.</exception>
    /// <exception cref="IOException">A required file cannot be read.</exception>
    public static SamusAtmosphericSourceArtwork Load(string directory)
    {
        SamusArtworkFile manifestFile = SamusArtworkFile.Read(Path.Combine(directory, ManifestFileName));
        ArtworkManifest manifest = manifestFile.Json<ArtworkManifest>(JsonOptions);
        return manifestFile.WithContext(() => LoadArtwork());

        SamusAtmosphericSourceArtwork LoadArtwork()
        {
            if (manifest.Version != FormatVersion ||
                !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Samus atmospheric artwork manifest does not match the pinned cartridge.");
            SamusArtworkFile installed = SamusArtworkFile.Read(Path.Combine(directory, ArtworkFileName));
            return installed.WithContext(() => Build(installed.Json<ArtworkDocument>(JsonOptions), installed.Path));
        }
    }

    /// <summary>Requires both four-frame single-piece lists before installing a ROM-independent catalog.</summary>
    private static SamusAtmosphericSourceArtwork Build(ArtworkDocument document, string source)
    {
        if (document.Version != FormatVersion || document.TypeOne is null ||
            document.SharedTypeFour is null)
            throw new InvalidDataException($"Samus atmospheric artwork {source} has invalid structure.");
        return new SamusAtmosphericSourceArtwork(Pieces(document.TypeOne, source), Pieces(document.SharedTypeFour, source));
    }

    /// <summary>The stored single small pieces of one list, each at the sprite origin.</summary>
    private static SpritePart[] Pieces(SpriteVisualPart[] parts, string source) =>
        [.. parts.Select(part => part is { OffsetX: 0, OffsetY: 0, Size: 8 }
            ? part.ToSpritePart(SpriteArtwork.CharactersPerRow)
            : throw new InvalidDataException($"Samus atmospheric artwork {source} has a part that is not one small piece at the origin."))];

    /// <summary>Imports one native atmospheric list, decoding its four attribute words into single small pieces.</summary>
    private static SpriteVisualPart[] ReadList(ISnesAddressSpace bus, int pointer)
    {
        var parts = new SpriteVisualPart[SamusAtmosphericArtworkDefinitions.DirectAtmosphericFrameCount];
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

    /// <summary>The extracted single small pieces for footsteps and the shared lava/dust effect.</summary>
    private sealed record ArtworkDocument(int Version, SpriteVisualPart[] TypeOne, SpriteVisualPart[] SharedTypeFour);
    /// <summary>The extracted artwork's source revision.</summary>
    private sealed record ArtworkManifest(int Version, string SourceCartridgeSha256);
}

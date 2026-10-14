using SuperMetroid.Core.Rom;
using SuperMetroid.Core;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Death;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts the authored bank-$9B fatal-damage color and color-selection data.</summary>
[CpuBusBoundary("codec: ROM bank-$9B fatal-damage colors and selections → Samus death palette files")]
public static class SamusDeathPaletteArtworkFiles
{
    /// <summary>Stock/override JSON filename for suited/suitless death rows, whiteout shades, and explosion palette selectors.</summary>
    public const string ArtworkFileName = "samus-death-palettes.json";
    /// <summary>Stock manifest filename recording version-one format, cartridge provenance, and the death-palette JSON's SHA-256.</summary>
    public const string ManifestFileName = "samus-death-palettes-manifest.json";
    /// <summary>Schema revision shared by death-palette art and its stock manifest.</summary>
    private const int FormatVersion = 1;
    /// <summary>Strict camel-case JSON settings for both color artwork and provenance.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectRequiredConstructorParameters = true,
        WriteIndented = true,
    };

    /// <summary>Exports three ten-row suited families, ten suitless rows, twenty-two whiteout shades, and nine explosion palette selectors from bank $9B.</summary>
    /// <param name="bus">Cartridge source for native death palette pointer tables, sixteen-color payloads, ShadesOfWhite, and odd-byte explosion selectors.</param>
    /// <param name="directory">Artwork directory, created if absent; existing JSON and companion manifest are overwritten.</param>
    /// <param name="sourceCartridgeSha256">Caller-supplied provenance recorded verbatim; the post-write load requires the supported cartridge hash.</param>
    /// <remarks>Colors remain packed RGB5 words 0..$7FFF including row color zero; suited order is Power, Varia, Gravity. Selectors are editable palette choices, not adjacent timing bytes. Separate writes are not rolled back if post-write validation fails.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">Provenance, palette dimensions, RGB5 words, or explosion selector values are invalid.</exception>
    /// <exception cref="IOException">Filesystem output or post-write input fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        Directory.CreateDirectory(directory);
        var suited = new ushort[SamusDeathPaletteArtworkCatalog.SuitCount][][];
        for (int suit = 0; suit < suited.Length; suit++)
        {
            suited[suit] = new ushort[SamusPaletteRomData.Death.PaletteCount][];
            for (int palette = 0; palette < suited[suit].Length; palette++)
            {
                ushort pointer = CartridgeImportSource.Require(bus).ReadCartridgeWord((SamusPaletteRomData.Death.SuitPointers +
                    suit * SamusPaletteRomData.Death.PaletteCount * sizeof(ushort) +
                    palette * sizeof(ushort)));
                suited[suit][palette] = ReadPalette(bus, pointer);
            }
        }
        var suitless = new ushort[SamusPaletteRomData.Death.PaletteCount][];
        for (int palette = 0; palette < suitless.Length; palette++)
        {
            ushort pointer = CartridgeImportSource.Require(bus).ReadCartridgeWord((SamusPaletteRomData.Death.SuitlessPointers + palette * sizeof(ushort)));
            suitless[palette] = ReadPalette(bus, pointer);
        }
        var whiteout = new ushort[SamusPaletteRomData.Death.WhiteoutShadeCount];
        for (int shade = 0; shade < whiteout.Length; shade++)
            whiteout[shade] = CartridgeImportSource.Require(bus).ReadCartridgeWord((SamusPaletteRomData.Death.WhiteoutShades + shade * sizeof(ushort)));
        var selectors = new ushort[SamusDeathExplosionTimingDefinitions.RecordCount];
        for (int frame = 0; frame < selectors.Length; frame++)
            selectors[frame] = bus.ReadCartridgeByte(
                SamusPaletteRomData.Death.ExplosionTimingAndPaletteIndices + frame * 2 + 1);
        var document = new ArtworkDocument(FormatVersion, suited, suitless, whiteout, selectors);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        File.WriteAllBytes(Path.Combine(directory, ArtworkFileName), bytes);
        File.WriteAllBytes(Path.Combine(directory, ManifestFileName),
            JsonSerializer.SerializeToUtf8Bytes(new ArtworkManifest(FormatVersion,
                sourceCartridgeSha256, Convert.ToHexString(SHA256.HashData(bytes))), JsonOptions));
        _ = Load(directory, null);
    }

    /// <summary>Validates stock fatal-damage color/selector artwork before selecting an optional complete JSON replacement.</summary>
    /// <param name="stockDirectory">Artwork directory containing required version-one death-palette JSON and its provenance/hash manifest.</param>
    /// <param name="overrideDirectory">Optional artwork directory; null or absent artwork JSON uses validated stock.</param>
    /// <returns>A ROM-independent catalog of owned color inputs and selector choices, leaving death/explosion durations and suit-selection mechanics compiled.</returns>
    /// <remarks>Stock provenance, byte hash, and strict schema/catalog admission are mandatory even with overrides. Replacement JSON needs all original dimensions, packed colors at most $7FFF, and nine row selectors 0..9, but no manifest.</remarks>
    /// <exception cref="InvalidDataException">Provenance, hash, strict JSON, version, dimensions, color words, or selectors are invalid; admission errors identify the source filename.</exception>
    /// <exception cref="IOException">A required stock or selected override file cannot be read.</exception>
    public static SamusDeathPaletteArtworkCatalog Load(string stockDirectory, string? overrideDirectory)
    {
        SamusArtworkFile manifestFile = SamusArtworkFile.Read(Path.Combine(stockDirectory, ManifestFileName));
        ArtworkManifest manifest = manifestFile.Json<ArtworkManifest>(JsonOptions);
        return manifestFile.WithContext(() => LoadArtwork());

        SamusDeathPaletteArtworkCatalog LoadArtwork()
        {
            if (manifest.Version != FormatVersion ||
                !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Samus death-palette manifest does not match the pinned cartridge.");
            SamusArtworkFile stock = SamusArtworkFile.Stock(Path.Combine(stockDirectory, ArtworkFileName), manifest.ArtworkSha256);
            _ = stock.WithContext(() => Build(stock.Json<ArtworkDocument>(JsonOptions)));
            SamusArtworkFile selected = stock.Select(overrideDirectory);
            return selected.WithContext(() => Build(selected.Json<ArtworkDocument>(JsonOptions)));
        }
    }

    /// <summary>Validates death-palette document structure and converts packed RGB5 words into the installed catalog.</summary>
    private static SamusDeathPaletteArtworkCatalog Build(ArtworkDocument document)
    {
        if (document.Version != FormatVersion || document.Suited is null ||
            document.Suitless is null || document.Whiteout is null ||
            document.ExplosionPaletteIndices is null)
            throw new InvalidDataException("Samus death-palette artwork has an invalid structure.");
        return new SamusDeathPaletteArtworkCatalog(
            Array.ConvertAll(document.Suited, suit => Array.ConvertAll(
                suit ?? throw new InvalidDataException("Samus death-palette artwork is missing a suit."), Colors)),
            Array.ConvertAll(document.Suitless, Colors), Colors(document.Whiteout), document.ExplosionPaletteIndices);
    }

    /// <summary>The artwork file stores CGRAM words; each must be a fifteen-bit color.</summary>
    private static Bgr555[] Colors(ushort[]? words) => words is null
        ? throw new InvalidDataException("Samus death-palette artwork is missing a palette.")
        : Array.ConvertAll(words, word =>
        (word & 0x8000) == 0 ? Bgr555.FromWord(word)
            : throw new InvalidDataException($"Samus death-palette color ${word:X4} sets bit 15."));

    /// <summary>Imports all colors in one native sixteen-entry death palette row.</summary>
    private static ushort[] ReadPalette(ISnesAddressSpace bus, ushort pointer)
    {
        var colors = new ushort[SamusDeathPaletteArtworkCatalog.ColorCount];
        for (int color = 0; color < colors.Length; color++)
            colors[color] = CartridgeImportSource.Require(bus).ReadCartridgeWord((SamusPaletteRomData.Banks.Palette |
                unchecked((ushort)(pointer + color * sizeof(ushort)))));
        return colors;
    }

    /// <summary>Complete editable suited, suitless, whiteout, and explosion-selector payload.</summary>
    private sealed record ArtworkDocument(int Version, ushort[][][] Suited,
        ushort[][] Suitless, ushort[] Whiteout, ushort[] ExplosionPaletteIndices);
    /// <summary>Stock death-palette provenance and payload hash.</summary>
    private sealed record ArtworkManifest(int Version, string SourceCartridgeSha256, string ArtworkSha256);
}

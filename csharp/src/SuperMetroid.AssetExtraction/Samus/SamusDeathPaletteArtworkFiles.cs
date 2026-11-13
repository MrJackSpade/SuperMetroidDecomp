using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Extracts Samus's fatal-damage colours (#1637): her suited and suitless death colours only pre-colour her death shades, so
/// the death-palette document is import data, which the installer deletes before it publishes; what the game itself works
/// with, the death step each explosion frame shows and the colour the room whitens to at each whiteout step, is installed
/// by purpose in the death-step document, which the player edits in place.
/// </summary>
public static class SamusDeathPaletteArtworkFiles
{
    /// <summary>Import-data JSON filename for suited/suitless death rows, whiteout shades, and explosion death steps.</summary>
    public const string ArtworkFileName = "samus-death-palettes.json";
    /// <summary>Installed JSON filename for the explosion's death steps and the whiteout colours, named by purpose.</summary>
    public const string StepFileName = "samus-death-steps.json";
    /// <summary>Schema revision shared by the death documents; 2 installs the death steps by purpose (#1637).</summary>
    private const int FormatVersion = 2;
    /// <summary>Strict camel-case JSON settings for the colour artwork and the death steps.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectRequiredConstructorParameters = true,
        WriteIndented = true,
    };

    /// <summary>
    /// Exports three ten-row suited families, ten suitless rows, twenty-two whiteout shades, and nine explosion death steps
    /// from bank $9B: the complete document to <paramref name="importData"/>, the death steps and whiteout colours to
    /// <paramref name="directory"/>.
    /// </summary>
    /// <param name="bus">Cartridge source for native death palette pointer tables, sixteen-color payloads, ShadesOfWhite, and odd-byte explosion selectors.</param>
    /// <param name="directory">The component's directory, created if absent, receiving the death-step document and no manifest (#1693).</param>
    /// <param name="importData">The component's import-data directory, created if absent, receiving the death-palette document.</param>
    /// <remarks>Suited order is Power, Varia, Gravity. Separate writes are not rolled back if a later write fails.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">A directory is null, empty, or whitespace.</exception>
    /// <exception cref="IOException">Filesystem output fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory, string importData)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(importData);
        Directory.CreateDirectory(directory);
        Directory.CreateDirectory(importData);
        var suited = new ushort[InstalledSamusDeathPalettes.SuitCount][][];
        for (int suit = 0; suit < suited.Length; suit++)
        {
            suited[suit] = new ushort[SamusPaletteDefinitions.Death.PaletteCount][];
            for (int palette = 0; palette < suited[suit].Length; palette++)
            {
                int pointer = CartridgeImportSource.Require(bus).ReadCartridgeWord((SamusSourceDefinitions.SuitPointers.Value +
                    suit * SamusPaletteDefinitions.Death.PaletteCount * sizeof(ushort) +
                    palette * sizeof(ushort)));
                suited[suit][palette] = ReadPalette(bus, pointer);
            }
        }
        var suitless = new ushort[SamusPaletteDefinitions.Death.PaletteCount][];
        for (int palette = 0; palette < suitless.Length; palette++)
        {
            int pointer = CartridgeImportSource.Require(bus).ReadCartridgeWord((SamusSourceDefinitions.SuitlessPointers.Value + palette * sizeof(ushort)));
            suitless[palette] = ReadPalette(bus, pointer);
        }
        var whiteout = new ushort[SamusPaletteDefinitions.Death.WhiteoutShadeCount];
        for (int shade = 0; shade < whiteout.Length; shade++)
            whiteout[shade] = CartridgeImportSource.Require(bus).ReadCartridgeWord((SamusSourceDefinitions.DeathWhiteoutShades.Value + shade * sizeof(ushort)));
        var selectors = new ushort[SamusDeathExplosionTimingDefinitions.RecordCount];
        for (int frame = 0; frame < selectors.Length; frame++)
            selectors[frame] = bus.ReadCartridgeByte(
                SamusSourceDefinitions.DeathExplosionTimingAndPaletteIndices.Value + frame * 2 + 1);
        var document = new ArtworkDocument(FormatVersion, suited, suitless, whiteout, selectors);
        File.WriteAllBytes(Path.Combine(importData, ArtworkFileName), JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions));
        // The game works with the death steps and the whiteout colours alone: they are installed by purpose.
        SamusDeathSteps steps = Build(document).Steps;
        File.WriteAllBytes(Path.Combine(directory, StepFileName), JsonSerializer.SerializeToUtf8Bytes(new StepDocument(FormatVersion,
            [.. Enumerable.Range(0, SamusDeathExplosionTimingDefinitions.RecordCount).Select(steps.ExplosionStep)],
            [.. Enumerable.Range(0, SamusPaletteDefinitions.Death.WhiteoutShadeCount).Select(step => PaletteRgb5.From(steps.WhiteoutColour(step)))]),
            JsonOptions));
    }

    /// <summary>
    /// Loads the installed death steps, as the player may have edited them: the death step each explosion frame shows and the
    /// colour the room whitens to at each whiteout step.
    /// </summary>
    /// <param name="directory">The component's directory, holding the death-step document.</param>
    /// <returns>What the game reads of her death sequence; her death colours are pre-coloured on her sheets.</returns>
    /// <exception cref="InvalidDataException">Strict JSON, version, counts, steps or channels are invalid; errors identify the file.</exception>
    /// <exception cref="IOException">A required installed file cannot be read.</exception>
    public static SamusDeathSteps LoadSteps(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        SamusArtworkFile installed = SamusArtworkFile.Read(Path.Combine(directory, StepFileName));
        return installed.WithContext(() => BuildSteps(installed.Json<StepDocument>(JsonOptions)));
    }

    /// <summary>Loads the import's fatal-damage colour and death-step artwork, which only the import pre-colours her death shades from.</summary>
    /// <param name="importData">The component's import-data directory, holding the death-palette document.</param>
    /// <returns>A ROM-independent catalog of owned colour inputs and death steps.</returns>
    /// <remarks>The JSON needs all original dimensions, packed colors at most $7FFF, and nine death steps 0..9.</remarks>
    /// <exception cref="InvalidDataException">Strict JSON, version, dimensions, color words, or steps are invalid; errors identify the file.</exception>
    /// <exception cref="IOException">The extracted file cannot be read.</exception>
    public static InstalledSamusDeathPalettes LoadForImport(string importData)
    {
        SamusArtworkFile extracted = SamusArtworkFile.Read(Path.Combine(importData, ArtworkFileName));
        return extracted.WithContext(() => Build(extracted.Json<ArtworkDocument>(JsonOptions)));
    }

    /// <summary>Validates death-palette document structure and converts packed RGB5 words into the catalog.</summary>
    private static InstalledSamusDeathPalettes Build(ArtworkDocument document)
    {
        if (document.Version != FormatVersion || document.Suited is null ||
            document.Suitless is null || document.Whiteout is null ||
            document.ExplosionPaletteIndices is null)
            throw new InvalidDataException("Samus death-palette artwork has an invalid structure.");
        return new InstalledSamusDeathPalettes(
            Array.ConvertAll(document.Suited, suit => Array.ConvertAll(
                suit ?? throw new InvalidDataException("Samus death-palette artwork is missing a suit."), Colors)),
            Array.ConvertAll(document.Suitless, Colors), Colors(document.Whiteout), document.ExplosionPaletteIndices);
    }

    /// <summary>Validates the death-step document and builds the steps the game reads.</summary>
    private static SamusDeathSteps BuildSteps(StepDocument document)
    {
        if (document.Version != FormatVersion || document.ExplosionSteps is null || document.Whiteout is null)
            throw new InvalidDataException("Samus death steps have an invalid structure.");
        return new SamusDeathSteps([.. document.ExplosionSteps], [.. document.Whiteout.Select((levels, step) =>
            levels is not null && (uint)levels.Red <= 31 && (uint)levels.Green <= 31 && (uint)levels.Blue <= 31
                ? levels.ToRgbColor()
                : throw new InvalidDataException($"Samus death whiteout step {step} requires channels 0..31."))]);
    }

    /// <summary>The artwork file stores the cartridge's colour words; each must be a fifteen-bit color.</summary>
    private static RgbColor[] Colors(ushort[]? words) => words is null
        ? throw new InvalidDataException("Samus death-palette artwork is missing a palette.")
        : Array.ConvertAll(words, word =>
        RgbColor.IsColorWord(word) ? RgbColor.FromWord(word)
            : throw new InvalidDataException($"Samus death-palette color ${word:X4} sets bit 15."));

    /// <summary>Imports all colors in one native sixteen-entry death palette row.</summary>
    private static ushort[] ReadPalette(ISnesAddressSpace bus, int pointer)
    {
        var colors = new ushort[InstalledSamusDeathPalettes.ColorCount];
        for (int color = 0; color < colors.Length; color++)
            colors[color] = CartridgeImportSource.Require(bus).ReadCartridgeWord((SamusSourceDefinitions.PaletteBank.Value |
                unchecked((ushort)(pointer + color * sizeof(ushort)))));
        return colors;
    }

    /// <summary>Complete suited, suitless, whiteout, and explosion death-step payload, which only the import reads.</summary>
    private sealed record ArtworkDocument(int Version, ushort[][][] Suited,
        ushort[][] Suitless, ushort[] Whiteout, ushort[] ExplosionPaletteIndices);
    /// <summary>
    /// The installed death steps by purpose: the death step each explosion frame shows her suit and suitless body in, and the
    /// colour the room and every other sprite whiten to at each whiteout step.
    /// </summary>
    private sealed record StepDocument(int Version, int[] ExplosionSteps, PaletteRgb5[] Whiteout);
}

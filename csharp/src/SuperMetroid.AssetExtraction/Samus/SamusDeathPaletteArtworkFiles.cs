using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.Samus.Artwork;
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
    internal const string ArtworkFileName = "samus-death-palettes.json";
    /// <summary>Installed JSON filename for the explosion's death steps and the whiteout colours, named by purpose.</summary>
    public const string StepFileName = "samus-death-steps.json";
    /// <summary>
    /// Schema revision of the import's death-palette document; 2 installs the death steps by purpose (#1637), and 3 keys each
    /// suit's colours by the suit and each explosion picture's death step by the picture (#2565).
    /// </summary>
    private const int FormatVersion = 3;
    /// <summary>
    /// Schema revision of the installed death-step document: 2 installs the death steps by purpose (#1637), and 3 writes each
    /// whiteout colour as <c>#rrggbb</c> (#2564).
    /// </summary>
    private const int StepFormatVersion = 4;
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
        Dictionary<string, ushort[][]> suited = SamusSuitListLayout.BySuit(suit =>
        {
            var steps = new ushort[SamusShadeDefinitions.Death.ExplosionShadeCount][];
            for (int palette = 0; palette < steps.Length; palette++)
            {
                int pointer = CartridgeImportSource.Require(bus).ReadCartridgeWord((SamusSourceDefinitions.SuitPointers.Value +
                    suit * SamusShadeDefinitions.Death.ExplosionShadeCount * sizeof(ushort) +
                    palette * sizeof(ushort)));
                steps[palette] = ReadPalette(bus, pointer);
            }
            return steps;
        });
        var suitless = new ushort[SamusShadeDefinitions.Death.ExplosionShadeCount][];
        for (int palette = 0; palette < suitless.Length; palette++)
        {
            int pointer = CartridgeImportSource.Require(bus).ReadCartridgeWord((SamusSourceDefinitions.SuitlessPointers.Value + palette * sizeof(ushort)));
            suitless[palette] = ReadPalette(bus, pointer);
        }
        var whiteout = new ushort[SamusShadeDefinitions.Death.WhiteoutShadeCount];
        for (int shade = 0; shade < whiteout.Length; shade++)
            whiteout[shade] = CartridgeImportSource.Require(bus).ReadCartridgeWord((SamusSourceDefinitions.DeathWhiteoutShades.Value + shade * sizeof(ushort)));
        // The cartridge's explosion records follow the explosion's pictures in the order its animation shows them.
        var selectors = new Dictionary<string, int>(StringComparer.Ordinal);
        for (int record = 0; record < SamusDeathSheetDefinitions.Explosion.Count; record++)
        {
            SamusDeathExplosionFrame frame = SamusDeathSheetDefinitions.Explosion[record];
            int selector = bus.ReadCartridgeByte(SamusSourceDefinitions.DeathExplosionTimingAndPaletteIndices.Value + record * 2 + 1);
            string key = SamusDeathExplosionFrameKeys.Installed.Of(frame);
            if (selectors.TryGetValue(key, out int known) && known != selector)
                throw new InvalidDataException($"Samus's death explosion shows its {frame} picture in two death steps.");
            selectors[key] = selector;
        }
        var document = new ArtworkDocument(FormatVersion, suited, suitless, whiteout, selectors);
        File.WriteAllBytes(Path.Combine(importData, ArtworkFileName), JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions));
        // The game works with the death steps and the whiteout colours alone: they are installed by purpose.
        SamusDeathSteps steps = Build(document).Steps;
        File.WriteAllBytes(Path.Combine(directory, StepFileName), JsonSerializer.SerializeToUtf8Bytes(new StepDocument(StepFormatVersion,
            SamusDeathSheetDefinitions.Explosion.Distinct().ToDictionary(frame => SamusDeathExplosionFrameKeys.Installed.Of(frame), frame => steps.ExplosionShade(frame).Step, StringComparer.Ordinal),
            [.. SamusWhiteoutShade.All.Select(shade => InstalledColourText.Format(steps.WhiteoutColour(shade)))]),
            JsonOptions));
    }

    /// <summary>
    /// Loads the installed death steps, as the player may have edited them: the death step each explosion frame shows and the
    /// colour the room whitens to at each whiteout step.
    /// </summary>
    /// <param name="directory">The component's directory, holding the death-step document.</param>
    /// <returns>What the game reads of her death sequence; her death colours are pre-coloured on her sheets.</returns>
    /// <exception cref="InvalidDataException">Strict JSON, version, counts or steps are invalid, or a whiteout colour is not written as <c>#rrggbb</c> on the game's fade scale; errors identify the file.</exception>
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
            document.ExplosionSteps is null)
            throw new InvalidDataException("Samus death-palette artwork has an invalid structure.");
        var explosion = new Dictionary<SamusDeathExplosionFrame, int>();
        foreach ((string key, int step) in document.ExplosionSteps)
        {
            if (!SamusDeathExplosionFrameKeys.Installed.TryParse(key, out SamusDeathExplosionFrame frame) || !explosion.TryAdd(frame, step))
                throw new InvalidDataException($"Samus death-palette artwork names explosion picture '{key}', which is no explosion picture or is named twice.");
        }
        return new InstalledSamusDeathPalettes(
            SamusSuitListLayout.Order.ToDictionary(suit => suit, suit => Array.ConvertAll(
                document.Suited.TryGetValue(SamusSuitKeys.Installed.Of(suit), out ushort[][]? steps) && steps is not null
                    ? steps : throw new InvalidDataException($"Samus death-palette artwork is missing the {suit} suit."), Colors)),
            Array.ConvertAll(document.Suitless, Colors), Colors(document.Whiteout), explosion);
    }

    /// <summary>Validates the death-step document and builds the steps the game reads.</summary>
    private static SamusDeathSteps BuildSteps(StepDocument document)
    {
        if (document.Version != StepFormatVersion || document.ExplosionSteps is null || document.Whiteout is null)
            throw new InvalidDataException("Samus death steps have an invalid structure.");
        if (document.Whiteout.Length != SamusWhiteoutShade.All.Count)
            throw new InvalidDataException("Samus death steps hold one whiteout colour per whiteout step.");
        var explosion = new Dictionary<SamusDeathExplosionFrame, SamusDeathShade>();
        foreach ((string key, int step) in document.ExplosionSteps)
        {
            if (!SamusDeathExplosionFrameKeys.Installed.TryParse(key, out SamusDeathExplosionFrame frame) || !explosion.TryAdd(frame, new SamusDeathShade(step)))
                throw new InvalidDataException($"Samus death steps name explosion frame '{key}', which is no explosion frame or is named twice.");
        }
        // The whiteout's colours are listed in the order of its steps.
        return new SamusDeathSteps(explosion, SamusWhiteoutShade.All.ToDictionary(shade => shade,
            shade => InstalledColourText.ParseOnFadeScale(document.Whiteout[shade.Step], $"Samus death whiteout step {shade.Step}")));
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
    private sealed record ArtworkDocument(int Version, Dictionary<string, ushort[][]> Suited,
        ushort[][] Suitless, ushort[] Whiteout, Dictionary<string, int> ExplosionSteps);
    /// <summary>
    /// The installed death steps by purpose: the death step each explosion frame shows her suit and suitless body in, and the
    /// colour the room and every other sprite whiten to at each whiteout step, written as <c>#rrggbb</c> (#2564).
    /// </summary>
    private sealed record StepDocument(int Version, Dictionary<string, int> ExplosionSteps, string[] Whiteout);
}

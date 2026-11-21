using System.Text.Json;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>Reads and writes the installed Mother Brain health palette document.</summary>
public static class MotherBrainHealthPaletteFile
{
    /// <summary>Supported schema version: 2 keys each health band's body and rear-leg colours by the band (#2565).</summary>
    public const int Version = 2;

    /// <summary>Installed resource filename loaded with area-map presentation assets for Mother Brain's body/brain and rear-leg damage colors.</summary>
    public const string FileName = "mother-brain-health-palette.json";

    /// <summary>Reads the installed RGB5 health-state artwork, keeping every colour as installed.</summary>
    /// <param name="json">UTF-8 JSON input consumed from its current position to the end; the stream remains open.</param>
    /// <returns>Compiled body and rear-leg palette state independent of the source document's mutable arrays.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">The JSON is malformed, null, has duplicate properties, uses an unsupported version, or lacks four fifteen-color rows in each palette with RGB5 channels from zero through 31.</exception>
    public static MotherBrainHealthPalettePresentation Read(Stream json)
    {
        MotherBrainHealthPaletteDocument document = JsonAssetDocument.Read<MotherBrainHealthPaletteDocument>(
            json, ArtworkJson.Options, "Mother Brain health palette");
        if (document.Version != Version)
            throw new InvalidDataException("Unsupported Mother Brain health palette version.");
        IReadOnlyList<MotherBrainHealthBand> bands = MotherBrainHealthBandDefinitions.HighestFirst;
        if (document.HealthBands is not { } installed || installed.Count != bands.Count)
            throw new InvalidDataException($"Mother Brain health palette requires {bands.Count} damage states, each keyed by its band.");
        Dictionary<MotherBrainHealthBand, MotherBrainBodyColoursDocument> byBand = bands.ToDictionary(band => band, band =>
            installed.TryGetValue(band.Key, out MotherBrainBodyColoursDocument? colours) && colours is not null
                ? colours
                : throw new InvalidDataException($"Mother Brain health palette holds no colours for her {band} band."));
        return new MotherBrainHealthPalettePresentation(
            Rows(byBand.ToDictionary(entry => entry.Key, entry => Rgb5Rows.Compile(entry.Value.Body, MotherBrainRainbowPaletteGeometry.ColorCount,
                $"Mother Brain {entry.Key} body")), "body"),
            Rows(byBand.ToDictionary(entry => entry.Key, entry => Rgb5Rows.Compile(entry.Value.BackLegs, MotherBrainRainbowPaletteGeometry.ColorCount,
                $"Mother Brain {entry.Key} rear legs")), "rear legs"));

        InstalledKeyedColourRows<MotherBrainHealthBand> Rows(Dictionary<MotherBrainHealthBand, RgbColor[]> colours, string name) =>
            new(colours, bands, MotherBrainRainbowPaletteGeometry.ColorCount, $"Mother Brain {name}");
    }

    /// <summary>Serializes artwork as UTF-8 JSON and writes the bytes without retaining the document.</summary>
    /// <param name="json">Output written at its current position and left open; existing trailing bytes are not truncated.</param>
    /// <param name="document">Four health-band images for body/brain and rear legs; supplied arrays are not modified.</param>
    public static void Write(Stream json, MotherBrainHealthPaletteDocument document)
    {
        json.Write(JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options));
    }
}

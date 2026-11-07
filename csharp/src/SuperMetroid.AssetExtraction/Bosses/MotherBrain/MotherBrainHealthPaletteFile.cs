using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>Reads and writes the installed Mother Brain health palette document.</summary>
public static class MotherBrainHealthPaletteFile
{
    /// <summary>Supported schema version requiring paired body and rear-leg arrays with four fifteen-color RGB5 rows each.</summary>
    public const int Version = 1;

    /// <summary>Installed resource filename loaded with area-map presentation assets for Mother Brain's body/brain and rear-leg damage colors.</summary>
    public const string FileName = "mother-brain-health-palette.json";

    /// <summary>Compiles installed RGB5 health-state artwork, retaining supplied edits independently of the native shade ramps and red-tint calculations.</summary>
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
        return MotherBrainHealthPaletteRecognition.Create(Convert(document.Body, nameof(document.Body)),
            Convert(document.BackLegs, nameof(document.BackLegs)));

        static RgbColor[][] Convert(PaletteRgb5[][]? frames, string name)
        {
            if (frames is null || frames.Length != MotherBrainHealthPaletteFormat.StateCount)
                throw new InvalidDataException($"Mother Brain {name} requires four damage states.");
            var values = new RgbColor[frames.Length][];
            for (int state = 0; state < frames.Length; state++)
            {
                PaletteRgb5[]? colors = frames[state];
                if (colors is null || colors.Length != MotherBrainRainbowPaletteGeometry.ColorCount)
                    throw new InvalidDataException($"Mother Brain {name} state {state} requires fifteen colors.");
                values[state] = new RgbColor[colors.Length];
                for (int color = 0; color < colors.Length; color++)
                {
                    PaletteRgb5? rgb = colors[color];
                    if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 || (uint)rgb.Blue > 31)
                        throw new InvalidDataException($"Mother Brain {name} state {state} color {color} requires RGB5 components.");
                    values[state][color] = rgb.ToRgbColor();
                }
            }
            return values;
        }
    }

    /// <summary>Serializes artwork as UTF-8 JSON and writes the bytes without retaining the document.</summary>
    /// <param name="json">Output written at its current position and left open; existing trailing bytes are not truncated.</param>
    /// <param name="document">Four health-band images for body/brain and rear legs; supplied arrays are not modified.</param>
    public static void Write(Stream json, MotherBrainHealthPaletteDocument document)
    {
        json.Write(JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options));
    }
}

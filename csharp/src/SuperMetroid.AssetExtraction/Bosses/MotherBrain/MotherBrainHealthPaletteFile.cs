using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>Reads and writes the installed Mother Brain health palette document.</summary>
public static class MotherBrainHealthPaletteFile
{
    /// <summary>Compiles installed RGB5 health-state artwork, retaining supplied edits independently of the native shade ramps and red-tint calculations.</summary>
    /// <param name="json">UTF-8 JSON input consumed from its current position to the end; the stream remains open.</param>
    /// <returns>Compiled body and rear-leg palette state independent of the source document's mutable arrays.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">The JSON is malformed, null, has duplicate properties, uses an unsupported version, or lacks four fifteen-color rows in each palette with RGB5 channels from zero through 31.</exception>
    public static MotherBrainHealthPalettePresentation Read(Stream json)
    {
        MotherBrainHealthPaletteDocument document = JsonAssetDocument.Read<MotherBrainHealthPaletteDocument>(
            json, ArtworkJson.Options, "Mother Brain health palette");
        if (document.Version != MotherBrainHealthPaletteFormat.Version)
            throw new InvalidDataException("Unsupported Mother Brain health palette version.");
        return new(Convert(document.Body, nameof(document.Body)),
            Convert(document.BackLegs, nameof(document.BackLegs)));

        static RgbColor[][] Convert(PaletteRgb5[][]? frames, string name)
        {
            if (frames is null || frames.Length != MotherBrainHealthPaletteFormat.StateCount)
                throw new InvalidDataException($"Mother Brain {name} requires four damage states.");
            var values = new RgbColor[frames.Length][];
            for (int state = 0; state < frames.Length; state++)
            {
                PaletteRgb5[]? colors = frames[state];
                if (colors is null || colors.Length != MotherBrainRainbowPaletteDefinitions.ColorCount)
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

    /// <summary>Serializes artwork as UTF-8 JSON, validates it through <see cref="Read"/>, then writes the validated bytes without retaining the document.</summary>
    /// <param name="json">Output written at its current position and left open; existing trailing bytes are not truncated.</param>
    /// <param name="document">Four health-band images for body/brain and rear legs; supplied arrays are not modified.</param>
    /// <exception cref="InvalidDataException">The serialized document is null or fails schema, palette dimensions, or RGB5 channel validation.</exception>
    public static void Write(Stream json, MotherBrainHealthPaletteDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        _ = Read(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

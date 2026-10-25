using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Magdollite;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Reads and writes the installed Magdollite palette cycle document.</summary>
public static class MagdollitePaletteCycleFile
{
    /// <summary>Validates and compiles the four RGB5 glow rows, preserving independent edits rather than requiring all later rows to rotate the first.</summary>
    /// <param name="json">UTF-8 JSON source consumed from its current position and left open.</param>
    /// <returns>Compiled palette-cycle colors detached from the mutable document arrays.</returns>
    /// <exception cref="ArgumentNullException">The source stream is null.</exception>
    /// <exception cref="InvalidDataException">The JSON contains duplicate or unknown properties, an unsupported version, dimensions other than four rows of four colors, null colors, or channels outside 0..31.</exception>
    public static MagdollitePaletteCycle Read(Stream json)
    {
        Ensure.NotNull(json);
        MagdollitePaletteCycleDocument document;
        document = JsonAssetDocument.Read<MagdollitePaletteCycleDocument>(json, ArtworkJson.Options, "Magdollite palette cycle");

        if (document.Version != MagdollitePaletteCycleFormat.Version ||
            document.Frames is null ||
            document.Frames.Length != MagdollitePaletteDefinitions.FrameCount)
            throw new InvalidDataException("Magdollite palette cycle requires four version-one frames.");

        var compiled = new RgbColor[MagdollitePaletteDefinitions.FrameCount][];
        for (int frame = 0; frame < compiled.Length; frame++)
        {
            PaletteRgb5[]? source = document.Frames[frame];
            if (source is null || source.Length != MagdollitePaletteDefinitions.AnimatedColorCount)
                throw new InvalidDataException($"Magdollite palette frame {frame} requires four RGB5 colors.");
            compiled[frame] = new RgbColor[source.Length];
            for (int color = 0; color < source.Length; color++)
            {
                PaletteRgb5? rgb = source[color];
                if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                    (uint)rgb.Blue > 31)
                    throw new InvalidDataException(
                        $"Magdollite palette frame {frame} color {color} requires RGB5 channels 0..31.");
                compiled[frame][color] = rgb.ToRgbColor();
            }
        }
        return new MagdollitePaletteCycle(compiled);
    }

    /// <summary>Serializes the editable glow cycle to indented camel-case UTF-8 JSON and validates the resulting bytes through <see cref="Read"/>.</summary>
    /// <param name="document">Four complete RGB5 glow rows to serialize; their arrays are not retained.</param>
    /// <returns>Validated JSON bytes for <see cref="MagdollitePaletteCycleFormat.FileName"/>.</returns>
    /// <exception cref="InvalidDataException">The serialized document fails schema, row-dimension, or RGB5-channel validation.</exception>
    public static byte[] Write(MagdollitePaletteCycleDocument document) =>
        JsonAssetDocument.WriteValidated(document, ArtworkJson.Options, json => _ = Read(json));
}

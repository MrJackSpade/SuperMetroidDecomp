using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Reads and writes the installed Samus Hyper Beam colour document.</summary>
public static class SamusHyperBeamColorFile
{
    /// <summary>Validates and compiles the ten full-body Hyper Beam RGB5 rows, preserving independently supplied color edits while leaving palette-cycle timing to Samus's native handler.</summary>
    /// <param name="json">UTF-8 JSON source consumed from its current position and left open.</param>
    /// <returns>Compiled packed-color catalog detached from the document arrays, with shared hue/shade relationships resolved on demand.</returns>
    /// <exception cref="ArgumentNullException">The source stream is null.</exception>
    /// <exception cref="InvalidDataException">The JSON contains duplicate or unknown properties, an unsupported version, dimensions other than ten rows of sixteen colors, null colors, or channels outside 0..31.</exception>
    public static SamusHyperBeamColorCatalog Read(Stream json)
    {
        Ensure.NotNull(json);
        SamusHyperBeamColorDocument document;
        document = JsonAssetDocument.Read<SamusHyperBeamColorDocument>(json, ArtworkJson.Options, "Samus Hyper Beam color");
        if (document.Version != SamusHyperBeamColorFormat.Version)
            throw new InvalidDataException("Samus Hyper Beam colors require the supported version.");
        return FromFrames(document.Frames);
    }

    /// <summary>Validates independently supplied rows for either native view of the Hyper Beam palette.</summary>
    internal static SamusHyperBeamColorCatalog FromFrames(PaletteRgb5[][]? frames)
    {
        if (!SamusHyperBeamColorFormat.Shape.HasRows(frames))
            throw new InvalidDataException("Samus Hyper Beam colors require ten frames.");
        var compiled = new RgbColor[frames.Length][];
        for (int frame = 0; frame < compiled.Length; frame++)
        {
            PaletteRgb5[]? source = frames[frame];
            if (!SamusHyperBeamColorFormat.Shape.HasColumns(source))
                throw new InvalidDataException($"Samus Hyper Beam frame {frame} requires sixteen RGB5 colors.");
            compiled[frame] = new RgbColor[source.Length];
            for (int colorIndex = 0; colorIndex < source.Length; colorIndex++)
            {
                PaletteRgb5? color = source[colorIndex];
                if (color is null || (uint)color.Red > 31 ||
                    (uint)color.Green > 31 || (uint)color.Blue > 31)
                    throw new InvalidDataException($"Samus Hyper Beam frame {frame} color {colorIndex} requires RGB components from zero through 31.");
                compiled[frame][colorIndex] = color.ToRgbColor();
            }
        }
        return new(compiled);
    }

    /// <summary>Serializes the editable full-body palette cycle to indented camel-case UTF-8 JSON and validates the resulting bytes through <see cref="Read"/>.</summary>
    /// <param name="document">Ten complete RGB5 palette rows to serialize; the writer does not retain their arrays.</param>
    /// <returns>Validated JSON bytes for <see cref="SamusHyperBeamColorFormat.FileName"/>.</returns>
    /// <exception cref="InvalidDataException">The serialized document fails schema, palette-dimension, or RGB5-channel validation.</exception>
    public static byte[] Write(SamusHyperBeamColorDocument document) =>
        JsonAssetDocument.WriteValidated(document, ArtworkJson.Options, json => _ = Read(json));
}

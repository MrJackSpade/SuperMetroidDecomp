using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Chozo;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Reads and writes the installed Chozo statue and tube colour document.</summary>
public static class ChozoAndTubeColorFile
{
    /// <summary>Loads version-1 <c>chozo-and-tube-colors.json</c>, requiring three independent 32-color images and RGB5 channels from 0 through 31.</summary>
    /// <param name="json">Caller-owned stream consumed from its current position and left open; unknown and duplicate JSON properties are rejected.</param>
    /// <returns>Compiled selected tube and statue inks; statue variant selection, hand PLMs, and tube behavior remain engine-owned.</returns>
    public static ChozoAndTubeColorCatalog Read(Stream json)
    {
        Ensure.NotNull(json);
        ChozoAndTubeColorDocument document;
        document = JsonAssetDocument.Read<ChozoAndTubeColorDocument>(json, ArtworkJson.Options, "Chozo/tube color");
        if (document.Version != ChozoAndTubeColorFormat.Version)
            throw new InvalidDataException("Chozo/tube colors require the supported version.");
        return new(
            Compile(document.TubeCracks, "tube cracks"),
            Compile(document.WreckedShip, "Wrecked Ship"),
            Compile(document.LowerNorfair, "Lower Norfair"));
    }

    /// <summary>Serializes the three color images as indented camel-case UTF-8 JSON, validating version, 32-color dimensions, and RGB5 channel bounds before returning the bytes.</summary>
    public static byte[] Write(ChozoAndTubeColorDocument document) =>
        JsonAssetDocument.WriteValidated(document, ArtworkJson.Options, json => _ = Read(json));

    /// <summary>Validates exactly 32 five-bit RGB triples and converts them to packed CGRAM colors.</summary>
    private static RgbColor[] Compile(PaletteRgb5[]? source, string name)
    {
        if (source is null || source.Length != ChozoAndTubeColorDefinitions.ColorCount)
            throw new InvalidDataException(
                $"Chozo/tube {name} requires {ChozoAndTubeColorDefinitions.ColorCount} RGB5 colors.");
        var compiled = new RgbColor[source.Length];
        for (int color = 0; color < source.Length; color++)
        {
            PaletteRgb5? rgb = source[color];
            if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                (uint)rgb.Blue > 31)
                throw new InvalidDataException(
                    $"Chozo/tube {name} color {color} requires RGB5 channels 0..31.");
            compiled[color] = rgb.ToRgbColor();
        }
        return compiled;
    }
}

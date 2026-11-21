using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Draygon.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Draygon;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>Reads and writes the installed Draygon colour document.</summary>
public static class DraygonColorFile
{
    /// <summary>Loads and validates the editable Draygon RGB5 color document.</summary>
    public static DraygonColorCatalog Read(Stream json)
    {
        Ensure.NotNull(json);
        DraygonColorDocument document;
        document = JsonAssetDocument.Read<DraygonColorDocument>(json, ArtworkJson.Options, "Draygon color");
        if (document.Version != DraygonColorFormat.Version)
            throw new InvalidDataException("Draygon colors require the supported version.");
        return new(
            Compile(document.Intro, DraygonColorDefinitions.IntroCount, "intro"),
            Compile(document.Background, DraygonColorDefinitions.BackgroundCount, "background"),
            Compile(document.Sprite, DraygonColorDefinitions.SpriteCount, "sprite"),
            Compile(document.WhiteFlash, DraygonColorDefinitions.WhiteFlashCount, "white flash"),
            Rgb5Rows.CompileKeyed(document.HealthBands, DraygonHealthBandDefinitions.HighestFirst, band => band.Key,
                DraygonColorDefinitions.HealthBandColorCount, "Draygon health band"));
    }

    /// <summary>Serializes a Draygon color document as indented camel-case JSON.</summary>
    public static byte[] Write(DraygonColorDocument document) =>
        JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);

    /// <summary>Checks a named RGB5 array's required length and channel bounds before packing its colors.</summary>
    private static RgbColor[] Compile(PaletteRgb5[]? source, int count, string name) =>
        Rgb5Rows.Compile(source, count, $"Draygon {name}");
}

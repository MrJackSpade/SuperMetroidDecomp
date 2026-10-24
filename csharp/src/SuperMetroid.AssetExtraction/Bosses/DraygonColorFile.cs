using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
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
        if (document.HealthBands is null ||
            document.HealthBands.Length != DraygonColorRomData.HealthBandCount)
            throw new InvalidDataException("Draygon colors require eight health bands.");
        return new(
            Compile(document.Intro, DraygonColorRomData.IntroCount, "intro"),
            Compile(document.Background, DraygonColorRomData.BackgroundCount, "background"),
            Compile(document.Sprite, DraygonColorRomData.SpriteCount, "sprite"),
            Compile(document.WhiteFlash, DraygonColorRomData.WhiteFlashCount, "white flash"),
            document.HealthBands.Select((band, index) =>
                Compile(band, DraygonColorRomData.HealthBandColorCount,
                    $"health band {index}")).ToArray());
    }

    /// <summary>Validates and serializes a Draygon color document as indented camel-case JSON.</summary>
    public static byte[] Write(DraygonColorDocument document) =>
        JsonAssetDocument.WriteValidated(document, ArtworkJson.Options, json => _ = Read(json));

    /// <summary>Checks a named RGB5 array's required length and channel bounds before packing its colors.</summary>
    private static Bgr555[] Compile(PaletteRgb5[]? source, int count, string name) =>
        Rgb5Rows.Compile(source, count, $"Draygon {name}");
}

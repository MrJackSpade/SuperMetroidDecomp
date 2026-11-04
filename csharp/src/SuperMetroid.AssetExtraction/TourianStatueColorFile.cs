using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Areas.Tourian.Artwork;

namespace SuperMetroid.AssetExtraction;

/// <summary>Reads and writes the installed Tourian statue colour document.</summary>
public static class TourianStatueColorFile
{
    /// <summary>Writes camel-case resource JSON and rejects unknown members on load.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Loads version-1 <c>tourian-statue-colors.json</c>, validating sixteen-color base/statue bands, four four-color eye rows, eight grey inks, and RGB5 channels from 0 through 31.</summary>
    /// <param name="json">Caller-owned JSON stream consumed from its current position and left open; unknown and duplicate properties are rejected.</param>
    /// <returns>Compiled selected colors retaining independent edits over calculated stock paint bands.</returns>
    public static TourianStatuePalettes Read(Stream json)
    {
        TourianStatueColorDocument document;
        document = JsonAssetDocument.Read<TourianStatueColorDocument>(json, Options, "Tourian statue color");
        if (document.Version != TourianStatueColorFormat.Version ||
            document.Eye is null || document.Eye.Length != TourianStatueColorDefinitions.EyeGlowCount)
            throw new InvalidDataException("Tourian statue colors require four eye rows at the supported version.");
        var eye = new RgbColor[document.Eye.Length][];
        for (int row = 0; row < eye.Length; row++)
            eye[row] = Compile(document.Eye[row], TourianStatueColorDefinitions.EyeColorCount,
                $"eye row {row}");
        return new(
            Band(TourianStatuePaintBand.Base, Compile(document.Base, TourianStatueColorFormat.BaseColorCount, "base")),
            Band(TourianStatuePaintBand.Statue, Compile(document.Statue, TourianStatueColorFormat.StatueColorCount, "statue")),
            Band(TourianStatuePaintBand.Eye, eye.SelectMany(glow => glow).ToArray()),
            Band(TourianStatuePaintBand.Grey, Compile(document.Grey, TourianStatueColorDefinitions.GreyColorCount, "grey")));
    }

    /// <summary>Compares supplied colors with the band's stock paint and retains the independent edits.</summary>
    private static TourianStatuePalettes.PaletteBand Band(TourianStatuePaintBand band, RgbColor[] supplied)
    {
        var edits = new Dictionary<PaintColour, RgbColor>();
        for (int color = 0; color < supplied.Length; color++)
            if (supplied[color] != TourianStatuePaint.Color(band, color)) edits.Add(new PaintColour(0, color), supplied[color]);
        return new TourianStatuePalettes.PaletteBand(band, supplied.Length, edits);
    }

    /// <summary>Serializes the editable colors as indented camel-case UTF-8 JSON.</summary>
    public static byte[] Write(TourianStatueColorDocument document) =>
        JsonSerializer.SerializeToUtf8Bytes(document, Options);

    /// <summary>Checks a band's required length and five-bit channels before converting to hardware color words.</summary>
    private static RgbColor[] Compile(PaletteRgb5[]? colors, int count, string label)
    {
        if (colors is null || colors.Length != count)
            throw new InvalidDataException($"Tourian statue {label} requires {count} colors.");
        var compiled = new RgbColor[count];
        for (int color = 0; color < compiled.Length; color++)
        {
            PaletteRgb5? rgb = colors[color];
            if (rgb is null || (uint)rgb.Red > 31 ||
                (uint)rgb.Green > 31 || (uint)rgb.Blue > 31)
                throw new InvalidDataException($"Tourian statue {label} color {color} requires RGB5 channels.");
            compiled[color] = rgb.ToRgbColor();
        }
        return compiled;
    }
}

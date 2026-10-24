using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.Core.Areas.Tourian;

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
    public static TourianStatueColorCatalog Read(Stream json)
    {
        TourianStatueColorDocument document;
        document = JsonAssetDocument.Read<TourianStatueColorDocument>(json, Options, "Tourian statue color");
        if (document.Version != TourianStatueColorFormat.Version ||
            document.Eye is null || document.Eye.Length != TourianStatuePaletteRomData.EyeRowCount)
            throw new InvalidDataException("Tourian statue colors require four eye rows at the supported version.");
        var eye = new Bgr555[document.Eye.Length][];
        for (int row = 0; row < eye.Length; row++)
            eye[row] = Compile(document.Eye[row], TourianStatuePaletteRomData.EyeColorCount,
                $"eye row {row}");
        return new(
            Compile(document.Base, TourianStatuePaletteRomData.BaseColorCount, "base"),
            Compile(document.Statue, TourianStatuePaletteRomData.StatueColorCount, "statue"),
            eye,
            Compile(document.Grey, TourianStatuePaletteRomData.GreyColorCount, "grey"));
    }

    /// <summary>Serializes the editable colors as indented camel-case UTF-8 JSON, validating version, palette dimensions, and RGB5 bounds before returning the bytes.</summary>
    public static byte[] Write(TourianStatueColorDocument document) =>
        JsonAssetDocument.WriteValidated(document, Options, json => _ = Read(json));

    /// <summary>Checks a band's required length and five-bit channels before converting to hardware color words.</summary>
    private static Bgr555[] Compile(PaletteRgb5[]? colors, int count, string label)
    {
        if (colors is null || colors.Length != count)
            throw new InvalidDataException($"Tourian statue {label} requires {count} colors.");
        var compiled = new Bgr555[count];
        for (int color = 0; color < compiled.Length; color++)
        {
            PaletteRgb5? rgb = colors[color];
            if (rgb is null || (uint)rgb.Red > 31 ||
                (uint)rgb.Green > 31 || (uint)rgb.Blue > 31)
                throw new InvalidDataException($"Tourian statue {label} color {color} requires RGB5 channels.");
            compiled[color] = rgb.ToBgr555();
        }
        return compiled;
    }
}

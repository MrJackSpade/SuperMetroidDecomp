using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Reads and writes the installed Samus visor colour document.</summary>
public static class SamusVisorColorFile
{
    /// <summary>Supported schema revision, one, requiring exactly the six authored visor words.</summary>
    public const int Version = 1;

    /// <summary>JSON resource filename containing the six editable visor RGB5 colors.</summary>
    public const string FileName = "samus-visor-colors.json";

    /// <summary>Validates the six RGB5 visor colors and compiles independent edits relative to the reviewed $9B:A3C0 widening/cycling definitions.</summary>
    /// <param name="json">UTF-8 JSON source consumed from its current position and left open.</param>
    /// <returns>Compiled visor colors detached from the document array, without changing X-ray or room-cycle timing.</returns>
    /// <exception cref="ArgumentNullException">The source stream is null.</exception>
    /// <exception cref="InvalidDataException">The JSON contains duplicate or unknown properties, an unsupported version, a length other than six, null colors, or RGB5 channels outside 0..31.</exception>
    public static InstalledSamusVisorColors Read(Stream json)
    {
        Ensure.NotNull(json);
        SamusVisorColorDocument document;
        document = JsonAssetDocument.Read<SamusVisorColorDocument>(json, ArtworkJson.Options, "Samus visor color");
        if (document.Version != Version ||
            document.Colors is null || document.Colors.Length != SamusVisorColorFormat.ColorCount)
            throw new InvalidDataException("Samus visor colors require the supported version and six RGB5 colors.");

        var compiled = new Dictionary<TableColourPlace, RgbColor>();
        for (int index = 0; index < document.Colors.Length; index++)
        {
            PaletteRgb5? color = document.Colors[index];
            if (color is null || (uint)color.Red > 31 ||
                (uint)color.Green > 31 || (uint)color.Blue > 31)
                throw new InvalidDataException($"Samus visor color {index} requires RGB components from zero through 31.");
            RgbColor packed = color.ToRgbColor();
            if (packed != SamusVisorColorSteps.Color(index)) compiled.Add(new TableColourPlace(0, index), packed);
        }
        return new(compiled);
    }

    /// <summary>Serializes the editable visor colors to indented camel-case UTF-8 JSON.</summary>
    /// <param name="document">All six RGB5 visor colors to serialize; the array is not retained.</param>
    /// <returns>JSON bytes for <see cref="FileName"/>.</returns>
    public static byte[] Write(SamusVisorColorDocument document) =>
        JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
}

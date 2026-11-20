using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Reads and writes the installed Samus visor colour document.</summary>
public static class SamusVisorColorFile
{
    /// <summary>Supported schema revision, one, requiring exactly the six authored visor words.</summary>
    public const int Version = 1;

    /// <summary>JSON resource filename containing the six editable visor RGB5 colors.</summary>
    public const string FileName = "samus-visor-colors.json";

    /// <summary>Validates the six RGB5 visor colors and keeps every one as installed.</summary>
    /// <param name="json">UTF-8 JSON source consumed from its current position and left open.</param>
    /// <returns>The installed visor colors detached from the document array, without changing X-ray or room-cycle timing.</returns>
    /// <exception cref="ArgumentNullException">The source stream is null.</exception>
    /// <exception cref="InvalidDataException">The JSON contains duplicate or unknown properties, an unsupported version, a length other than six, null colors, or RGB5 channels outside 0..31.</exception>
    public static InstalledSamusVisorColors Read(Stream json)
    {
        Ensure.NotNull(json);
        SamusVisorColorDocument document;
        document = JsonAssetDocument.Read<SamusVisorColorDocument>(json, ArtworkJson.Options, "Samus visor color");
        if (document.Version != Version || document.Widening is null || document.Cycle is null ||
            document.Widening.Length + document.Cycle.Length != SamusVisorColorFormat.ColorCount || document.Widening.Length != document.Cycle.Length)
            throw new InvalidDataException("Samus visor colors require the supported version, three widening and three cycle RGB5 colors.");

        PaletteRgb5[] colours = document.InReaderOrder();
        var compiled = new RgbColor[colours.Length];
        for (int index = 0; index < colours.Length; index++)
        {
            PaletteRgb5? color = colours[index];
            if (color is null || (uint)color.Red > 31 ||
                (uint)color.Green > 31 || (uint)color.Blue > 31)
                throw new InvalidDataException($"Samus visor color {index} requires RGB components from zero through 31.");
            compiled[index] = color.ToRgbColor();
        }
        return new(compiled);
    }

    /// <summary>Serializes the editable visor colors to indented camel-case UTF-8 JSON.</summary>
    /// <param name="document">All six RGB5 visor colors to serialize; the array is not retained.</param>
    /// <returns>JSON bytes for <see cref="FileName"/>.</returns>
    public static byte[] Write(SamusVisorColorDocument document) =>
        JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
}

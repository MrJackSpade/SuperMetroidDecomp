using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Reads and writes the installed Samus hurt colour document.</summary>
public static class SamusHurtColorFile
{
    /// <summary>Compiles the installed hurt and cinematic palettes into independent color edits and calculated native shade relationships.</summary>
    /// <param name="json">UTF-8 JSON input, consumed from its current position and left open.</param>
    /// <returns>A catalog with private compiled color state, independent of the input document's arrays.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">The JSON is malformed, null, has duplicate or unknown properties, uses an unsupported version, or does not supply two sixteen-color palettes with RGB channels from zero through 31.</exception>
    public static InstalledSamusHurtColors Read(Stream json)
    {
        Ensure.NotNull(json);
        SamusHurtColorDocument document;
        document = JsonAssetDocument.Read<SamusHurtColorDocument>(json, ArtworkJson.Options, "Samus hurt color");
        if (document.Version != SamusHurtColorFormat.Version)
            throw new InvalidDataException("Samus hurt colors require the supported version.");
        return new(Compile(document.Hurt, "hurt"), Compile(document.Intro, "intro"));
    }

    /// <summary>Serializes a palette document as indented, camel-case UTF-8 JSON and validates it through the same loader used for installed content.</summary>
    /// <param name="document">Hurt and cinematic RGB5 artwork to serialize; its arrays are not modified.</param>
    /// <returns>A newly allocated JSON byte array suitable for <see cref="SamusHurtColorFormat.FileName"/>.</returns>
    /// <exception cref="InvalidDataException">The serialized document is null or fails the supported version, palette length, or RGB5 channel requirements.</exception>
    public static byte[] Write(SamusHurtColorDocument document) =>
        JsonAssetDocument.WriteValidated(document, ArtworkJson.Options, json => _ = Read(json));

    /// <summary>Checks one complete sixteen-color RGB5 document array and converts it to packed colors.</summary>
    private static RgbColor[] Compile(PaletteRgb5[]? source, string name)
    {
        if (source is null || source.Length != SamusHurtColorFormat.ColorsPerPalette)
            throw new InvalidDataException($"Samus {name} palette requires sixteen RGB5 colors.");
        var colors = new RgbColor[source.Length];
        for (int index = 0; index < colors.Length; index++)
        {
            PaletteRgb5? color = source[index];
            if (color is null || (uint)color.Red > 31 ||
                (uint)color.Green > 31 || (uint)color.Blue > 31)
                throw new InvalidDataException($"Samus {name} color {index} requires RGB components from zero through 31.");
            colors[index] = color.ToRgbColor();
        }
        return colors;
    }
}

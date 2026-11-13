using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Reads and writes the installed Power Bomb glow colour document.</summary>
public static class PowerBombGlowColorFile
{
    /// <summary>
    /// Supported schema revision, fixing the sixteen- and thirty-two-entry sequence dimensions; 2 names the file for the
    /// explosion's glow (#1631).
    /// </summary>
    public const int Version = 2;

    /// <summary>JSON resource filename containing the pre-explosion and explosion RGB5 sequences.</summary>
    public const string FileName = "power-bomb-glow-colors.json";

    /// <summary>Validates the installed glow colour JSON and keeps both complete RGB5 sequences as installed, without changing phase or radius mechanics.</summary>
    /// <param name="json">UTF-8 JSON source consumed from its current position and left open.</param>
    /// <returns>Every colour of both sequences, detached from the document arrays.</returns>
    /// <exception cref="ArgumentNullException">The source stream is null.</exception>
    /// <exception cref="InvalidDataException">The JSON contains duplicate or unknown properties, an unsupported version, incorrect sequence lengths, null colors, or channels outside 0..31.</exception>
    public static InstalledPowerBombGlowColors Read(Stream json)
    {
        Ensure.NotNull(json);
        PowerBombGlowColorDocument document;
        document = JsonAssetDocument.Read<PowerBombGlowColorDocument>(json, ArtworkJson.Options, "Power Bomb glow colour");
        if (document.Version != Version)
            throw new InvalidDataException("Power Bomb glow colours require the supported version.");
        return new(
            Compile(document.PreExplosion, SamusPaletteDefinitions.PowerBomb.PreExplosionColorCount, "pre-explosion"),
            Compile(document.Explosion, SamusPaletteDefinitions.PowerBomb.ExplosionColorCount, "explosion"));
    }

    /// <summary>Serializes the editable sequences to indented camel-case UTF-8 JSON.</summary>
    /// <param name="document">The two complete RGB5 color sequences to serialize; the writer does not retain their collections.</param>
    /// <returns>JSON bytes for <see cref="FileName"/>.</returns>
    public static byte[] Write(PowerBombGlowColorDocument document) =>
        JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);

    /// <summary>Checks a sequence's length and RGB5 channels before packing its colours.</summary>
    private static RgbColor[] Compile(PaletteRgb5[]? source, int count, string name)
    {
        if (source is null || source.Length != count)
            throw new InvalidDataException($"Power Bomb {name} requires {count} RGB5 colors.");
        var colors = new RgbColor[source.Length];
        for (int index = 0; index < source.Length; index++)
        {
            PaletteRgb5? color = source[index];
            if (color is null || (uint)color.Red > 31 ||
                (uint)color.Green > 31 || (uint)color.Blue > 31)
                throw new InvalidDataException($"Power Bomb {name} color {index} requires components from zero through 31.");
            colors[index] = new FadeLevelColour(color.Red, color.Green, color.Blue).ToColour();
        }
        return colors;
    }
}

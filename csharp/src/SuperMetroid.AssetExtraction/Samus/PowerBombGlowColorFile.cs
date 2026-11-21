using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Colours;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Reads and writes the installed Power Bomb glow colour document.</summary>
public static class PowerBombGlowColorFile
{
    /// <summary>
    /// Supported schema revision, fixing the sixteen- and thirty-two-entry sequence dimensions; 2 names the file for the
    /// explosion's glow (#1631), and 3 writes each colour as <c>#rrggbb</c> (#2564).
    /// </summary>
    public const int Version = 3;

    /// <summary>JSON resource filename containing the pre-explosion and explosion colour sequences.</summary>
    public const string FileName = "power-bomb-glow-colors.json";

    /// <summary>Validates the installed glow colour JSON and keeps both complete colour sequences as installed, without changing phase or radius mechanics.</summary>
    /// <param name="json">UTF-8 JSON source consumed from its current position and left open.</param>
    /// <returns>Every colour of both sequences, detached from the document arrays.</returns>
    /// <exception cref="ArgumentNullException">The source stream is null.</exception>
    /// <exception cref="InvalidDataException">The JSON contains duplicate or unknown properties, an unsupported version, incorrect sequence lengths, or a colour not written as <c>#rrggbb</c> or off the game's fade scale.</exception>
    public static InstalledPowerBombGlowColors Read(Stream json)
    {
        Ensure.NotNull(json);
        PowerBombGlowColorDocument document;
        document = JsonAssetDocument.Read<PowerBombGlowColorDocument>(json, ArtworkJson.Options, "Power Bomb glow colour");
        if (document.Version != Version)
            throw new InvalidDataException("Power Bomb glow colours require the supported version.");
        return new(
            Compile(document.PreExplosion, SamusShadeDefinitions.PowerBomb.PreExplosionColorCount, "pre-explosion"),
            Compile(document.Explosion, SamusShadeDefinitions.PowerBomb.ExplosionColorCount, "explosion"));
    }

    /// <summary>Serializes the editable sequences to indented camel-case UTF-8 JSON.</summary>
    /// <param name="document">The two complete colour sequences to serialize; the writer does not retain their collections.</param>
    /// <returns>JSON bytes for <see cref="FileName"/>.</returns>
    public static byte[] Write(PowerBombGlowColorDocument document) =>
        JsonSerializer.SerializeToUtf8Bytes(document, global::SuperMetroid.AssetExtraction.AssetExtractionJson.Info<global::SuperMetroid.AssetExtraction.Samus.PowerBombGlowColorDocument>(ArtworkJson.Options));

    /// <summary>Checks a sequence's length and reads each colour, which lies on the game's fade scale.</summary>
    private static RgbColor[] Compile(string[]? source, int count, string name)
    {
        if (source is null || source.Length != count)
            throw new InvalidDataException($"Power Bomb {name} requires {count} colours.");
        var colors = new RgbColor[source.Length];
        for (int index = 0; index < source.Length; index++)
            colors[index] = InstalledColourText.ParseOnFadeScale(source[index], $"Power Bomb {name} colour {index}");
        return colors;
    }
}

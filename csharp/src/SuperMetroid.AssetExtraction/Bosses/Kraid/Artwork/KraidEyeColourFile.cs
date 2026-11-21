using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Compatibility;

namespace SuperMetroid.AssetExtraction.Bosses.Kraid.Artwork;

/// <summary>
/// Reads and writes the installed Kraid eye colours (#1637): the only colours of Kraid's the game works with, named by
/// what they colour, without the colour groups and places the cartridge kept them at.
/// </summary>
public static class KraidEyeColourFile
{
    /// <summary>The installed document's file name, beside the enemy compositions.</summary>
    public const string FileName = "kraid-eye-colours.json";

    /// <summary>Supported schema revision: 3 keys each health band's colours by the band (#2565).</summary>
    public const int Version = 3;

    /// <summary>The document holding <paramref name="eyes"/>.</summary>
    /// <param name="eyes">The eyes' colours under the hurt flash and each health band.</param>
    /// <returns>Indented camel-case JSON bytes for <see cref="FileName"/>.</returns>
    public static byte[] Write(KraidEyeColours eyes)
    {
        Ensure.NotNull(eyes);
        return JsonSerializer.SerializeToUtf8Bytes(new KraidEyeColourDocument
        {
            Version = Version,
            HurtFlash = Written(eyes.Hurt),
            HealthBands = KraidHealthBandDefinitions.HighestFirst.ToDictionary(band => band.Key, band => Written(eyes.Of(band)), StringComparer.Ordinal),
        }, JsonOptions);

        static string[] Written(ReadOnlySpan<RgbColor> colours)
        {
            var written = new string[colours.Length];
            for (int eye = 0; eye < colours.Length; eye++)
                written[eye] = InstalledColourText.Format(colours[eye]);
            return written;
        }
    }

    /// <summary>Reads the eye colours a document holds, as installed.</summary>
    /// <param name="json">UTF-8 JSON consumed from its current position and left open.</param>
    /// <returns>The eyes' colours under the hurt flash and each health band.</returns>
    /// <exception cref="InvalidDataException">
    /// The JSON has duplicate or unknown properties, another version, a set other than one colour per eye, other than eight
    /// health bands, or a colour not written as <c>#rrggbb</c> or off the game's fade scale.
    /// </exception>
    public static KraidEyeColours Read(Stream json)
    {
        Ensure.NotNull(json);
        KraidEyeColourDocument document = JsonAssetDocument.Read<KraidEyeColourDocument>(json, JsonOptions, "Kraid eye colours");
        if (document.Version != Version)
            throw new InvalidDataException($"Kraid eye colours require version {Version}.");
        IReadOnlyList<KraidHealthBand> bands = KraidHealthBandDefinitions.HighestFirst;
        if (document.HealthBands is not { } healthBands || healthBands.Count != bands.Count)
            throw new InvalidDataException($"Kraid eye colours require {bands.Count} health bands, each keyed by its band.");
        return new KraidEyeColours(Compile(document.HurtFlash, "hurt flash"),
            bands.ToDictionary(band => band, band => (IReadOnlyList<RgbColor>)Compile(
                healthBands.TryGetValue(band.Key, out string[]? colours) ? colours : throw new InvalidDataException($"Kraid eye colours hold none for his {band} health band."),
                $"{band} health band")));
    }

    /// <summary>One eye colour set's colours, one per eye, each on the game's fade scale.</summary>
    /// <exception cref="InvalidDataException">The set does not hold one colour per eye, or a colour is not written as <c>#rrggbb</c> or is off the scale.</exception>
    private static RgbColor[] Compile(string[]? set, string name)
    {
        if (set is not { Length: KraidEyeGlow.Eyes })
            throw new InvalidDataException($"Kraid's {name} eye colours require one colour for each of his {KraidEyeGlow.Eyes} eyes.");
        var colours = new RgbColor[set.Length];
        for (int eye = 0; eye < set.Length; eye++)
            colours[eye] = InstalledColourText.ParseOnFadeScale(set[eye], $"Kraid's {name} eye {eye} colour");
        return colours;
    }

    /// <summary>Indented camel-case JSON that accepts case-insensitive names and rejects unknown fields.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}

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

    /// <summary>Supported schema revision.</summary>
    public const int Version = 1;

    /// <summary>The document holding <paramref name="eyes"/>.</summary>
    /// <param name="eyes">The eyes' colours under the hurt flash and each health band.</param>
    /// <returns>Indented camel-case JSON bytes for <see cref="FileName"/>.</returns>
    public static byte[] Write(KraidEyeColours eyes)
    {
        Ensure.NotNull(eyes);
        return JsonSerializer.SerializeToUtf8Bytes(new KraidEyeColourDocument
        {
            Version = Version,
            HurtFlash = Levels(eyes.Hurt),
            HealthBands = [.. Enumerable.Range(0, KraidHealthThresholdDefinitions.EighthCount).Select(band => Levels(eyes.HealthBand(band)))],
        }, JsonOptions);

        static PaletteRgb5[] Levels(ReadOnlySpan<RgbColor> colours)
        {
            var levels = new PaletteRgb5[colours.Length];
            for (int eye = 0; eye < colours.Length; eye++)
                levels[eye] = PaletteRgb5.From(colours[eye]);
            return levels;
        }
    }

    /// <summary>Reads the eye colours a document holds, as installed.</summary>
    /// <param name="json">UTF-8 JSON consumed from its current position and left open.</param>
    /// <returns>The eyes' colours under the hurt flash and each health band.</returns>
    /// <exception cref="InvalidDataException">
    /// The JSON has duplicate or unknown properties, another version, a set other than one colour per eye, other than eight
    /// health bands, or channels outside 0..31.
    /// </exception>
    public static KraidEyeColours Read(Stream json)
    {
        Ensure.NotNull(json);
        KraidEyeColourDocument document = JsonAssetDocument.Read<KraidEyeColourDocument>(json, JsonOptions, "Kraid eye colours");
        if (document.Version != Version)
            throw new InvalidDataException($"Kraid eye colours require version {Version}.");
        if (document.HealthBands is not { Length: KraidHealthThresholdDefinitions.EighthCount })
            throw new InvalidDataException($"Kraid eye colours require {KraidHealthThresholdDefinitions.EighthCount} health bands.");
        return new KraidEyeColours(Compile(document.HurtFlash, "hurt flash"),
            [.. document.HealthBands.Select((band, index) => Compile(band, $"health band {index}"))]);
    }

    /// <summary>One eye colour set's colours, one per eye, each channel 0..31.</summary>
    /// <exception cref="InvalidDataException">The set does not hold one colour per eye, or a channel is out of range.</exception>
    private static RgbColor[] Compile(PaletteRgb5[]? set, string name)
    {
        if (set is not { Length: KraidEyeGlow.Eyes })
            throw new InvalidDataException($"Kraid's {name} eye colours require one colour for each of his {KraidEyeGlow.Eyes} eyes.");
        var colours = new RgbColor[set.Length];
        for (int eye = 0; eye < set.Length; eye++)
        {
            PaletteRgb5? levels = set[eye];
            if (levels is null || (uint)levels.Red > 31 || (uint)levels.Green > 31 || (uint)levels.Blue > 31)
                throw new InvalidDataException($"Kraid's {name} eye {eye} colour requires channels 0..31.");
            colours[eye] = levels.ToRgbColor();
        }
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

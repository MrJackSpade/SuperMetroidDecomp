using SuperMetroid.Core;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Bosses.Kraid;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>Reads and writes the installed Kraid colour document.</summary>
public static class KraidColorFile
{
    /// <summary>Validates and compiles all five Kraid RGB5 sources, preserving independent authored edits while leaving health-band selection and fade timing to the boss mechanics.</summary>
    /// <param name="json">UTF-8 JSON source consumed from its current position and left open; property names are matched case-insensitively.</param>
    /// <returns>Compiled packed-color catalog detached from the document arrays.</returns>
    /// <exception cref="ArgumentNullException">The source stream is null.</exception>
    /// <exception cref="InvalidDataException">The JSON has duplicate or unknown properties, an unsupported version, incorrect source lengths, null colors, or RGB5 channels outside 0..31.</exception>
    public static KraidColorCatalog Read(Stream json)
    {
        Ensure.NotNull(json);
        KraidColorDocument document;
        document = JsonAssetDocument.Read<KraidColorDocument>(json, JsonOptions, "Kraid colors");
        if (document.Version != KraidColorFormat.Version)
            throw new InvalidDataException("Kraid colors require the supported version.");
        return new(
            Compile(document.RoomBackdrop, KraidPaletteSource.RoomBackdrop),
            Compile(document.InitialTarget, KraidPaletteSource.InitialTarget),
            Compile(document.Health, KraidPaletteSource.Health),
            Compile(document.Secondary, KraidPaletteSource.Secondary),
            Compile(document.DeathArm, KraidPaletteSource.DeathArm));
    }

    /// <summary>Serializes the five editable sources to indented camel-case UTF-8 JSON and validates the resulting bytes through <see cref="Read"/>.</summary>
    /// <param name="document">Complete palette sources to serialize; the writer does not retain their arrays.</param>
    /// <returns>Validated JSON bytes for <see cref="KraidColorFormat.FileName"/>.</returns>
    /// <exception cref="InvalidDataException">The serialized document fails schema, palette-length, or RGB5-channel validation.</exception>
    public static byte[] Write(KraidColorDocument document) =>
        JsonAssetDocument.WriteValidated(document, JsonOptions, json => _ = Read(json));

    /// <summary>Validates one source's native length and RGB5 channels before packing its colors.</summary>
    private static Bgr555[] Compile(PaletteRgb5[]? source, KraidPaletteSource kind)
    {
        int expected = KraidPaletteRomData.ColorCount(kind);
        if (source is null || source.Length != expected)
            throw new InvalidDataException($"Kraid {kind} requires {expected} colors.");
        var result = new Bgr555[expected];
        for (int index = 0; index < expected; index++)
        {
            PaletteRgb5? rgb = source[index];
            if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                (uint)rgb.Blue > 31)
                throw new InvalidDataException(
                    $"Kraid {kind} color {index} requires RGB5 channels 0..31.");
            result[index] = rgb.ToBgr555();
        }
        return result;
    }

    /// <summary>Writes indented camel-case JSON while accepting case-insensitive property names and rejecting unknown fields.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}

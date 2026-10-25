using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Reads and writes the installed enemy palette sheet document.</summary>
public static class EnemyPaletteSheetFile
{
    /// <summary>Loads a version-one palette document with exactly sixteen non-null RGB5 colors, rejecting duplicate or unknown properties and channels outside 0..31; JSON property names are matched case-insensitively.</summary>
    /// <param name="json">UTF-8 JSON stream containing the selected enemy palette in color-index order.</param>
    /// <returns>An immutable sheet owning the compiled sixteen RGB5 words.</returns>
    public static EnemyPaletteSheet Read(Stream json)
    {
        EnemyPaletteSheetDocument document;
        document = JsonAssetDocument.Read<EnemyPaletteSheetDocument>(json, Options, "enemy palette");
        if (document.Version != 1 || document.Colors is null || document.Colors.Length != EnemyPaletteSheet.ColorCount)
            throw new InvalidDataException("Enemy palette requires version 1 and exactly sixteen RGB5 colors.");
        var compiled = new RgbColor[EnemyPaletteSheet.ColorCount];
        for (int i = 0; i < EnemyPaletteSheet.ColorCount; i++)
        {
            PaletteRgb5? color = document.Colors[i];
            if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
                (uint)color.Blue > 31)
                throw new InvalidDataException($"Enemy palette color {i} requires RGB channels in 0..31.");
            compiled[i] = color.ToRgbColor();
        }
        return new EnemyPaletteSheet(compiled);
    }

    /// <summary>Serializes a palette document as indented camel-case UTF-8 JSON and validates it through <see cref="Read"/> before returning the bytes.</summary>
    /// <param name="document">Version-one document containing sixteen valid RGB5 colors.</param>
    /// <returns>Validated enemy-palette JSON bytes.</returns>
    public static byte[] Write(EnemyPaletteSheetDocument document) =>
        JsonAssetDocument.WriteValidated(document, Options, json => _ = Read(json));

    /// <summary>Requires case-insensitive known property names and writes indented camel-case JSON.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}

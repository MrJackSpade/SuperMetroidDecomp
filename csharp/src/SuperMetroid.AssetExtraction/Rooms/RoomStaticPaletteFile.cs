using SuperMetroid.Core;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Reads and writes the installed room palette document.</summary>
public static class RoomStaticPaletteFile
{
    /// <summary>
    /// Loads one room graphics-set palette from a JSON document containing exactly 128 RGB5 colors.
    /// </summary>
    /// <param name="json">The caller-owned stream containing the palette document.</param>
    /// <returns>The compiled 256-byte little-endian CGRAM transfer.</returns>
    /// <exception cref="InvalidDataException">
    /// The document version, color count, or an RGB5 component is invalid.
    /// </exception>
    public static RoomStaticPalette Read(Stream json)
    {
        RoomStaticPaletteDocument document = JsonAssetDocument.Read<RoomStaticPaletteDocument>(
            json, JsonOptions, "room palette");
        if (document.Version != RoomStaticPaletteFormat.Version ||
            document.Colors is null || document.Colors.Length != RoomStaticPaletteFormat.ColorCount)
            throw new InvalidDataException(
                $"Room palette requires version {RoomStaticPaletteFormat.Version} and " +
                $"{RoomStaticPaletteFormat.ColorCount} RGB5 colors.");

        var colours = new RgbColor[document.Colors.Length];
        for (int index = 0; index < document.Colors.Length; index++)
        {
            PaletteRgb5? color = document.Colors[index];
            if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
                (uint)color.Blue > 31)
                throw new InvalidDataException(
                    $"Room palette color {index} requires red, green and blue in 0..31.");
            colours[index] = color.ToRgbColor();
        }
        return RoomStaticPalette.FromColours(colours);
    }

    /// <summary>Validates and writes one editable room-palette document as JSON.</summary>
    /// <param name="json">The caller-owned destination stream.</param>
    /// <param name="document">The palette document to validate and serialize.</param>
    public static void Write(Stream json, RoomStaticPaletteDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Read(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }

    /// <summary>Uses camel-case JSON and rejects unknown properties so palette schema mistakes fail during loading.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow,
    };
}

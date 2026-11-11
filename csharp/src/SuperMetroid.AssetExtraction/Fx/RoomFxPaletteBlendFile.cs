using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Fx.Artwork;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>Reads and writes the installed room-FX blend colour document.</summary>
public static class RoomFxPaletteBlendFile
{
    /// <summary>Loads all eight three-color room-FX blends, rejecting duplicate or unknown JSON properties, incomplete selections, and RGB components outside 0..31.</summary>
    /// <param name="json">Caller-owned stream containing the supported blend-palette JSON document.</param>
    /// <returns>Installed BG3 inks, which only the import reads; native selectors, CGRAM destinations, and fade timing remain compiled mechanics.</returns>
    /// <exception cref="InvalidDataException">The JSON, schema version, blend identities, color count, or RGB5 values are invalid.</exception>
    public static RoomFxBlendInks Read(Stream json)
    {
        Ensure.NotNull(json);
        RoomFxPaletteBlendDocument document;
        document = JsonAssetDocument.Read<RoomFxPaletteBlendDocument>(json, ArtworkJson.Options, "room-FX blend palette");
        if (document.Version != RoomFxPaletteBlendDefinitions.Version ||
            document.Blends is null || document.Blends.Count != RoomFxPaletteBlendDocumentRules.Ids.Count())
            throw new InvalidDataException("Room-FX blend palettes require the supported version and all eight selections.");

        return new(RoomFxPaletteBlendDocumentRules.Ids.ToDictionary(id => id, Compile));

        RgbColor[] Compile(RoomFxPaletteBlend id)
        {
            if (!document.Blends.TryGetValue(RoomFxPaletteBlendDocumentRules.Key(id), out PaletteRgb5[]? colors) ||
                colors is null || colors.Length != RoomFxPaletteBlendDefinitions.ColorCount)
                throw new InvalidDataException($"Room-FX blend {id} requires three colors.");
            var words = new RgbColor[colors.Length];
            for (int index = 0; index < colors.Length; index++)
            {
                PaletteRgb5? color = colors[index];
                if (color is null || (uint)color.Red > 31 ||
                    (uint)color.Green > 31 || (uint)color.Blue > 31)
                    throw new InvalidDataException($"Room-FX blend {id} color {index} requires RGB components from zero through 31.");
                words[index] = color.ToRgbColor();
            }
            return words;
        }
    }

    /// <summary>Serializes a complete blend-palette document as UTF-8 JSON.</summary>
    /// <param name="document">Eight named blend selections.</param>
    /// <returns>The UTF-8 JSON payload; this method does not write an external resource.</returns>
    public static byte[] Write(RoomFxPaletteBlendDocument document) =>
        JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
}

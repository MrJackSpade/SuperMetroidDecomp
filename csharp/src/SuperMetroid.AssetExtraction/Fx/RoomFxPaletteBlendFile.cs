using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>Reads and writes the installed room-FX blend colour document.</summary>
public static class RoomFxPaletteBlendFile
{
    /// <summary>Loads all eight three-color room-FX blends and optional Ceres haze tints, rejecting duplicate or unknown JSON properties, incomplete selections, and RGB components outside 0..31.</summary>
    /// <param name="json">Caller-owned stream containing the supported blend-palette JSON document.</param>
    /// <returns>Installed BG3 inks plus fixed-color haze tints; native selectors, CGRAM destinations, and fade timing remain compiled mechanics.</returns>
    /// <exception cref="InvalidDataException">The JSON, schema version, blend identities, color count, or RGB5 values are invalid.</exception>
    public static RoomFxPaletteBlendCatalog Read(Stream json)
    {
        Ensure.NotNull(json);
        RoomFxPaletteBlendDocument document;
        document = JsonAssetDocument.Read<RoomFxPaletteBlendDocument>(json, ArtworkJson.Options, "room-FX blend palette");
        if (document.Version != RoomFxPaletteBlendDefinitions.Version ||
            document.Blends is null || document.Blends.Count != RoomFxPaletteBlendDefinitions.Ids.Count())
            throw new InvalidDataException("Room-FX blend palettes require the supported version and all eight selections.");

        return new(new RoomFxPaletteBlendParts { Lava = Compile(RoomFxPaletteBlend.Lava), LandingSiteRain = Compile(RoomFxPaletteBlend.LandingSiteRain), MaridiaWaterA = Compile(RoomFxPaletteBlend.MaridiaWaterA), WaterAndAcid = Compile(RoomFxPaletteBlend.WaterAndAcid), Fog = Compile(RoomFxPaletteBlend.Fog), MaridiaWaterB = Compile(RoomFxPaletteBlend.MaridiaWaterB), MaridiaWaterC = Compile(RoomFxPaletteBlend.MaridiaWaterC), MaridiaWaterD = Compile(RoomFxPaletteBlend.MaridiaWaterD), CeresHazeBlue = HazeTint(document.CeresHazeBlue, RoomFxPaletteBlendDefinitions.StockCeresHazeBlue,
                nameof(document.CeresHazeBlue)), CeresHazeRed = HazeTint(document.CeresHazeRed, RoomFxPaletteBlendDefinitions.StockCeresHazeRed,
                nameof(document.CeresHazeRed)) });

        RoomFxBlendColors Compile(RoomFxPaletteBlend id)
        {
            if (!document.Blends.TryGetValue(RoomFxPaletteBlendDefinitions.Key(id), out PaletteRgb5[]? colors) ||
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
            return new(id, words[0], words[1], words[2]);
        }
    }

    /// <summary>Serializes a complete blend-palette document as UTF-8 JSON and validates it through <see cref="Read"/> before returning the bytes.</summary>
    /// <param name="document">Eight named blend selections and optional haze tint overrides.</param>
    /// <returns>The validated UTF-8 JSON payload; this method does not write an external resource.</returns>
    public static byte[] Write(RoomFxPaletteBlendDocument document) =>
        JsonAssetDocument.WriteValidated(document, ArtworkJson.Options, json => _ = Read(json));

    /// <summary>The authored Ceres fixed-color tint, or the cartridge tint an older document omits; rejects a component outside RGB5 range.</summary>
    private static RgbColor HazeTint(PaletteRgb5? color, RgbColor stock, string name)
    {
        if (color is null) return stock;
        if ((uint)color.Red > 31 || (uint)color.Green > 31 || (uint)color.Blue > 31)
            throw new InvalidDataException($"{name} requires RGB components from zero through 31.");
        return color.ToRgbColor();
    }
}

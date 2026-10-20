using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>Editable three-color room-FX blends selected by native FX records.</summary>
public sealed class RoomFxPaletteBlendCatalog
{
    /// <summary>Three installed CGRAM inks for the lava selector.</summary>
    private readonly RoomFxBlendColors lava;
    /// <summary>Three installed CGRAM inks for Landing Site rain.</summary>
    private readonly RoomFxBlendColors landingSiteRain;
    /// <summary>Three installed inks for Maridia's first water selector.</summary>
    private readonly RoomFxBlendColors maridiaWaterA;
    /// <summary>Three installed inks shared by the water and acid selector.</summary>
    private readonly RoomFxBlendColors waterAndAcid;
    /// <summary>Three installed inks for the fog selector.</summary>
    private readonly RoomFxBlendColors fog;
    /// <summary>Three installed inks for Maridia's second water selector.</summary>
    private readonly RoomFxBlendColors maridiaWaterB;
    /// <summary>Three installed inks for Maridia's third water selector.</summary>
    private readonly RoomFxBlendColors maridiaWaterC;
    /// <summary>Three installed inks for Maridia's fourth water selector.</summary>
    private readonly RoomFxBlendColors maridiaWaterD;

    /// <summary>Retains each validated room-FX ink and both optional Ceres haze tints.</summary>
    private RoomFxPaletteBlendCatalog(RoomFxPaletteBlendParts parts)
    {
        this.lava = parts.Lava;
        this.landingSiteRain = parts.LandingSiteRain;
        this.maridiaWaterA = parts.MaridiaWaterA;
        this.waterAndAcid = parts.WaterAndAcid;
        this.fog = parts.Fog;
        this.maridiaWaterB = parts.MaridiaWaterB;
        this.maridiaWaterC = parts.MaridiaWaterC;
        this.maridiaWaterD = parts.MaridiaWaterD;
        CeresHazeBlue = parts.CeresHazeBlue;
        CeresHazeRed = parts.CeresHazeRed;
    }

    /// <summary>Cosmetic fixed-color tint selected when Ceres Ridley is alive.</summary>
    public PaletteRgb5 CeresHazeBlue { get; }

    /// <summary>Cosmetic fixed-color tint selected after Ceres Ridley is defeated.</summary>
    public PaletteRgb5 CeresHazeRed { get; }

    /// <summary>Loads all eight three-color room-FX blends and optional Ceres haze tints, rejecting duplicate or unknown JSON properties, incomplete selections, and RGB components outside 0..31.</summary>
    /// <param name="json">Caller-owned stream containing the supported blend-palette JSON document.</param>
    /// <returns>Installed BG3 inks plus fixed-color haze tints; native selectors, CGRAM destinations, and fade timing remain compiled mechanics.</returns>
    /// <exception cref="InvalidDataException">The JSON, schema version, blend identities, color count, or RGB5 values are invalid.</exception>
    public static RoomFxPaletteBlendCatalog Load(Stream json)
    {
        Ensure.NotNull(json);
        RoomFxPaletteBlendDocument document;
        document = JsonAssetDocument.Read<RoomFxPaletteBlendDocument>(json, ArtworkJson.Options, "room-FX blend palette");
        if (document.Version != RoomFxPaletteBlendDefinitions.Version ||
            document.Blends is null || document.Blends.Count != RoomFxPaletteBlendDefinitions.Ids.Count())
            throw new InvalidDataException("Room-FX blend palettes require the supported version and all eight selections.");

        return new(new RoomFxPaletteBlendParts { Lava = Compile(RoomFxPaletteBlend.Lava), LandingSiteRain = Compile(RoomFxPaletteBlend.LandingSiteRain), MaridiaWaterA = Compile(RoomFxPaletteBlend.MaridiaWaterA), WaterAndAcid = Compile(RoomFxPaletteBlend.WaterAndAcid), Fog = Compile(RoomFxPaletteBlend.Fog), MaridiaWaterB = Compile(RoomFxPaletteBlend.MaridiaWaterB), MaridiaWaterC = Compile(RoomFxPaletteBlend.MaridiaWaterC), MaridiaWaterD = Compile(RoomFxPaletteBlend.MaridiaWaterD), CeresHazeBlue = ValidateHazeTint(document.CeresHazeBlue ?? RoomFxPaletteBlendDefinitions.StockCeresHazeBlue,
                nameof(document.CeresHazeBlue)), CeresHazeRed = ValidateHazeTint(document.CeresHazeRed ?? RoomFxPaletteBlendDefinitions.StockCeresHazeRed,
                nameof(document.CeresHazeRed)) });

        RoomFxBlendColors Compile(RoomFxPaletteBlend id)
        {
            if (!document.Blends.TryGetValue(RoomFxPaletteBlendDefinitions.Key(id), out PaletteRgb5[]? colors) ||
                colors is null || colors.Length != RoomFxRomData.Layer3.PaletteBlendColorCount)
                throw new InvalidDataException($"Room-FX blend {(int)id:X2} requires three colors.");
            var words = new Bgr555[colors.Length];
            for (int index = 0; index < colors.Length; index++)
            {
                PaletteRgb5? color = colors[index];
                if (color is null || (uint)color.Red > 31 ||
                    (uint)color.Green > 31 || (uint)color.Blue > 31)
                    throw new InvalidDataException($"Room-FX blend {(int)id:X2} color {index} requires RGB components from zero through 31.");
                words[index] = color.ToBgr555();
            }
            return new(id, words[0], words[1], words[2]);
        }
    }

    /// <summary>Serializes a complete blend-palette document as UTF-8 JSON and validates it through <see cref="Load"/> before returning the bytes.</summary>
    /// <param name="document">Eight named blend selections and optional haze tint overrides.</param>
    /// <returns>The validated UTF-8 JSON payload; this method does not write an external resource.</returns>
    public static byte[] Write(RoomFxPaletteBlendDocument document) =>
        JsonAssetDocument.WriteValidated(document, ArtworkJson.Options, json => _ = Load(json));

    /// <summary>Applies the native three-color write, or clears only color 27 for selection zero.</summary>
    public void Apply(IPaletteColors cgram, RoomFxPaletteBlend selection)
    {
        Ensure.NotNull(cgram);
        if (selection == RoomFxPaletteBlend.None)
        {
            new RoomFxPalettes(cgram).Layer3Empty.Write(0, Bgr555.Black);
            return;
        }
        SelectColors(selection).Apply(cgram);
    }

    /// <summary>Resolves one authored blend selector to its installed three-color record.</summary>
    internal RoomFxBlendColors SelectColors(RoomFxPaletteBlend selection) => selection switch
    {
        RoomFxPaletteBlend.Lava => lava,
        RoomFxPaletteBlend.LandingSiteRain => landingSiteRain,
        RoomFxPaletteBlend.MaridiaWaterA => maridiaWaterA,
        RoomFxPaletteBlend.WaterAndAcid => waterAndAcid,
        RoomFxPaletteBlend.Fog => fog,
        RoomFxPaletteBlend.MaridiaWaterB => maridiaWaterB,
        RoomFxPaletteBlend.MaridiaWaterC => maridiaWaterC,
        RoomFxPaletteBlend.MaridiaWaterD => maridiaWaterD,
        RoomFxPaletteBlend.None => throw new InvalidDataException($"Room-FX palette blend ${(int)selection:X2} is not an authored retail selection."),
        _ => throw new InvalidDataException($"Room-FX palette blend ${(int)selection:X2} is not an authored retail selection."),
    };
    /// <summary>Rejects a Ceres fixed-color tint with a component outside RGB5 range.</summary>
    private static PaletteRgb5 ValidateHazeTint(PaletteRgb5 color, string name)
    {
        if ((uint)color.Red > 31 || (uint)color.Green > 31 || (uint)color.Blue > 31)
            throw new InvalidDataException($"{name} requires RGB components from zero through 31.");
        return color;
    }

}

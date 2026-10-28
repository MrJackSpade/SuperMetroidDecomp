using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>Editable three-color room-FX blends selected by native FX records.</summary>
public sealed class RoomFxBlendInks
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
    internal RoomFxBlendInks(RoomFxPaletteBlendParts parts)
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
    public RgbColor CeresHazeBlue { get; }

    /// <summary>Cosmetic fixed-color tint selected after Ceres Ridley is defeated.</summary>
    public RgbColor CeresHazeRed { get; }

    /// <summary>Applies the native three-color write, or clears only color 27 for selection zero.</summary>
    public void Apply(BackgroundColourSets colours, RoomFxPaletteBlend selection)
    {
        Ensure.NotNull(colours);
        if (selection == RoomFxPaletteBlend.None)
        {
            RoomFxColourDefinitions.Layer3Empty.In(colours).Fill(RgbColor.Black);
            return;
        }
        SelectColors(selection).Apply(RoomFxColourDefinitions.Layer3Blend.In(colours));
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
        RoomFxPaletteBlend.None => throw new InvalidDataException($"Room-FX palette blend {selection} is not an authored retail selection."),
        _ => throw new InvalidDataException($"Room-FX palette blend {selection} is not an authored retail selection."),
    };
}

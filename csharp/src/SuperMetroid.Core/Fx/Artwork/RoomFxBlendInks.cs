using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>Editable three-color room-FX blends selected by FX records.</summary>
public sealed class RoomFxBlendInks
{
    /// <summary>Three installed inks for the lava selector.</summary>
    internal readonly RoomFxBlendColors lava;
    /// <summary>Three installed inks for Landing Site rain.</summary>
    internal readonly RoomFxBlendColors landingSiteRain;
    /// <summary>Three installed inks for Maridia's first water selector.</summary>
    internal readonly RoomFxBlendColors maridiaWaterA;
    /// <summary>Three installed inks shared by the water and acid selector.</summary>
    internal readonly RoomFxBlendColors waterAndAcid;
    /// <summary>Three installed inks for the fog selector.</summary>
    internal readonly RoomFxBlendColors fog;
    /// <summary>Three installed inks for Maridia's second water selector.</summary>
    internal readonly RoomFxBlendColors maridiaWaterB;
    /// <summary>Three installed inks for Maridia's third water selector.</summary>
    internal readonly RoomFxBlendColors maridiaWaterC;
    /// <summary>Three installed inks for Maridia's fourth water selector.</summary>
    internal readonly RoomFxBlendColors maridiaWaterD;

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
}

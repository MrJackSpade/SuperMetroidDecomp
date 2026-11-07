using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.AssetExtraction.Fx.Artwork;

/// <summary>
/// The installed room-FX blends selected by FX records (#1512): the import pre-colours the effect overlay pictures in
/// each selection's three inks. The game reads only the Ceres haze tints (<see cref="CeresHaze"/>).
/// </summary>
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

    /// <summary>Retains each validated room-FX ink and both Ceres haze tints.</summary>
    internal RoomFxBlendInks(RoomFxPaletteBlendParts parts)
    {
        lava = parts.Lava;
        landingSiteRain = parts.LandingSiteRain;
        maridiaWaterA = parts.MaridiaWaterA;
        waterAndAcid = parts.WaterAndAcid;
        fog = parts.Fog;
        maridiaWaterB = parts.MaridiaWaterB;
        maridiaWaterC = parts.MaridiaWaterC;
        maridiaWaterD = parts.MaridiaWaterD;
        CeresHaze = new CeresHazeTints(parts.CeresHazeBlue, parts.CeresHazeRed);
    }

    /// <summary>The Ceres haze's installed tints, which the game reads.</summary>
    public CeresHazeTints CeresHaze { get; }

    /// <summary>
    /// The colours <paramref name="selection"/> loads for the effect overlay's three blend inks, in order (#1512): the
    /// import pre-colours the effect overlay pictures in each selection's. The empty selection loads only the third, black.
    /// </summary>
    internal IReadOnlyList<RgbColor> Inks(RoomFxPaletteBlend selection) => selection == RoomFxPaletteBlend.None
        ? [RgbColor.Black]
        : SelectColors(selection).Colours();

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

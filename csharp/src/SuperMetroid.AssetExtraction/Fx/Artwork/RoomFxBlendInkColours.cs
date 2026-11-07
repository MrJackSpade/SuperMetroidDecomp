using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.AssetExtraction.Fx.Artwork;

/// <summary>
/// The colours each room-FX palette blend loads into the effect overlay's three blend inks (#1512), rebuilt from the
/// installed blends' edits and the shared tint rules; the import pre-colours the effect overlay pictures in each
/// selection's. The game reads only the Ceres haze tints.
/// </summary>
internal static class RoomFxBlendInkColours
{
    /// <summary>Rebuilding one of the first two blend colours.</summary>
    extension(RoomFxPairColor pair)
    {
        /// <summary>Rebuilds the ink from retained edits and any still-applicable shared tint rules.</summary>
        internal RgbColor CreateColor()
        {
            int red = pair.redOverride ?? RoomFxPaletteBlendRules.CalculatedPairRed(pair.selection, pair.isPrimary) ??
                throw new InvalidOperationException("A blend color without a red edit has a calculated red.");
            int green = pair.greenOverride ?? RoomFxPaletteBlendRules.CalculatedPairGreen(pair.selection, red, pair.isPrimary) ??
                throw new InvalidOperationException("A blend color without a green edit has a calculated green.");
            int blue = pair.blueOverride ?? RoomFxPaletteBlendRules.CalculatedPairBlue(pair.selection, red, green, pair.isPrimary) ??
                throw new InvalidOperationException("A blend color without a blue edit has a calculated blue.");
            return new RgbColor(red, green, blue);
        }
    }

    /// <summary>Rebuilding the third blend colour.</summary>
    extension(RoomFxThirdColor third)
    {
        /// <summary>Combines the retained red channel with edited or calculated weather channels.</summary>
        internal RgbColor CreateColor() => new(third.red,
            third.greenOverride ?? RoomFxPaletteBlendRules.CalculatedThirdGreen(third.selection) ??
                throw new InvalidOperationException("A blend color without a green edit has a calculated green."),
            third.blueOverride ?? RoomFxPaletteBlendRules.CalculatedThirdBlue(third.selection, third.red) ??
                throw new InvalidOperationException("A blend color without a blue edit has a calculated blue."));
    }

    /// <summary>Rebuilding one selection's three blend colours.</summary>
    extension(RoomFxBlendColors blend)
    {
        /// <summary>The three resulting blend colours, in order.</summary>
        internal IReadOnlyList<RgbColor> Colours() =>
            [blend.primary.CreateColor(), blend.secondary.CreateColor(), blend.thirdOverride?.CreateColor() ?? RgbColor.Black];
    }

    /// <summary>Selecting the blend a room-FX record names.</summary>
    extension(RoomFxBlendInks blends)
    {
        /// <summary>
        /// The colours <paramref name="selection"/> loads for the effect overlay's three blend inks, in order (#1512): the
        /// import pre-colours the effect overlay pictures in each selection's. The empty selection loads only the third, black.
        /// </summary>
        internal IReadOnlyList<RgbColor> Inks(RoomFxPaletteBlend selection) => selection == RoomFxPaletteBlend.None
            ? [RgbColor.Black]
            : blends.SelectColors(selection).Colours();

        /// <summary>Resolves one authored blend selector to its installed three-color record.</summary>
        internal RoomFxBlendColors SelectColors(RoomFxPaletteBlend selection) => selection switch
        {
            RoomFxPaletteBlend.Lava => blends.lava,
            RoomFxPaletteBlend.LandingSiteRain => blends.landingSiteRain,
            RoomFxPaletteBlend.MaridiaWaterA => blends.maridiaWaterA,
            RoomFxPaletteBlend.WaterAndAcid => blends.waterAndAcid,
            RoomFxPaletteBlend.Fog => blends.fog,
            RoomFxPaletteBlend.MaridiaWaterB => blends.maridiaWaterB,
            RoomFxPaletteBlend.MaridiaWaterC => blends.maridiaWaterC,
            RoomFxPaletteBlend.MaridiaWaterD => blends.maridiaWaterD,
            RoomFxPaletteBlend.None => throw new InvalidDataException($"Room-FX palette blend {selection} is not an authored retail selection."),
            _ => throw new InvalidDataException($"Room-FX palette blend {selection} is not an authored retail selection."),
        };
    }
}

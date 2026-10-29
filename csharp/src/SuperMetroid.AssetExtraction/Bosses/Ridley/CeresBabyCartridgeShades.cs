using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.Ridley;

namespace SuperMetroid.AssetExtraction.Bosses.Ridley;

/// <summary>
/// The cartridge's order of the Ceres Baby's colour steps, which the import alone knows: resting, then the horizontal
/// squish, the round pose and the vertical squish, each brighter than the one before. Older colour documents stored the
/// same ordered steps.
/// </summary>
public static class CeresBabyCartridgeShades
{
    /// <summary>Names each of the four ordered colour steps by the shade it holds.</summary>
    /// <param name="steps">The Baby's colour steps in cartridge order.</param>
    public static CeresBabyShadeColorsDocument FromSteps(PaletteRgb5[][] steps) => Ensure.NotNull(steps) switch
    {
        [var resting, var horizontalSquish, var round, var verticalSquish] => new CeresBabyShadeColorsDocument
        {
            Resting = resting,
            HorizontalSquish = horizontalSquish,
            Round = round,
            VerticalSquish = verticalSquish,
        },
        _ => throw new InvalidDataException(
            $"Ceres Ridley Baby requires {CeresRidleyPaletteDefinitions.BabyShadeCount} colour steps."),
    };
}

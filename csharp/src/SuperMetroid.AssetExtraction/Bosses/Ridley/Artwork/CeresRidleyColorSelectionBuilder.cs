using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Ridley.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork;

/// <summary>
/// Compiles Ceres Ridley's supplied colour families into the selection the game reads: the fades, health, alarm and Baby
/// paint, and the retreat colours kept only where they differ from the startup and fade colours they repeat.
/// </summary>
internal static class CeresRidleyColorSelectionBuilder
{
    /// <summary>
    /// Builds the selected Ceres Ridley colours from their supplied families; a missing alarm or Baby family, which older
    /// overrides predate, is taken from <paramref name="stock"/>.
    /// </summary>
    /// <param name="supplied">The supplied colour families.</param>
    /// <param name="stock">The current stock colours a missing family is inherited from.</param>
    internal static CeresRidleyColorSelection Select(CeresRidleySuppliedColors supplied, CeresRidleyColorCatalog? stock)
    {
        Ensure.NotNull(supplied);
        CeresRidleyFade bodyFade = CeresRidleyPaintRecognition.Fade(CeresRidleyFadeKind.Body, supplied.BodyFade);
        CeresRidleyFade eyeFade = CeresRidleyPaintRecognition.Fade(CeresRidleyFadeKind.Eyes, supplied.EyeFade);
        CeresBabyPaint baby = supplied.Baby is null ? Ensure.NotNull(stock).selection.Baby : CeresRidleyPaintRecognition.Baby(supplied.Baby, bodyFade);
        CeresRidleyStartColors start = CeresRidleyPaintRecognition.Start(supplied.Start, baby);
        return new CeresRidleyColorSelection
        {
            Start = start,
            EyeFade = eyeFade,
            BodyFade = bodyFade,
            Health = CeresRidleyPaintRecognition.Health(supplied.Health, bodyFade, eyeFade),
            Alarm = supplied.Alarm is null ? Ensure.NotNull(stock).selection.Alarm : CeresRidleyPaintRecognition.Alarm(supplied.Alarm),
            RetreatBg = RetreatBgEdits(supplied.RetreatBg, bodyFade, eyeFade),
            RetreatShared = RetreatSharedEdits(supplied.RetreatShared, start),
            Baby = baby,
        };
    }

    /// <summary>Keeps the supplied retreat BG colours that differ from their body, eye and neutral composition.</summary>
    private static Dictionary<ColourCell, RgbColor> RetreatBgEdits(RgbColor[] retreatBg, CeresRidleyFade bodyFade, CeresRidleyFade eyeFade)
    {
        var edits = new Dictionary<ColourCell, RgbColor>();
        for (int color = 0; color < retreatBg.Length; color++)
            if (retreatBg[color] != CeresRidleyColors.CalculateRetreatBg(bodyFade, eyeFade, color))
                edits.Add(new ColourCell(0, color), retreatBg[color]);
        return edits;
    }

    /// <summary>
    /// The retreat copies the normal door/container paints into both drawing domains; keeps only independent supplied
    /// differences from the existing startup owner.
    /// </summary>
    private static Dictionary<ColourCell, RgbColor> RetreatSharedEdits(RgbColor[] retreatShared, CeresRidleyStartColors start)
    {
        var edits = new Dictionary<ColourCell, RgbColor>();
        for (int color = 0; color < retreatShared.Length; color++)
            if (retreatShared[color] != start.Resolve(color + 1))
                edits.Add(new ColourCell(0, color), retreatShared[color]);
        return edits;
    }
}

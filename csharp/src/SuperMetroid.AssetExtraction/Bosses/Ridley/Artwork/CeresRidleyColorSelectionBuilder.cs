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
        var bodyFade = new CeresRidleyFade(CeresRidleyFadeKind.Body, supplied.BodyFade);
        var eyeFade = new CeresRidleyFade(CeresRidleyFadeKind.Eyes, supplied.EyeFade);
        CeresBabyPaint baby = supplied.Baby is null ? Ensure.NotNull(stock).selection.Baby : new CeresBabyPaint(supplied.Baby, bodyFade);
        var start = new CeresRidleyStartColors(supplied.Start, baby);
        return new CeresRidleyColorSelection
        {
            Start = start,
            EyeFade = eyeFade,
            BodyFade = bodyFade,
            Health = new CeresRidleyHealthPaint(supplied.Health, bodyFade, eyeFade),
            Alarm = supplied.Alarm is null ? Ensure.NotNull(stock).selection.Alarm : new CeresRidleyAlarmColors(supplied.Alarm),
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

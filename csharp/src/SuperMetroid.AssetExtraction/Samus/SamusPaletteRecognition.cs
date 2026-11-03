using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Compares Samus's supplied beam, Crystal Flash and hurt colours with their calculated paint and keeps only the colours
/// the calculation does not reproduce, the form the game resolves those colours from.
/// </summary>
internal static class SamusPaletteRecognition
{
    /// <summary>Stores independently edited beam colors while deriving unchanged words from stock paint.</summary>
    internal static InstalledBeamPalettes Beams(RgbColor[][] selections)
    {
        var palettes = new Dictionary<TableColourPlace, RgbColor>();
        for (int index = 0; index < selections.Length; index++)
            for (int color = 0; color < BeamPaletteDefinitions.ColorCount; color++)
                if (selections[index][color] != BeamPaint.Color(SamusBeamCombinations.FromTableIndex(index), color))
                    palettes.Add(new TableColourPlace(index, color), selections[index][color]);
        return new(palettes);
    }

    /// <summary>
    /// Discards stock Crystal Flash body frames only after checking every supplied color, and keeps only the bubble
    /// samples the rotating ramp does not reproduce, the independently painted stock white among them.
    /// </summary>
    internal static InstalledCrystalFlashColors CrystalFlash(RgbColor[][] body, RgbColor[][] bubble)
    {
        bool calculated = true;
        for (int frame = 0; frame < CrystalFlashColorFormat.BodyFrameCount; frame++)
            for (int color = 0; color < CrystalFlashColorFormat.BodyColorCount; color++)
                calculated &= body[frame][color] == InstalledCrystalFlashColors.CalculateBody(frame, color);
        var bubbleEdits = new Dictionary<ColourCell, RgbColor>();
        for (int frame = 0; frame < CrystalFlashColorFormat.BubbleFrameCount; frame++)
            for (int color = 0; color < CrystalFlashColorFormat.BubbleColorCount; color++)
                if (bubble[frame][color] != InstalledCrystalFlashColors.CalculateBubble(frame, color))
                    bubbleEdits.Add(new ColourCell(frame, color), bubble[frame][color]);
        return new(calculated ? null : body, bubbleEdits);
    }

    /// <summary>Retains only independent hurt and intro palette edits, deriving unchanged stock shade relationships.</summary>
    internal static InstalledSamusHurtColors Hurt(RgbColor[] hurt, RgbColor[] intro)
    {
        Dictionary<int, int> introLevels = Enumerable.Range(1, 15)
            .Where(index => intro[index].Red != SamusHurtColorLevels.DefaultIntroLevel(index))
            .ToDictionary(index => index, index => intro[index].Red);
        Dictionary<TableColourPlace, RgbColor> introOverrides = Enumerable.Range(1, 15)
            .Where(index => intro[index] != SamusHurtColorLevels.IntroFromLevel(
                introLevels.TryGetValue(index, out int level) ? level : SamusHurtColorLevels.DefaultIntroLevel(index)))
            .ToDictionary(index => new TableColourPlace(0, index), index => intro[index]);
        Dictionary<TableColourPlace, RgbColor> hurtOverrides = Enumerable.Range(1, 15)
            .Where(index => hurt[index] != SamusHurtColorLevels.HurtFromIntro(intro[index]))
            .ToDictionary(index => new TableColourPlace(0, index), index => hurt[index]);
        return new(hurt[0] == SamusHurtColorDefinitions.HurtTransparentWord ? null : hurt[0],
            intro[0] == SamusHurtColorDefinitions.IntroTransparentWord ? null : intro[0],
            introLevels, introOverrides, hurtOverrides);
    }
}

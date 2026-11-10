using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Compares Samus's supplied Crystal Flash and hurt colours with their calculated paint and keeps only the colours the
/// calculation does not reproduce, the form the game resolves those colours from.
/// </summary>
internal static class SamusPaletteRecognition
{
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
        var bubbleEdits = new Dictionary<PaintColour, RgbColor>();
        for (int frame = 0; frame < CrystalFlashColorFormat.BubbleFrameCount; frame++)
            for (int color = 0; color < CrystalFlashColorFormat.BubbleColorCount; color++)
                if (bubble[frame][color] != InstalledCrystalFlashColors.CalculateBubble(frame, color))
                    bubbleEdits.Add(new PaintColour(frame, color), bubble[frame][color]);
        return new(calculated ? null : body, bubbleEdits);
    }

    /// <summary>Retains only independent hurt and intro palette edits, deriving unchanged stock shade relationships.</summary>
    internal static InstalledSamusHurtColors Hurt(RgbColor[] hurt, RgbColor[] intro)
    {
        Dictionary<int, int> introLevels = Enumerable.Range(1, 15)
            .Where(index => intro[index].FadeLevels.Red != SamusHurtColorLevels.DefaultIntroLevel(index))
            .ToDictionary(index => index, index => intro[index].FadeLevels.Red);
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

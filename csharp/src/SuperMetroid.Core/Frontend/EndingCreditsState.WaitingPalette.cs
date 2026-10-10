using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

internal sealed partial class EndingCreditsState
{
    private void ApplyWaitingBackdropPalette(int step)
        => ApplyPostCreditsPaletteRange(EndingCreditsRomData.Rendering.WaitingPaletteStart,
            EndingCreditsRomData.Rendering.WaitingPaletteCount, step);

    private int rewardPaletteStep;

    private void ApplyRewardPalette()
    {
        if (EndingReward == EndingReward.Armored) return;
        ApplyWaitingBackdropPalette(EndingCreditsRomData.Timing.WaitingPaletteFadeFrames - rewardPaletteStep);
        ApplyPostCreditsPaletteRange(EndingCreditsRomData.Rendering.SuitlessRewardPaletteStart,
            EndingCreditsRomData.Rendering.WaitingPaletteCount, rewardPaletteStep);
        if (EndingReward == EndingReward.Helmetless)
            ApplyPostCreditsPaletteRange(EndingCreditsRomData.Rendering.SuitedRewardPaletteStart,
                EndingCreditsRomData.Rendering.WaitingPaletteCount, rewardPaletteStep);
    }

    private void ApplyPostCreditsPaletteRange(int start, int count, int step)
    {
        // E110 saves Intro4 as its target, clears colors $20-$2F, then E158 runs the
        // shared 8CB2 component accumulator 32 times. Each increment is component<<3
        // in 8.8 precision; composing takes only its high byte, preserving truncation.
        for (int index = start; index < start + count; index++)
        {
            Bgr555 target = StaticPaletteColor(EndingPaletteId.PostCredits, index);
            cgram.SetColor(index, target.Map((_, channel) => (channel << 3) * step >> 8));
        }
    }
}

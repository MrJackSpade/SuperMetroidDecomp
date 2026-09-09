using SuperMetroid.Core.Rom;

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
            ushort target = RomDataReader.ReadWordFixedBank(bus,
                EndingCreditsRomData.Assets.PostCreditsPalette + index * sizeof(ushort));
            int red = ((target & 31) << 3) * step;
            int green = ((target >> 5 & 31) << 3) * step;
            int blue = ((target >> 10 & 31) << 3) * step;
            cgram.SetColor(index, (ushort)((red >> 8) | ((green >> 8) << 5) | ((blue >> 8) << 10)));
        }
    }
}

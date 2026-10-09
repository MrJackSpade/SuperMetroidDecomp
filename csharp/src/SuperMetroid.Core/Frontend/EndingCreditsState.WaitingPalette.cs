using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

internal sealed partial class EndingCreditsState
{
    /// <summary>Applies one fixed-point fade step to the sixteen-color post-credits waiting backdrop range.</summary>
    /// <param name="step">Component-fade multiplier used for the current transition step.</param>
    private void ApplyWaitingBackdropPalette(int step)
        => ApplyPostCreditsPaletteRange(EndingCreditsRomData.Rendering.WaitingPaletteStart,
            EndingCreditsRomData.Rendering.WaitingPaletteCount, step);

    /// <summary>Current transition step used to fade reward-specific suit colors in and the waiting backdrop out.</summary>
    private int rewardPaletteStep;

    /// <summary>Updates the waiting backdrop and reward suit palette for the current ending-reward transition step.</summary>
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

    /// <summary>Writes a fixed-point component fade of the post-credits palette into a contiguous CGRAM range.</summary>
    /// <param name="start">First CGRAM color index to update.</param>
    /// <param name="count">Number of consecutive colors to write.</param>
    /// <param name="step">Multiplier applied to each RGB5 component before conversion back to five-bit channels.</param>
    private void ApplyPostCreditsPaletteRange(int start, int count, int step)
    {
        // E110 saves Intro4 as its target, clears colors $20-$2F, then E158 runs the
        // shared 8CB2 component accumulator 32 times. Each increment is component<<3
        // in 8.8 precision; composing takes only its high byte, preserving truncation.
        for (int index = start; index < start + count; index++)
        {
            ushort target = StaticPaletteColor(EndingPaletteId.PostCredits, index);
            int red = ((target & 31) << 3) * step;
            int green = ((target >> 5 & 31) << 3) * step;
            int blue = ((target >> 10 & 31) << 3) * step;
            cgram.SetColor(index, (ushort)((red >> 8) | ((green >> 8) << 5) | ((blue >> 8) << 10)));
        }
    }
}

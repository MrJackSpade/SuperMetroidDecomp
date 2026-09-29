using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    /// <summary>Runs $A9:BCFD at the body-frame cadence, then $BD1D's three literal copies.</summary>
    private void ApplyMotherBrainRainbowPalette(MotherBrainEnemyState state,
        MotherBrainRainbowBeamAttackStepResult step)
    {
        bool draining = step.PhaseBefore == MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidTransitionToGrey;
        bool reviving = step.PhaseBefore == MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfTransitionFromGrey;
        if ((draining || reviving) && step.PaletteRequested)
        {
            // The sequence increments before publishing this request. Keep the distinct
            // native copy lengths: revival intentionally preserves two brain/body colors.
            int index = state.RainbowBeamSequence!.GreyTransitionCounter - 1;
            var installed = MotherBrainRainbowColors ?? throw new InvalidDataException(
                "Mother Brain's grey transition requires installed palette artwork.");
            if (draining) installed.ApplyToGrey(_bus!, _cgram!, index);
            else installed.ApplyFromGrey(_bus!, _cgram!, index);
            return;
        }
        if (step.PhaseBefore == MotherBrainRainbowBeamAttackPhase.FinishFiring &&
            step.PhaseAfter == MotherBrainRainbowBeamAttackPhase.LetSamusFall)
        {
            var installed = MotherBrainRainbowColors ?? throw new InvalidDataException(
                "Mother Brain's normal palette requires installed artwork.");
            installed.ApplyNormal(_cgram!);
            return;
        }
        if (step.PhaseBefore == MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidFiringRainbowBeam &&
            step.PhaseAfter == MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidRainbowBeamRunOut)
        {
            var installed = MotherBrainRainbowColors ?? throw new InvalidDataException(
                "Mother Brain's drained palette requires installed artwork.");
            installed.ApplyRainbow(_cgram!, MotherBrainRainbowPaletteRomData.DrainedPointerOffset / sizeof(ushort));
            return;
        }
        bool rainbowPhase = step.PhaseBefore is
            MotherBrainRainbowBeamAttackPhase.MoveSamusTowardWall or
            MotherBrainRainbowBeamAttackPhase.OneFrameDelay or
            MotherBrainRainbowBeamAttackPhase.StartDrainingSamus or
            MotherBrainRainbowBeamAttackPhase.DrainingSamus or
            MotherBrainRainbowBeamAttackPhase.FinishFiring or
            MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidRegainBalance or
            MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidFiringRainbowBeam;
        if (!rainbowPhase || !step.PaletteRequested || (state.Body.FrameCounter & 2) == 0)
            return;

        var rainbow = MotherBrainRainbowColors ?? throw new InvalidDataException(
            "Mother Brain's rainbow phase requires installed palette artwork.");
        if ((state.RainbowPaletteCursor & 1) != 0 ||
            state.RainbowPaletteCursor > MotherBrainRainbowPaletteFormat.RainbowFrameCount * sizeof(ushort))
            throw new InvalidDataException($"Mother Brain rainbow cursor ${state.RainbowPaletteCursor:X4} is outside the compiled loop.");
        int frame = state.RainbowPaletteCursor / sizeof(ushort);
        if (frame == MotherBrainRainbowPaletteFormat.RainbowFrameCount)
        {
            state.RainbowPaletteCursor = 0;
            frame = 0;
        }
        state.RainbowPaletteCursor += sizeof(ushort);
        rainbow.ApplyRainbow(_cgram!, frame);
    }
}

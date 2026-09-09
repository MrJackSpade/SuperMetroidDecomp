namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    /// <summary>Runs $A9:BCFD at the body-frame cadence, then $BD1D's three literal copies.</summary>
    private void ApplyMotherBrainRainbowPalette(MotherBrainEnemyState state,
        MotherBrainRainbowBeamAttackStepResult step)
    {
        if (step.PhaseBefore == MotherBrainRainbowBeamAttackPhase.FinishFiring &&
            step.PhaseAfter == MotherBrainRainbowBeamAttackPhase.LetSamusFall)
        {
            WriteMotherBrainRainbowColors(MotherBrainRainbowPaletteRomData.NormalBrainSource,
                MotherBrainRainbowPaletteRomData.NormalSecondarySource);
            return;
        }
        if (step.PhaseBefore == MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidFiringRainbowBeam &&
            step.PhaseAfter == MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidRainbowBeamRunOut)
        {
            int drainedSource = MotherBrainRainbowPaletteRomData.SourceBank | ReadWord(_bus!,
                MotherBrainRainbowPaletteRomData.PointerTable + MotherBrainRainbowPaletteRomData.DrainedPointerOffset);
            WriteMotherBrainRainbowColors(drainedSource, drainedSource + MotherBrainRainbowPaletteRomData.ColorCount * 2);
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

        ushort pointer = ReadWord(_bus!, MotherBrainRainbowPaletteRomData.PointerTable + state.RainbowPaletteCursor);
        if (pointer == 0)
        {
            state.RainbowPaletteCursor = 0;
            pointer = ReadWord(_bus!, MotherBrainRainbowPaletteRomData.PointerTable);
        }
        if (pointer == 0)
            throw new InvalidDataException("Mother Brain rainbow palette list has no first entry.");
        state.RainbowPaletteCursor += 2;
        int source = MotherBrainRainbowPaletteRomData.SourceBank | pointer;
        WriteMotherBrainRainbowColors(source, source + MotherBrainRainbowPaletteRomData.ColorCount * 2);
    }

    private void WriteMotherBrainRainbowColors(int source, int secondarySource)
    {
        _cgram!.LoadFromBus(_bus!, source, MotherBrainRainbowPaletteRomData.ColorCount,
            MotherBrainRainbowPaletteRomData.BodyColor);
        _cgram.LoadFromBus(_bus!, source, MotherBrainRainbowPaletteRomData.ColorCount,
            MotherBrainRainbowPaletteRomData.BrainColor);
        _cgram.LoadFromBus(_bus!, secondarySource,
            MotherBrainRainbowPaletteRomData.ColorCount, MotherBrainRainbowPaletteRomData.SecondaryColor);
    }
}

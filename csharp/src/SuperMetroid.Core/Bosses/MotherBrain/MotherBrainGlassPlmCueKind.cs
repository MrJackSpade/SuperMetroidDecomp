namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What one of the glass programs' cues does (#1296).</summary>
internal enum MotherBrainGlassPlmCueKind
{
    /// <summary>Continues at <see cref="MotherBrainGlassPlmProgram.BossDead"/> once the area boss is dead.</summary>
    SkipIfBossDead,
    /// <summary>Continues at <see cref="MotherBrainGlassPlmProgram.NoGlass"/> once the glass-destroyed event is set.</summary>
    SkipIfGlassDestroyed,
    /// <summary>Installs the hit-counting pre-instruction.</summary>
    ArmHitWake,
    /// <summary>Sets the glass-destroyed event.</summary>
    SetGlassDestroyed,
}

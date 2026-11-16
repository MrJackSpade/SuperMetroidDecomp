namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The phases of Mother Brain finishing Samus off (#2208), until the Baby Metroid latches onto her final beam. Its values are arbitrary.</summary>
public enum MotherBrainFinishOffPhase
{
    /// <summary>Uses suit-adjusted health thresholds and sampled RNG to choose onion rings/bombs until Samus reaches the intended low-health floor.</summary>
    GetSamusToLowEnergy,

    /// <summary>Waits for standing, then immediately enters the 16-count admiration pause.</summary>
    StandUp,

    /// <summary>Expires the admiration pause, starts the stretching head animation, and starts a 256-count final-charge delay.</summary>
    Admire,

    /// <summary>When the timer runs out, installs the charging head animation and immediately begins readying the Baby Metroid's arrival.</summary>
    ChargeFinalBeam,

    /// <summary>Readies the Baby Metroid's arrival one update at a time; the last retracts the head, spawns the baby, and seeds a 256-count firing wait.</summary>
    ReadyBaby,

    /// <summary>Expires the final wait, starts the firing head animation, neck extension and its sound, then holds in the next phase.</summary>
    FireFinalBeam,

    /// <summary>Inert step that waits until the independently running baby latches on and replaces the body phase.</summary>
    FinalBeamHolding,
}

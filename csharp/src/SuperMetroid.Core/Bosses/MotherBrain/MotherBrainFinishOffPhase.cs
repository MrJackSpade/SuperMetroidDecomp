namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The phases of Mother Brain finishing Samus off (#2208), until the Baby Metroid latches onto her final beam. Its values are arbitrary.</summary>
public enum MotherBrainFinishOffPhase
{
    /// <summary>Uses suit-adjusted health thresholds and sampled RNG to choose onion rings/bombs until Samus reaches the intended low-health floor.</summary>
    GetSamusToLowEnergy,

    /// <summary>Waits for standing, then immediately enters the admiration pause.</summary>
    StandUp,

    /// <summary>Waits out the admiration pause, starts the stretching head animation, and starts the wait before the final charge.</summary>
    Admire,

    /// <summary>When the wait runs out, installs the charging head animation and starts the wait for the Baby Metroid's arrival, whose first update is this one.</summary>
    ChargeFinalBeam,

    /// <summary>Waits for the Baby Metroid's arrival; the wait's last update retracts the head, spawns the baby, and starts the wait before firing.</summary>
    BringInBaby,

    /// <summary>Waits out the wait before firing, starts the firing head animation, neck extension and its sound, then holds in the next phase.</summary>
    FireFinalBeam,

    /// <summary>Inert step that waits until the independently running baby latches on and replaces the body phase.</summary>
    FinalBeamHolding,
}

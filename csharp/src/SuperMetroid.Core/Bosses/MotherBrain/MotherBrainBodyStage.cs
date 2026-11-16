namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The stage Mother Brain's body is in, each running phases of its own type (#2208); setting a stage's phase on
/// <see cref="MotherBrainEnemyState"/> enters that stage. Its values are arbitrary.
/// </summary>
public enum MotherBrainBodyStage
{
    /// <summary>She fights inside her glass tank, her first phase; the stage runs no phase of its own.</summary>
    Glass,

    /// <summary>Her fake death's descent: the pauses, the lock, the flash, the grey fade and her tubes' collapse. Its phases are <see cref="MotherBrainDescentPhase"/>.</summary>
    FakeDeathDescent,

    /// <summary>Her fake death's ascent: the mural, her second form's setup, her rise and her colours' return. Its phases are <see cref="MotherBrainAscentPhase"/>.</summary>
    FakeDeathAscent,

    /// <summary>Her second phase's fight before her health first runs out: the stretch, her thinking and her attacks. Its phases are <see cref="MotherBrainPhase2FightPhase"/>.</summary>
    Phase2Fight,

    /// <summary>Her rainbow beam's attack, charged and fired once her second phase's health runs out. Its phases are <see cref="MotherBrainRainbowPhase"/>.</summary>
    RainbowBeam,

    /// <summary>Her finishing Samus off with onion rings and bombs until the final beam, which the Baby interrupts. Its phases are <see cref="MotherBrainFinishOffPhase"/>.</summary>
    FinishingSamusOff,

    /// <summary>The Baby Metroid draining her: taken aback, firing back, retreating and greying into a corpse. Its phases are <see cref="MotherBrainDrainedPhase"/>.</summary>
    DrainedByBaby,

    /// <summary>Her revival from the corpse and her attacks on the Baby Metroid. Its phases are <see cref="MotherBrainRevivalPhase"/>.</summary>
    Reviving,

    /// <summary>Her third phase's fight against the Hyper Beam. Its phases are <see cref="MotherBrainPhase3FightPhase"/>.</summary>
    Phase3Fight,

    /// <summary>Her final death and the escape it starts. Its phases are <see cref="MotherBrainDeathPhase"/>.</summary>
    Dying,
}

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The stage Mother Brain's body is in before her late fight, each running phases of its own type (#2208); setting a
/// stage's phase on <see cref="MotherBrainEnemyState"/> enters that stage. Her late fight runs the stages from her rainbow
/// beam on (<see cref="MotherBrainLateFightStage"/>). Its values are arbitrary.
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

}

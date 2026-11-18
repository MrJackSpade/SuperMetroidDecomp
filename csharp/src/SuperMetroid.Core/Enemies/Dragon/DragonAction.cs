namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// The behaviour a Dragon body's main AI runs each frame; its cosmetic wing has none.
/// Its values are arbitrary.
/// </summary>
public enum DragonAction
{
    /// <summary>Waits below the lava, then selects facing and begins the rise (<see cref="DragonTimingDefinitions.RiseOrSinkDuration"/>).</summary>
    WaitToRise,

    /// <summary>Moves the body and cosmetic wing upward one pixel per update.</summary>
    Rising,

    /// <summary>Plays its attack animation, starting it unless it already plays, until the animation's finishing cue.</summary>
    Attacking,

    /// <summary>
    /// The attack animation has finished, as its last cue says, and rests on its last pose: the next update spits a
    /// fireball, then starts over or, after the third, waits to sink.
    /// </summary>
    AttackFinished,

    /// <summary>
    /// Has spat a fireball with shots left: the next update starts the attack animation over, even though it still plays
    /// it, which spaces the three fireballs by a whole attack animation (#2145).
    /// </summary>
    RestartingAttack,

    /// <summary>Holds the exposed idle pose (<see cref="DragonTimingDefinitions.WaitBeforeSink"/>) before sinking.</summary>
    WaitToSink,

    /// <summary>Moves the body and wing downward before restoring the hidden wait.</summary>
    Sinking,
}

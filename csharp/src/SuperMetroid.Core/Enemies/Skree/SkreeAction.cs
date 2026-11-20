namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>
/// The behaviour a Skree's main AI runs.
/// Its values are arbitrary.
/// </summary>
public enum SkreeAction
{
    /// <summary>Waits until Samus is within its wind-up distance across (<see cref="SkreeMotionDefinitions.WindUpDistance"/>).</summary>
    Idling,

    /// <summary>Winds up to dive while its preparation animation plays.</summary>
    PreparingAttack,

    /// <summary>
    /// Ready to dive, as the wind-up's cue says (#2450): the next update starts the dive animation and plays the dive
    /// screech.
    /// </summary>
    AttackReady,

    /// <summary>Descends by <see cref="SkreeMotionDefinitions.DiveStep"/> and steers toward Samus by <see cref="SkreeMotionDefinitions.DiveDrift"/> until floor collision.</summary>
    Diving,

    /// <summary>Sinks for its burrow (<see cref="SkreeMotionDefinitions.BurrowDuration"/>), emits debris, then deletes the enemy.</summary>
    Burrowing,
}

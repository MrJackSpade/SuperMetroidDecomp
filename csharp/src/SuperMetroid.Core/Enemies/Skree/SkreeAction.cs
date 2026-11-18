namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>
/// The behaviour a Skree's main AI runs.
/// Its values are arbitrary.
/// </summary>
public enum SkreeAction
{
    /// <summary>Waits until Samus is within 48 horizontal pixels.</summary>
    Idling,

    /// <summary>Winds up to dive while its preparation animation plays.</summary>
    PreparingAttack,

    /// <summary>
    /// Ready to dive, as the wind-up's cue says (#2450): the next update starts the dive animation and plays the dive
    /// screech.
    /// </summary>
    AttackReady,

    /// <summary>Descends six pixels and steers one pixel toward Samus until floor collision.</summary>
    Diving,

    /// <summary>Sinks for the retained 21-update lifetime, emits debris, then deletes the enemy.</summary>
    Burrowing,
}

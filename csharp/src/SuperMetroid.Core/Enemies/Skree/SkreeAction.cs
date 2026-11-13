namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>
/// The behaviour a Skree's main AI runs.
/// Its values are arbitrary.
/// </summary>
public enum SkreeAction
{
    /// <summary>Waits until Samus is within 48 horizontal pixels.</summary>
    Idling = 1,

    /// <summary>Waits for the wind-up cue to authorize the dive.</summary>
    PreparingAttack,

    /// <summary>Descends six pixels and steers one pixel toward Samus until floor collision.</summary>
    Diving,

    /// <summary>Sinks for the retained 21-update lifetime, emits debris, then deletes the enemy.</summary>
    Burrowing,
}

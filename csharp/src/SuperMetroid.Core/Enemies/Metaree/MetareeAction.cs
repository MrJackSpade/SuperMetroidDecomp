namespace SuperMetroid.Core.Enemies.Metaree;

/// <summary>
/// The behaviour a Metaree's main AI runs.
/// Its values are arbitrary.
/// </summary>
/// <remarks>
/// AttackReady is a state of its course, not a request another owner stores for it (#2562): a cue, a touch or a shot ends
/// the state before, and the game acts on it one update later, the state naming where it stands until then; the update
/// that acts on it moves it on, as every other state's does. The update between is the game's.
/// </remarks>
public enum MetareeAction
{
    /// <summary>Dives once Samus is within 72 pixels either side, whether or not she is below, at a downward speed set as it starts.</summary>
    Idling,
    /// <summary>Crouches to launch while its preparation animation plays.</summary>
    PreparingAttack,
    /// <summary>
    /// Ready to attack, as the preparation animation's cue says (#2439): the next update starts the dive animation and plays
    /// the dive sound.
    /// </summary>
    AttackReady,
    /// <summary>Dives at its whole-pixel speed, steering two pixels an update toward Samus, until the block below stops it and it starts burrowing for 21 updates.</summary>
    LaunchedAttack,
    /// <summary>Decrements the timer, emits four metal particles when it reaches eight, sinks one pixel per surviving update, and deletes the actor at zero.</summary>
    Burrowing,
}

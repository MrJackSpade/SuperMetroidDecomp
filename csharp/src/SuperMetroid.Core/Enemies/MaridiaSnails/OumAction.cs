namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>
/// The behaviour Oum's main AI runs.
/// Its values are arbitrary.
/// </summary>
/// <remarks>
/// AttackFinished is a state of its course, not a request another owner stores for it (#2562): a cue, a touch or a shot ends
/// the state before, and the game acts on it one update later, the state naming where it stands until then; the update
/// that acts on it moves it on, as every other state's does. The update between is the game's.
/// </remarks>
public enum OumAction
{
    /// <summary>Completes settling bounces, then selects an idle facing and enters rolling when Samus is strictly within its pushing reach across (<see cref="OumMotionDefinitions.PushingReachX"/>).</summary>
    Idle,
    /// <summary>Bounces, rolls and meets walls; a stop the previous update asked for also holds the attack cooldown, which lasts its whole duration and then stays run out until Oum attacks.</summary>
    Rolling,
    /// <summary>Waits, without moving, for the attack animation to finish.</summary>
    Attacking,
    /// <summary>
    /// Finished attacking, as the attack animation's cue says (#2439): the next update starts the rolling animation for
    /// the way Oum faces and rolls on.
    /// </summary>
    AttackFinished,
}

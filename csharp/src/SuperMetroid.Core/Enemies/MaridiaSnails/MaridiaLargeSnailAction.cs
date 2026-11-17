namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>
/// The behaviour Oum's main AI runs.
/// Its values are arbitrary.
/// </summary>
public enum MaridiaLargeSnailAction
{
    /// <summary>Completes settling bounces, then selects an idle facing and enters rolling when Samus is strictly within 24 horizontal pixels.</summary>
    Idle,
    /// <summary>Bounces, rolls and meets walls; a stop the previous update asked for also holds the attack cooldown, which lasts its whole duration and then stays run out until Oum attacks.</summary>
    Rolling,
    /// <summary>Waits for the attack animation's finished flag, consumes it, and installs the corresponding rolling animation without moving during the wait.</summary>
    Attacking,
}

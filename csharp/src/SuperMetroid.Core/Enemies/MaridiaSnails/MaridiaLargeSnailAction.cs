namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>
/// The behaviour Oum's main AI runs.
/// Its values are arbitrary.
/// </summary>
public enum MaridiaLargeSnailAction
{
    /// <summary>Completes settling bounces, then selects an idle facing and enters rolling when Samus is strictly within 24 horizontal pixels.</summary>
    Idle = 1,
    /// <summary>Applies bounce/floor and horizontal collision movement; the preceding update's stop flag also suppresses the signed-underflow attack countdown.</summary>
    Rolling,
    /// <summary>Waits for the attack animation's finished flag, consumes it, and installs the corresponding rolling animation without moving during the wait.</summary>
    Attacking,
}

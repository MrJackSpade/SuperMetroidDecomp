namespace SuperMetroid.Core.Enemies.Skultera;

/// <summary>
/// The behaviour a Skultera's main AI runs.
/// Its values are arbitrary.
/// </summary>
public enum SkulteraEnemyFunction
{
    /// <summary>Applies signed 16.16 leftward displacement and whole-pixel sine-offset changes; either axis collision starts a right turn.</summary>
    SwimmingLeft = 1,
    /// <summary>Mirrors leftward swimming with the rightward speed; unobstructed movement advances the angle, and either axis collision starts a left turn.</summary>
    SwimmingRight,
    /// <summary>Waits without swimming until animation sets the turn-finished flag, then clears it, negates the angle delta, and installs rightward swimming.</summary>
    TurningRight,
    /// <summary>Waits for the animation's turn-finished flag, then clears it, negates the angle delta, and installs leftward swimming.</summary>
    TurningLeft,
}

namespace SuperMetroid.Core.Game;

/// <summary>Collision and lifetime result from one `$90:DF38` frame.</summary>
/// <param name="Landed">Whether downward collision produced the landing seam consumed by presentation and transition owners.</param>
public readonly record struct KnockbackMovementResult(
    bool Landed = false);

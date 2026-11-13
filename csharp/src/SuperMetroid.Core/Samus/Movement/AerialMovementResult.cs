namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Both collision scans and collision state produced by one aerial frame.</summary>
/// <param name="Landed">Whether the downward pass landed Samus on a surface.</param>
/// <param name="HitCeiling">Whether the upward pass stopped Samus at a ceiling.</param>
/// <param name="WallJumpTriggered">Whether the wall probe accepted a wall-jump launch this frame.</param>
[IndependentFlag("independent outcomes of one aerial step")]
public readonly record struct AerialMovementResult(
    bool Landed,
    bool HitCeiling,
    bool WallJumpTriggered = false);

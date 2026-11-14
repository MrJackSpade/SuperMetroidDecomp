using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>
/// What a Polyp's rock keeps: whether it is still rising or has passed the top of its arc, and where on the shared
/// quadratic speed curve its vertical speed stands.
/// </summary>
/// <param name="speedCurvePlace">The rock's starting vertical speed, as a place on the quadratic speed curve.</param>
internal sealed class PolypRockState(QuadraticSpeedCurvePlace speedCurvePlace) : EnemyProjectileState
{
    /// <summary>The part of its arc the rock is in; it starts rising.</summary>
    internal PolypRockMovement Phase { get; set; } = PolypRockMovement.Rising;

    /// <summary>
    /// The rock's vertical speed as a whole-mark place on the quadratic speed curve: it moves back two marks each update
    /// while the rock rises and on two marks, up to a cap, while it falls.
    /// </summary>
    internal QuadraticSpeedCurvePlace SpeedCurvePlace { get; set; } = speedCurvePlace;
}

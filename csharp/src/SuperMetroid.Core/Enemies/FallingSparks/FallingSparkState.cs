using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.FallingSparks;

/// <summary>What a falling spark keeps: the sideways drift and the vertical speed it adds to its position every update.</summary>
/// <param name="horizontalStep">The drift chosen at launch.</param>
internal sealed class FallingSparkState(SignedSixteenSixteen horizontalStep) : EnemyProjectileState
{
    /// <summary>The sideways distance the spark moves each update; it quadruples when the spark hits the floor.</summary>
    internal SignedSixteenSixteen HorizontalStep { get; set; } = horizontalStep;

    /// <summary>
    /// The vertical distance the spark moves each update: it starts still, gains a quarter pixel each falling update
    /// until its whole part would reach four, and is half a pixel upward once the spark has hit the floor.
    /// </summary>
    internal SignedSixteenSixteen VerticalSpeed { get; set; }
}

using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.FallingSparks;

/// <summary>What a falling spark keeps: the sideways drift it adds to its position every update.</summary>
/// <param name="horizontalStep">The drift chosen at launch.</param>
internal sealed class FallingSparkState(SignedSixteenSixteen horizontalStep) : EnemyProjectileState
{
    /// <summary>The sideways distance the spark moves each update; it quadruples when the spark hits the floor.</summary>
    internal SignedSixteenSixteen HorizontalStep { get; set; } = horizontalStep;
}

using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.FallingSparks;

/// <summary>What a falling spark keeps: the sideways drift and the vertical speed it adds to its position every update.</summary>
/// <param name="horizontalStep">The drift chosen at launch.</param>
internal sealed partial class FallingSparkState(SignedSixteenSixteen horizontalStep) : EnemyProjectileState
{
    /// <summary>The sideways distance the spark moves each update; it grows by <see cref="FallingSparkMotionDefinitions.ImpactDriftScale"/> when the spark hits the floor.</summary>
    internal SignedSixteenSixteen HorizontalStep { get; set; } = horizontalStep;

    /// <summary>
    /// The vertical distance the spark moves each update: it starts still, gains <see cref="FallingSparkMotionDefinitions.Gravity"/> each falling update
    /// until its whole part would reach <see cref="FallingSparkMotionDefinitions.TerminalWholeSpeed"/>, and is <see cref="FallingSparkMotionDefinitions.ImpactBounce"/> once the spark has hit the floor.
    /// </summary>
    internal SignedSixteenSixteen VerticalSpeed { get; set; }
}

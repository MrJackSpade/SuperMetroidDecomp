namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>
/// Oum's bounce behaviour. The enemy uses the shared quadratic speed curve for a diminishing
/// settling animation.
/// Its values are arbitrary.
/// </summary>
public enum OumBouncePhase
{
    /// <summary>Moves on along the speed curve by <see cref="OumMotionDefinitions.BounceAcceleration"/> each update up to <see cref="OumMotionDefinitions.FastestFallPlace"/>; a floor impact consumes a bounce, moves back by <see cref="OumMotionDefinitions.ImpactCurveLoss"/>, and selects rising.</summary>
    Falling,
    /// <summary>Moves back along the speed curve by <see cref="OumMotionDefinitions.BounceAcceleration"/> each update and rises by the curve's speed; a place stepped back past the start resets to the start and selects falling without moving that update.</summary>
    Rising,
}

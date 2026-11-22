using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>One of a standing Torizo's arms as it throws or swipes (#2411): its poses from lowered to overhead.</summary>
/// <param name="Down">The arm lowered.</param>
/// <param name="Lifting">The arm lifting.</param>
/// <param name="Level">The arm held level.</param>
/// <param name="Raised">The arm raised.</param>
/// <param name="Overhead">The arm overhead.</param>
internal sealed partial record TorizoArmPoses(EnemyFrameId Down, EnemyFrameId Lifting, EnemyFrameId Level, EnemyFrameId Raised, EnemyFrameId Overhead);

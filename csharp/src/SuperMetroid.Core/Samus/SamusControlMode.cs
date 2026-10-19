namespace SuperMetroid.Core.Samus;

/// <summary>The frame handler's installed beta handler, derived at the frame's two decision points.</summary>
/// <param name="Beta">The beta handler, derived before the pose lookup and again after EnemyMain.</param>
internal readonly record struct SamusControlMode(SamusBetaHandler Beta);

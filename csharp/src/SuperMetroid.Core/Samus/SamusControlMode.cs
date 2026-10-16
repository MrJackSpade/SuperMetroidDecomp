namespace SuperMetroid.Core.Samus;

/// <summary>The frame handler's installed alpha and beta handlers, derived at the frame's two decision points.</summary>
/// <param name="Alpha">The alpha handler, derived before the pose lookup.</param>
/// <param name="Beta">The beta handler, derived before the pose lookup and again after EnemyMain.</param>
internal readonly record struct SamusControlMode(SamusAlphaHandler Alpha, SamusBetaHandler Beta);

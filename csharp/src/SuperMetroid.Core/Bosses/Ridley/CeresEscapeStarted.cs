using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ceres Ridley moved Ceres from the fight to the escape (#2562): the escape's countdown starts on the same enemy pass.
/// </summary>
public sealed partial record CeresEscapeStarted : EnemyPassEvent;

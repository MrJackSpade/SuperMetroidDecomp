namespace SuperMetroid.Core.Enemies;

/// <summary>A special bomb reaction and the enemies it applies to.</summary>
/// <param name="Matches">The matches.</param>
/// <param name="Handler">The handler.</param>
internal readonly record struct EnemyBombSpecial(Func<EnemyBombContext, bool> Matches, Action<EnemyBombContext> Handler);

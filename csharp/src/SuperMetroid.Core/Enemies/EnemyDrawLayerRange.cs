namespace SuperMetroid.Core.Enemies;

/// <summary>The draw layers prepared so far, as a closed range.</summary>
/// <param name="First">The first prepared layer.</param>
/// <param name="Last">The last prepared layer.</param>
internal readonly record struct EnemyDrawLayerRange(int First, int Last);

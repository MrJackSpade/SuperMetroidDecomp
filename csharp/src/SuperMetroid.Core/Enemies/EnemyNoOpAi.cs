namespace SuperMetroid.Core.Enemies;

/// <summary>The AI handlers of a routine that is a literal RTL: running it changes nothing.</summary>
internal static class EnemyNoOpAi
{
    /// <summary>An initialization AI that does nothing.</summary>
    internal static EnemyInitializationAiHandler Initialization { get; } = static _ => { };

    /// <summary>A main AI that does nothing.</summary>
    internal static EnemyMainAiHandler Main { get; } = static (_, _) => { };
}

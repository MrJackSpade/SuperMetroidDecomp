namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The sounds the Tourian Shitroid's animations queue.</summary>
internal static class ShitroidSoundDefinitions
{
    /// <summary>The remorse cry before the Shitroid leaves.</summary>
    internal static SoundEffectId RemorseCry { get; } = EnemySound.BabyMetroidRemorse;

    /// <summary>The cry as the Shitroid flies away from Samus.</summary>
    internal static SoundEffectId FlyAwayCry { get; } = EnemySound.BabyMetroidGuilt;

    /// <summary>The cry while draining at the slow palette pace.</summary>
    internal static SoundEffectId DrainCry { get; } = EnemySound.BabyMetroidCry;

    /// <summary>The cry while draining at the quick palette pace.</summary>
    internal static SoundEffectId QuickDrainCry { get; } = EnemySound.BabyMetroidDrain;
}

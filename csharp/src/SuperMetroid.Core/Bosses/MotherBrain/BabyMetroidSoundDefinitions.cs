namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The sounds the Tourian Baby Metroid's animations queue.</summary>
internal static partial class BabyMetroidSoundDefinitions
{
    /// <summary>The remorse cry before the Baby Metroid leaves.</summary>
    internal static SoundEffectId RemorseCry { get; } = EnemySound.BabyMetroidRemorse;

    /// <summary>The cry as the Baby Metroid flies away from Samus.</summary>
    internal static SoundEffectId FlyAwayCry { get; } = EnemySound.BabyMetroidGuilt;

    /// <summary>The cry while draining at the slow colour pace.</summary>
    internal static SoundEffectId DrainCry { get; } = EnemySound.BabyMetroidCry;

    /// <summary>The cry while draining at the quick colour pace.</summary>
    internal static SoundEffectId QuickDrainCry { get; } = EnemySound.BabyMetroidDrain;
}

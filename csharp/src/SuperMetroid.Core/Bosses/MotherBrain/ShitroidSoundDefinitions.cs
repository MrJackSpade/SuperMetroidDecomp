namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The sounds the Tourian Shitroid's programs queue.</summary>
internal static class ShitroidSoundDefinitions
{
    /// <summary>The remorse cry before the Shitroid leaves.</summary>
    internal static SoundEffectId RemorseCry { get; } = Library2Sound.BabyMetroidRemorse;

    /// <summary>The cry as the Shitroid flies away from Samus.</summary>
    internal static SoundEffectId FlyAwayCry { get; } = Library2Sound.BabyMetroidGuilt;

    /// <summary>The cry while draining at the slow palette pace.</summary>
    internal static SoundEffectId DrainCry { get; } = Library2Sound.BabyMetroidCry;

    /// <summary>The cry while draining at the quick palette pace.</summary>
    internal static SoundEffectId QuickDrainCry { get; } = Library2Sound.BabyMetroidDrain;
}

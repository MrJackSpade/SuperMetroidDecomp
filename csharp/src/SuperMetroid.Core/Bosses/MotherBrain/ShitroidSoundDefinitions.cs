namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The sounds the Tourian Shitroid's programs queue.</summary>
internal static class ShitroidSoundDefinitions
{
    /// <summary>Library-2 sound $52: the remorse cry before the Shitroid leaves.</summary>
    internal static SoundEffectId RemorseCry { get; } = Library2Sound.BabyMetroidRemorse;

    /// <summary>Library-2 sound $7D: the cry as the Shitroid flies away from Samus.</summary>
    internal static SoundEffectId FlyAwayCry { get; } = Library2Sound.BabyMetroidGuilt;

    /// <summary>Library-2 sound $72: the cry while draining at the slow palette pace.</summary>
    internal static SoundEffectId DrainCry { get; } = Library2Sound.BabyMetroidCry;

    /// <summary>Library-2 sound $78: the cry while draining at the quick palette pace.</summary>
    internal static SoundEffectId QuickDrainCry { get; } = Library2Sound.BabyMetroidDrain;
}

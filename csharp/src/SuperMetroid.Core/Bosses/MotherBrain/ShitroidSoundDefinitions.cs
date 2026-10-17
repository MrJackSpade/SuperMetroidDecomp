namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The sounds the Tourian Shitroid's programs queue.</summary>
internal static class ShitroidSoundDefinitions
{
    /// <summary>Library-2 sound $52: the remorse cry before the Shitroid leaves.</summary>
    internal static SoundEffectId RemorseCry { get; } = SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x0052);

    /// <summary>Library-2 sound $7D: the cry as the Shitroid flies away from Samus.</summary>
    internal static SoundEffectId FlyAwayCry { get; } = SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x007d);

    /// <summary>Library-2 sound $72: the cry while draining at the slow palette pace.</summary>
    internal static SoundEffectId DrainCry { get; } = SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x0072);

    /// <summary>Library-2 sound $78: the cry while draining at the quick palette pace.</summary>
    internal static SoundEffectId QuickDrainCry { get; } = SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x0078);
}

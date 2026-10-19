namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's sound effects, queued by its programs with a six-entry admission limit.</summary>
internal static class DraygonSoundDefinitions
{
    /// <summary>Library-2 sound $25: the dying explosions and each tail-whip impact.</summary>
    internal static SoundEffectId Explosion { get; } = SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x0025);

    /// <summary>Library-2 sound $4C, as the goop leaves the mouth.</summary>
    internal static SoundEffectId GoopSpit { get; } = SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x004c);

    /// <summary>Library-2 sound $73, the roar.</summary>
    internal static SoundEffectId Roar { get; } = SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x0073);

    /// <summary>Library-3 sound $1B, the cry that starts the death sequence.</summary>
    internal static SoundEffectId DyingCry { get; } = SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 0x001b);
}

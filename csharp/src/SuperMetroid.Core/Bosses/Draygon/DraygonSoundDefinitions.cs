namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's sound effects, queued by its programs with a six-entry admission limit.</summary>
internal static class DraygonSoundDefinitions
{
    /// <summary>Library-2 sound $25: the dying explosions and each tail-whip impact.</summary>
    internal static SoundEffectId Explosion { get; } = Library2Sound.BigExplosion;

    /// <summary>Library-2 sound $4C, as the goop leaves the mouth.</summary>
    internal static SoundEffectId GoopSpit { get; } = Library2Sound.AcidSpit;

    /// <summary>Library-2 sound $73, the roar.</summary>
    internal static SoundEffectId Roar { get; } = Library2Sound.PhantoonCry;

    /// <summary>Library-3 sound $1B, the cry that starts the death sequence.</summary>
    internal static SoundEffectId DyingCry { get; } = Library3Sound.DraygonDyingCry;
}

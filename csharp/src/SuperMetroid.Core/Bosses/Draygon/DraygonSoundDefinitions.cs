namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's sound effects, queued by its animations with a six-entry admission limit.</summary>
internal static class DraygonSoundDefinitions
{
    /// <summary>The big explosion: the dying explosions and each tail-whip impact.</summary>
    internal static SoundEffectId Explosion { get; } = EnemySound.BigExplosion;

    /// <summary>The acid spit, as the goop leaves the mouth.</summary>
    internal static SoundEffectId GoopSpit { get; } = EnemySound.AcidSpit;

    /// <summary>The roar.</summary>
    internal static SoundEffectId Roar { get; } = EnemySound.PhantoonCry;

    /// <summary>The cry that starts the death sequence.</summary>
    internal static SoundEffectId DyingCry { get; } = RoomSound.DraygonDyingCry;
}

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's and Mini-Kraid's sound effects.</summary>
internal static class KraidSoundDefinitions
{
    /// <summary>Mini-Kraid's cry before it spits, requested only while its origin is on screen.</summary>
    internal static SoundEffectId MiniKraidCry { get; } = EnemySound.FakeKraidCry;

    /// <summary>Mini-Kraid launches a body spike, requested only while its origin is on screen.</summary>
    internal static SoundEffectId MiniKraidSpikeLaunch { get; } = EnemySound.AlcoonSpit;

    /// <summary>A spat rock leaves the mouth, or a rock rises from the floor.</summary>
    internal static SoundEffectId RockSpit { get; } = RoomSound.KraidEarthquake;

    /// <summary>A belly lint fires.</summary>
    internal static SoundEffectId LintFire { get; } = RoomSound.KraidLint;

    /// <summary>Kraid's roar, cried by the attack and dying roars of the head animation.</summary>
    internal static SoundEffectId HeadRoar { get; } = EnemySound.KraidRoar;

    /// <summary>Kraid's death cry, cried by the death head animation.</summary>
    internal static SoundEffectId HeadDyingCry { get; } = EnemySound.KraidDyingCry;

    /// <summary>A heavy footstep.</summary>
    internal static SoundEffectId Footstep { get; } = EnemySound.Quake;
}

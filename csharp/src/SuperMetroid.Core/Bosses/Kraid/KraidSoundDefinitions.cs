namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's and Fake Kraid's sound effects.</summary>
internal static class KraidSoundDefinitions
{
    /// <summary>Fake Kraid's cry before it spits, requested only while its origin is on screen.</summary>
    internal static SoundEffectId FakeKraidCry { get; } = EnemySound.FakeKraidCry;

    /// <summary>Fake Kraid launches a body spike, requested only while its origin is on screen.</summary>
    internal static SoundEffectId FakeKraidSpikeLaunch { get; } = EnemySound.Whoosh;

    /// <summary>A spat rock leaves the mouth, or a rock rises from the floor.</summary>
    internal static SoundEffectId RockSpit { get; } = RoomSound.KraidEarthquake;

    /// <summary>A belly lint fires.</summary>
    internal static SoundEffectId LintFire { get; } = RoomSound.KraidLint;

    /// <summary>Kraid's roar, cried by the attack and dying roars of the head animation.</summary>
    internal static SoundEffectId HeadRoar { get; } = EnemySound.Roar;

    /// <summary>Kraid's death cry, cried by the death head animation.</summary>
    internal static SoundEffectId HeadDyingCry { get; } = EnemySound.KraidDyingCry;

    /// <summary>A heavy footstep.</summary>
    internal static SoundEffectId Footstep { get; } = EnemySound.Quake;
}

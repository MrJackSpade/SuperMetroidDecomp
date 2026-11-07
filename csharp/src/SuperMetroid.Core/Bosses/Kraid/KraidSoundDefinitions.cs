namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's and Mini-Kraid's sound effects.</summary>
internal static class KraidSoundDefinitions
{
    /// <summary>Mini-Kraid's cry before it spits, requested only while its origin is on screen.</summary>
    internal static SoundEffectId MiniKraidCry { get; } = Library2Sound.FakeKraidCry;

    /// <summary>Mini-Kraid launches a body spike, requested only while its origin is on screen.</summary>
    internal static SoundEffectId MiniKraidSpikeLaunch { get; } = Library2Sound.AlcoonSpit;

    /// <summary>A spat rock leaves the mouth, or a rock rises from the floor.</summary>
    internal static SoundEffectId RockSpit { get; } = Library3Sound.KraidEarthquake;

    /// <summary>A belly lint fires.</summary>
    internal static SoundEffectId LintFire { get; } = Library3Sound.KraidLint;

    /// <summary>Kraid's roar, cried by the attack and dying roars of the head program.</summary>
    internal static SoundEffectId HeadRoar { get; } = Library2Sound.KraidRoar;

    /// <summary>Kraid's death cry, cried by the death head program.</summary>
    internal static SoundEffectId HeadDyingCry { get; } = Library2Sound.KraidDyingCry;

    /// <summary>A heavy footstep.</summary>
    internal static SoundEffectId Footstep { get; } = Library2Sound.Quake;
}

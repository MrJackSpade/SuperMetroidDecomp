namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's and Mini-Kraid's sound effects.</summary>
internal static class KraidSoundDefinitions
{
    /// <summary>Library-2 sound $16: Mini-Kraid's cry before it spits, requested only while its origin is on screen.</summary>
    internal static SoundEffectId MiniKraidCry { get; } = Library2Sound.FakeKraidCry;

    /// <summary>Library-2 sound $3F: Mini-Kraid launches a body spike, requested only while its origin is on screen.</summary>
    internal static SoundEffectId MiniKraidSpikeLaunch { get; } = Library2Sound.AlcoonSpit;

    /// <summary>Library-3 sound $1E: a spat rock leaves the mouth, or a rock rises from the floor.</summary>
    internal static SoundEffectId RockSpit { get; } = Library3Sound.KraidEarthquake;

    /// <summary>Library-3 sound $1F: a belly lint fires.</summary>
    internal static SoundEffectId LintFire { get; } = Library3Sound.KraidLint;

    /// <summary>Library-2 sound $2D: Kraid's roar, cried by the attack and dying roars of the head program.</summary>
    internal static SoundEffectId HeadRoar { get; } = Library2Sound.KraidRoar;

    /// <summary>Library-2 sound $2E: Kraid's death cry, cried by the death head program.</summary>
    internal static SoundEffectId HeadDyingCry { get; } = Library2Sound.KraidDyingCry;

    /// <summary>Library-2 sound $76: a heavy footstep.</summary>
    internal static SoundEffectId Footstep { get; } = Library2Sound.Quake;
}

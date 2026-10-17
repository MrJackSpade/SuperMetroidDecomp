namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's and Mini-Kraid's sound effects outside Kraid's head stream.</summary>
internal static class KraidSoundDefinitions
{
    /// <summary>Library-2 sound $16: Mini-Kraid's cry before it spits, requested only while its origin is on screen.</summary>
    internal static SoundEffectId MiniKraidCry { get; } = SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x0016);

    /// <summary>Library-2 sound $3F: Mini-Kraid launches a body spike, requested only while its origin is on screen.</summary>
    internal static SoundEffectId MiniKraidSpikeLaunch { get; } = SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x003f);

    /// <summary>Library-3 sound $1E: a spat rock leaves the mouth, or a rock rises from the floor.</summary>
    internal static SoundEffectId RockSpit { get; } = SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 0x001e);

    /// <summary>Library-3 sound $1F: a belly lint fires.</summary>
    internal static SoundEffectId LintFire { get; } = SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 0x001f);

    /// <summary>Library-2 sound $76: a heavy footstep.</summary>
    internal static SoundEffectId Footstep { get; } = SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x0076);
}

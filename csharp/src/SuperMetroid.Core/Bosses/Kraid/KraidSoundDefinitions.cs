namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's sound effects outside its head stream.</summary>
internal static class KraidSoundDefinitions
{
    /// <summary>Library-3 sound $1E: a spat rock leaves the mouth, or a rock rises from the floor.</summary>
    internal static SoundEffectId RockSpit { get; } = SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 0x001e);

    /// <summary>Library-3 sound $1F: a belly lint fires.</summary>
    internal static SoundEffectId LintFire { get; } = SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 0x001f);

    /// <summary>Library-2 sound $76: a heavy footstep.</summary>
    internal static SoundEffectId Footstep { get; } = SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x0076);
}

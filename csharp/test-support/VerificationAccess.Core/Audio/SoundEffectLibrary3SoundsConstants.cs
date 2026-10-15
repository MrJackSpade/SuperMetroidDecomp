namespace SuperMetroid.Core.Audio;

/// <summary>Cartridge values of <see cref="SoundEffectLibrary3Sounds"/> that only verification reads.</summary>
internal static class SoundEffectLibrary3SoundsConstants
{
    /// <summary>The low-health beep: it loops, enters low-health mode and keeps its priority (#1305).</summary>
    public static readonly SoundEffectId LowHealthBeep = new(SoundEffectLibrary.Library3, 0x02);

    /// <summary>Provides the library 3 low-health beep identifier for clip verification.</summary>
    extension(SoundEffectLibrary3Sounds)
    {
        /// <inheritdoc cref="SoundEffectLibrary3SoundsConstants.LowHealthBeep"/>
        internal static SoundEffectId LowHealthBeep => SoundEffectLibrary3SoundsConstants.LowHealthBeep;
    }
}

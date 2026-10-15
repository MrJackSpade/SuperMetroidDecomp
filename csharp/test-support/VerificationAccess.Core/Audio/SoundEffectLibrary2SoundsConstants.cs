namespace SuperMetroid.Core.Audio;

/// <summary>Cartridge values of <see cref="SoundEffectLibrary2Sounds"/> that only verification reads.</summary>
internal static class SoundEffectLibrary2SoundsConstants
{
    /// <summary>Etecoon cry (#1305's clip-fidelity fixture); its sample is absent from the EmptyCrateria upload.</summary>
    public static readonly SoundEffectId Etecoon = new(SoundEffectLibrary.Library2, 0x35);

    /// <summary>Provides the library 2 Etecoon identifier for clip verification.</summary>
    extension(SoundEffectLibrary2Sounds)
    {
        /// <inheritdoc cref="SoundEffectLibrary2SoundsConstants.Etecoon"/>
        internal static SoundEffectId Etecoon => SoundEffectLibrary2SoundsConstants.Etecoon;
    }
}

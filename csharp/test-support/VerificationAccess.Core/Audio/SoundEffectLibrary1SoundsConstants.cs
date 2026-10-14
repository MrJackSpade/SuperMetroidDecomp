using SuperMetroid.Core.Audio;

/// <summary>Cartridge values of <see cref="SoundEffectLibrary1Sounds"/> that only verification reads.</summary>
internal static class SoundEffectLibrary1SoundsConstants
{
    /// <summary>Uncharged Power Beam projectile launch.</summary>
    public static readonly SoundEffectId PowerBeam = new(SoundEffectLibrary.Library1, 0x0b);

    /// <summary>Provides the library 1 Power Beam sound identifier for audio event verification.</summary>
    extension(SoundEffectLibrary1Sounds)
    {
        /// <inheritdoc cref="SoundEffectLibrary1SoundsConstants.PowerBeam"/>
        internal static SoundEffectId PowerBeam => SoundEffectLibrary1SoundsConstants.PowerBeam;
    }
}

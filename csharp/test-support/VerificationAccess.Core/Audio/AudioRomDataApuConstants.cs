using SuperMetroid.Core.Audio;

/// <summary>Cartridge values of <see cref="AudioRomData.Apu"/> that only verification reads.</summary>
internal static class AudioRomDataApuConstants
{
    /// <summary>SPC I/O port used when the third uploaded audio library signals completion.</summary>
    public const byte LibraryThreePort = 3;

    /// <summary>Provides the APU library port address used when checking imported audio data against its native source.</summary>
    extension(AudioRomData.Apu)
    {
        /// <inheritdoc cref="AudioRomDataApuConstants.LibraryThreePort"/>
        internal static byte LibraryThreePort => AudioRomDataApuConstants.LibraryThreePort;
    }
}

namespace SuperMetroid.Core.Audio;

/// <summary>Verification access to <see cref="SoundEffectLibraries"/> members production does not use.</summary>
internal static class SoundEffectLibrariesAccess
{
    /// <summary>Converts raw cartridge library bytes to the bounded sound-library type.</summary>
    extension(SoundEffectLibraries)
    {
        /// <summary>Validates a raw cartridge library number before it enters typed code.</summary>
        internal static SoundEffectLibrary FromCartridge(byte value, string source) => value switch
        {
            1 => SoundEffectLibrary.Library1,
            2 => SoundEffectLibrary.Library2,
            3 => SoundEffectLibrary.Library3,
            _ => throw new InvalidDataException(
                $"{source} contains sound-effect library {value}, expected one through three."),
        };
    }
}

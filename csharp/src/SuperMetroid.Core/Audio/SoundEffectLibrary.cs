namespace SuperMetroid.Core.Audio;

/// <summary>
/// The three mutually exclusive sound-effect queues exposed by the retail cartridge.
/// These values select different SPC input ports and are not combinable flags.
/// </summary>
public enum SoundEffectLibrary : byte
{
    Library1 = 1,
    Library2 = 2,
    Library3 = 3,
}

/// <summary>Checked conversions for raw instruction data and zero-based queue storage.</summary>
public static class SoundEffectLibraries
{

    /// <summary>Returns the zero-based host queue for a validated library selector.</summary>
    public static int ToQueueIndex(SoundEffectLibrary library) => library switch
    {
        SoundEffectLibrary.Library1 => 0,
        SoundEffectLibrary.Library2 => 1,
        SoundEffectLibrary.Library3 => 2,
        _ => throw new ArgumentOutOfRangeException(
            nameof(library), library, "Unknown sound-effect library."),
    };
}

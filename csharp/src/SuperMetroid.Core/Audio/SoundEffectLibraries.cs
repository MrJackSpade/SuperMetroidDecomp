namespace SuperMetroid.Core.Audio;

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

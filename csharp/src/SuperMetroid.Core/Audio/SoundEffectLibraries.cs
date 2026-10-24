using System.Collections.Immutable;

namespace SuperMetroid.Core.Audio;

/// <summary>Checked conversions for raw instruction data and zero-based queue storage.</summary>
public static class SoundEffectLibraries
{
    /// <summary>The number of sound-effect libraries, each with its own host queue and APU port.</summary>
    public const int Count = SoundClockTiming.LibraryCount;

    /// <summary>The libraries in host queue order: queue index N is library N + 1.</summary>
    public static ImmutableArray<SoundEffectLibrary> InQueueOrder { get; } =
        [SoundEffectLibrary.Library1, SoundEffectLibrary.Library2, SoundEffectLibrary.Library3];

    /// <summary>The library on zero-based host queue <paramref name="queueIndex"/>.</summary>
    /// <param name="queueIndex">The host queue index, 0 through 2.</param>
    public static SoundEffectLibrary FromQueueIndex(int queueIndex) => InQueueOrder[queueIndex];

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

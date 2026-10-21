using System.Collections.Immutable;

namespace SuperMetroid.Core.Audio;

/// <summary>Domain-owned transitions over <see cref="ApuPort"/>.</summary>
public static class ApuPorts
{
    /// <summary>The sound-library ports in host queue order: the cartridge writes library queue X to <c>$2141+X</c>.</summary>
    public static ImmutableArray<ApuPort> SoundLibraryPorts { get; } =
        [ApuPort.SoundLibrary1, ApuPort.SoundLibrary2, ApuPort.SoundLibrary3];

    /// <summary>The port that carries zero-based sound library <paramref name="libraryIndex"/>.</summary>
    /// <param name="libraryIndex">The host queue index, 0 through 2.</param>
    public static ApuPort ForSoundLibrary(int libraryIndex) => SoundLibraryPorts[libraryIndex];

    /// <summary>The port that carries <paramref name="library"/>'s requests.</summary>
    /// <param name="library">The sound-effect library.</param>
    public static ApuPort ForLibrary(SoundEffectLibrary library) => ForSoundLibrary(SoundEffectLibraries.ToQueueIndex(library));

    /// <summary>The zero-based index of the sound library whose requests <paramref name="port"/> carries.</summary>
    /// <param name="port">A sound-library port.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> carries music, not a sound library.</exception>
    public static int LibraryIndexAt(ApuPort port) => SoundLibraryPorts.IndexOf(port) is int index and >= 0
        ? index
        : throw new ArgumentOutOfRangeException(nameof(port), port, "The port carries no sound library.");
}

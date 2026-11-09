using System.Collections.Immutable;

namespace SuperMetroid.Core.Audio;

/// <summary>The game's sound-effect libraries in queue order, and the positions of the game's per-library queues.</summary>
public static class SoundEffectLibraries
{
    /// <summary>
    /// The libraries in queue order: Samus's, the enemies', the room's. Each sound clock request check serves them in
    /// this order, and each library's sound queue and clip state is kept at its position in it.
    /// </summary>
    public static ImmutableArray<SoundEffectLibrary> InQueueOrder { get; } =
        [SoundEffectLibrary.Samus, SoundEffectLibrary.Enemies, SoundEffectLibrary.Room];

    /// <summary>The number of sound-effect libraries, each with its own request queue.</summary>
    public static int Count => InQueueOrder.Length;

    /// <summary>The library whose queue is at zero-based <paramref name="queueIndex"/>.</summary>
    /// <param name="queueIndex">The queue position, below <see cref="Count"/>.</param>
    /// <exception cref="IndexOutOfRangeException"><paramref name="queueIndex"/> is not a queue position.</exception>
    public static SoundEffectLibrary FromQueueIndex(int queueIndex) => InQueueOrder[queueIndex];

    /// <summary>The zero-based position of <paramref name="library"/>'s queue.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="library"/> is not a sound-effect library.</exception>
    public static int ToQueueIndex(SoundEffectLibrary library)
    {
        int index = InQueueOrder.IndexOf(library);
        return index >= 0 ? index : throw new ArgumentOutOfRangeException(nameof(library), library, "Unknown sound-effect library.");
    }
}

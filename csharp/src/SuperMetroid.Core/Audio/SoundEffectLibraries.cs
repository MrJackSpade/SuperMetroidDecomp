using System.Collections.Immutable;

namespace SuperMetroid.Core.Audio;

/// <summary>The game's sound-effect libraries in the order each request check serves them.</summary>
public static class SoundEffectLibraries
{
    /// <summary>
    /// The libraries in queue order: Samus's, the enemies', the room's. Each sound clock request check serves them in
    /// this order. Each library's sound queue and clip state is kept by the library itself (#1755).
    /// </summary>
    public static ImmutableArray<SoundEffectLibrary> InQueueOrder { get; } =
        [SoundEffectLibrary.Samus, SoundEffectLibrary.Enemies, SoundEffectLibrary.Room];

    /// <summary>The number of sound-effect libraries, each with its own request queue.</summary>
    public static int Count => InQueueOrder.Length;
}

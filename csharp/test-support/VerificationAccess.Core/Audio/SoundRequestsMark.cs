using System.Collections.Immutable;

namespace SuperMetroid.Core.Audio;

/// <summary>
/// One sound library's requests at a moment a check marks: the request its handler has taken from the queue and not yet
/// withdrawn, then the requests still waiting, with the handler's handshake phase. A later reading finds the requests
/// queued since (<see cref="AudioQueuesAccess"/>).
/// </summary>
/// <param name="Library">The library marked.</param>
/// <param name="Held">The request the library's handler has taken and not withdrawn, or none.</param>
/// <param name="Phase">Where the library's handler was in its handshake.</param>
/// <param name="Waiting">The requests waiting in the library's queue, oldest first.</param>
internal readonly record struct SoundRequestsMark(
    SoundEffectLibrary Library,
    SoundEffectId? Held,
    SoundRequestHandshakes<SoundEffectLibrary, SoundEffectId>.Phase Phase,
    ImmutableArray<SoundEffectId> Waiting)
{
    /// <summary>The held request, if any, followed by the waiting requests: every request of the library not yet withdrawn, oldest first.</summary>
    internal ImmutableArray<SoundEffectId> HeldThenWaiting =>
        Held is SoundEffectId held ? Waiting.Insert(0, held) : Waiting;
}

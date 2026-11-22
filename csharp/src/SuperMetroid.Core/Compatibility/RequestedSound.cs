namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// A sound a library is asked for. A library that is asked for nothing holds no request, so where the sound
/// acceptance clock, its checks, acknowledgements and the sound queues' handshakes may hold nothing they hold an
/// absent request, never an absent sound (#2554).
/// </summary>
/// <typeparam name="TSound">The sound a library is asked for.</typeparam>
/// <param name="Sound">The sound asked for.</param>
/// <remarks>
/// Made wherever a sound is requested of the clock or awaited by a handshake; compared by the clock's checks and the
/// handshakes' acknowledgements. Evidence: the original sound program's request ports carry one sound number or
/// nothing, which the sound acceptance clock check compares with a recording of the emulated sound program.
/// </remarks>
public readonly partial record struct RequestedSound<TSound>(TSound Sound)
    where TSound : notnull, IEquatable<TSound>;

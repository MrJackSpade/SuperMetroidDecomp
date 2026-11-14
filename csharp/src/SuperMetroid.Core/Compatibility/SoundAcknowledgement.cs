namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What one sound library reports back to the game's sound queue: nothing, the request it acted on, or, for the
/// music-load reporter while new music loads, the load in progress, which matches no request
/// (<see cref="SoundAcceptanceClock{TLibrary, TSound}"/>).
/// </summary>
/// <typeparam name="TSound">The sound a library is asked for.</typeparam>
/// <remarks>
/// Built by the sound acceptance clock at each acknowledgement; read by the sound queues' request handshakes every update.
/// Evidence: the original sound program replies to each library's handler with the request it last acted on, or with
/// nothing, and while new music loads the library that reports music loads replies with the load instead, which no
/// request matches; the sound
/// acceptance clock check compares these replies on every update with a recording of the emulated sound program's.
/// </remarks>
[CompatibilityService("what one sound library reports back as acknowledged")]
public readonly record struct SoundAcknowledgement<TSound>
    where TSound : struct, IEquatable<TSound>
{

    /// <summary>The music-load reporter reports new music loading instead of an acknowledgement; it matches no request.</summary>
    public static SoundAcknowledgement<TSound> MusicLoading { get; } = new() { IsMusicLoading = true };

    /// <summary>The library acknowledges <paramref name="sound"/>, or reports nothing for none.</summary>
    public static SoundAcknowledgement<TSound> Of(TSound? sound) => new() { Sound = sound };

    /// <summary>The acknowledged request, or none.</summary>
    public TSound? Sound { get; private init; }

    /// <summary>Whether the library reports new music loading.</summary>
    public bool IsMusicLoading { get; private init; }

    /// <summary>Whether the library reports nothing at all.</summary>
    public bool IsNone => !IsMusicLoading && Sound is null;

    /// <summary>Whether this acknowledges <paramref name="request"/>; nothing acknowledges no request.</summary>
    public bool Acknowledges(TSound? request) => !IsMusicLoading && Nullable.Equals(Sound, request);

    /// <inheritdoc />
    public override string ToString() => IsMusicLoading ? "music loading" : Sound?.ToString() ?? "none";
}

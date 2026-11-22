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
public readonly partial record struct SoundAcknowledgement<TSound>
    where TSound : notnull, IEquatable<TSound>
{

    /// <summary>The music-load reporter reports new music loading instead of an acknowledgement; it matches no request.</summary>
    public static SoundAcknowledgement<TSound> MusicLoading { get; } = new(new MusicLoadingReport());

    /// <summary>The library acknowledges <paramref name="request"/>, or reports nothing for no request.</summary>
    public static SoundAcknowledgement<TSound> Of(RequestedSound<TSound>? request) => request is { } acknowledged ? new(new SoundReport(acknowledged)) : default;

    /// <summary>The acknowledged request, or none.</summary>
    private RequestedSound<TSound>? Request => reported is SoundReport { Sound: var sound } ? sound : null;

    /// <summary>Whether the library reports new music loading.</summary>
    private bool IsMusicLoading => reported is MusicLoadingReport;

    /// <summary>What the library reports (#2560): an acknowledged sound or music loading; null when it reports nothing.</summary>
    private readonly Report? reported;

    /// <summary>Holds what the library reports.</summary>
    private SoundAcknowledgement(Report reported) => this.reported = reported;

    /// <summary>Whether the library reports nothing at all.</summary>
    public bool IsNone => !IsMusicLoading && Request is null;

    /// <summary>Whether this acknowledges <paramref name="request"/>; nothing acknowledges no request.</summary>
    public bool Acknowledges(RequestedSound<TSound>? request) => !IsMusicLoading && Nullable.Equals(Request, request);

    /// <inheritdoc />
    public override string ToString() => IsMusicLoading ? "music loading" : Request?.Sound.ToString() ?? "none";

    /// <summary>What a library reports back (#2560): a closed choice of an acknowledged sound and music loading.</summary>
    private abstract partial record Report;

    /// <summary>The library acknowledges a sound.</summary>
    /// <param name="Sound">The sound acknowledged.</param>
    private sealed partial record SoundReport(RequestedSound<TSound> Sound) : Report;

    /// <summary>The sound driver is loading music, which acknowledges no sound.</summary>
    private sealed partial record MusicLoadingReport : Report;
}

namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What one sound library reports back to the game's sound handler: nothing, the request it acted on, or, for library
/// one while new music loads, the load in progress, which matches no request (<see cref="SoundAcceptanceClock{TSound}"/>).
/// </summary>
/// <typeparam name="TSound">The sound a library is asked for.</typeparam>
[CompatibilityService("what one sound library reports back as acknowledged")]
public readonly record struct SoundAcknowledgement<TSound>
    where TSound : struct, IEquatable<TSound>
{

    /// <summary>Library one reports new music loading instead of an acknowledgement; it matches no request.</summary>
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

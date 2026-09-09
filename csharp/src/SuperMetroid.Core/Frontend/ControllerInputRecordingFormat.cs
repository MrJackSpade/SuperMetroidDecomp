namespace SuperMetroid.Core.Frontend;

/// <summary>Named bit assignments in the version-one deterministic input-recording header.</summary>
internal static class ControllerInputRecordingFormat
{
    /// <summary>Host option bit preserving the opening-cinematic skip.</summary>
    public const byte SkipOpeningCinematic = 1 << 0;

    /// <summary>Host option bit preserving the nonlethal damage guard.</summary>
    public const byte Invincibility = 1 << 1;

    /// <summary>Host option bit preserving the unlocked-ammunition floor.</summary>
    public const byte InfiniteAmmo = 1 << 2;

    /// <summary>Host option bit preserving the one-second escape-countdown floor.</summary>
    public const byte PreventEscapeTimeout = 1 << 5;

    /// <summary>First bit of the two-bit mutually exclusive map-reveal value.</summary>
    public const int MapRevealShift = 3;

    /// <summary>Two-bit field containing <see cref="Game.MapRevealMode"/>.</summary>
    public const byte MapRevealMask = 0b0001_1000;

    /// <summary>Every option bit currently understood by the version-one reader.</summary>
    public const byte KnownOptionMask = SkipOpeningCinematic | Invincibility | InfiniteAmmo |
        MapRevealMask | PreventEscapeTimeout;
}

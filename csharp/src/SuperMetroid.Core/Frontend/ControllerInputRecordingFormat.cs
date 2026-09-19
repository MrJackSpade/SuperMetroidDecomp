namespace SuperMetroid.Core.Frontend;

/// <summary>Named bit assignments in the shared deterministic input-recording header.</summary>
internal static class ControllerInputRecordingFormat
{
    /// <summary>Eight-byte signature shared by every supported recording version.</summary>
    public static ReadOnlySpan<byte> Magic => "SMINPUT1"u8;

    /// <summary>Original recording format containing ROM, options, SRAM, and inputs.</summary>
    public const uint LegacyFormatVersion = 1;

    /// <summary>Recording format which adds installed-content identity.</summary>
    public const uint CurrentFormatVersion = 2;

    /// <summary>Byte length of every SHA-256 digest stored by the format.</summary>
    public const int DigestByteCount = 32;

    /// <summary>Byte length of a serialized <see cref="Guid"/> build ID.</summary>
    public const int BuildIdByteCount = 16;

    /// <summary>Byte count of the original fixed header.</summary>
    public const int LegacyHeaderByteCount = 8 + sizeof(uint) + sizeof(long) + 8 +
        DigestByteCount + sizeof(int) + sizeof(int);

    /// <summary>Byte count of the installed-content identity extension.</summary>
    public const int ContentIdentityByteCount = sizeof(int) + BuildIdByteCount + 4 * DigestByteCount;

    /// <summary>Byte count of the current fixed header.</summary>
    public const int CurrentHeaderByteCount = LegacyHeaderByteCount + ContentIdentityByteCount;

    /// <summary>Offset of the source-cartridge digest in every header.</summary>
    public const int RomDigestOffset = 28;

    /// <summary>Offset of the identity-contract version in a current header.</summary>
    public const int ContentIdentityOffset = LegacyHeaderByteCount;

    /// <summary>Offset of the compiled-definition build ID in a current header.</summary>
    public const int ContentBuildIdOffset = ContentIdentityOffset + sizeof(int);

    /// <summary>Offset of the selected-audio digest in a current header.</summary>
    public const int AudioDigestOffset = ContentBuildIdOffset + BuildIdByteCount;

    /// <summary>Offset of the selected-map digest in a current header.</summary>
    public const int MapDigestOffset = AudioDigestOffset + DigestByteCount;

    /// <summary>Offset of the selected-projectile digest in a current header.</summary>
    public const int ProjectileDigestOffset = MapDigestOffset + DigestByteCount;

    /// <summary>Offset of the aggregate installed-content digest in a current header.</summary>
    public const int CompositeDigestOffset = ProjectileDigestOffset + DigestByteCount;

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

    /// <summary>Every option bit currently understood by the recording reader.</summary>
    public const byte KnownOptionMask = SkipOpeningCinematic | Invincibility | InfiniteAmmo |
        MapRevealMask | PreventEscapeTimeout;
}

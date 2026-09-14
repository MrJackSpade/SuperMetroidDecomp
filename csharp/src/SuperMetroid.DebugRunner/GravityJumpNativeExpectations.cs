/// <summary>Measured original-CPU expectations for the room-local #439 fixture.</summary>
internal static class GravityJumpNativeExpectations
{
    /// <summary>Pinned unheadered Japan/USA cartridge used by this comparison.</summary>
    public const string RomSha256 = "12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72";
    /// <summary>Original-CPU trace identity, UTF-8 with LF line endings.</summary>
    public const string TraceSha256 = "AF4F5C60E424E1018129A52B47346A5F4021FA79BE46EB768CDE3230C7660E39";
    /// <summary>Lowest Y reached with Gravity kept equipped, for all five tested launch frames.</summary>
    public const uint SuitedApex = 0x017ce7ff;
    /// <summary>Lowest Y of an ordinary suitless jump, including launching after Gravity removal.</summary>
    public const uint SuitlessApex = 0x01ba1fff;
    /// <summary>Suited launch speed retained across equipment-menu freeze.</summary>
    public const uint SuitedLaunchSpeed = 0x0004e000;
    /// <summary>Ordinary suitless underwater launch speed.</summary>
    public const uint SuitlessLaunchSpeed = 0x0001c000;
    /// <summary>Gravity removal apexes for launch frames 27 through 31, in input order.</summary>
    public static ReadOnlySpan<uint> RemovedGravityApexes =>
        [0x008523ff, 0x00795bff, 0x006d4fff, 0x006d4fff, SuitlessApex];
}

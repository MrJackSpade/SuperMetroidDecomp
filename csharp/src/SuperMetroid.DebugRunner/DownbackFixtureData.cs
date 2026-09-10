/// <summary>Authored room boundaries and native witnesses for the downback audit.</summary>
internal static class DownbackFixtureData
{
    /// <summary>Bank-$A0:DCFF Zoomer definition, used for actual ordinary touch damage.</summary>
    public const ushort ZoomerDefinition = 0xdcff;
    /// <summary>The close passage occupies column 66 facing right, column 61 facing left.</summary>
    public static int PassageColumn(bool left) => left ? 61 : 66;
    /// <summary>Native right-facing center stopped against the passage without compression.</summary>
    public const uint BlockedRightCenter = 0x041bffff;
    /// <summary>Native left-facing center stopped against the passage without compression.</summary>
    public const uint BlockedLeftCenter = 0x03e50000;
    /// <summary>Native fractional falling velocity preserved by a collision-selected crouch.</summary>
    public const ushort RejectedLandingSubspeed = 0x00c4 << 8;
}

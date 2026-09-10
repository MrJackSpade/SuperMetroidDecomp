/// <summary>Original-CPU witnesses for the authored corner-jump ledge.</summary>
internal static class CornerJumpFixtureData
{
    /// <summary>Right-going support ends at column 66; left-going support begins at column 62.</summary>
    public static int SupportEdge(bool left) => (left ? 62 : 66) * 16;
    /// <summary>Post-turn launch uses original dry-air spin velocity $0004:E000.</summary>
    public const ushort LaunchSpeed = 4;
    /// <summary>Fractional half of the native post-turn spin launch velocity.</summary>
    public const ushort LaunchSubspeed = 0xe000;
    /// <summary>Frame-12 launch center in the right-going case (turn frame six, Jump six frames later).</summary>
    public const uint RightLaunchCenter = 0x0200efff;
    /// <summary>Same witness facing left; block scan asymmetry is intentionally retained.</summary>
    public const uint LeftLaunchCenter = 0x02069fff;
}

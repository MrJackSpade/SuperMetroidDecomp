namespace SuperMetroid.Core.Game;

/// <summary>Phantoon movement, timing, palette, and flame-pattern data.</summary>
internal static class EnemyRomTablePointersPhantoon
{
    /// <summary>Eight first-round hiding timer words at $A7:CD41 (16 bytes).</summary>
    public const int FirstRoundHidingTimerWords = 0xa7cd41;
    /// <summary>Eight eye-closed timer words at $A7:CD53 (16 bytes).</summary>
    public const int EyeClosedTimerWords = 0xa7cd53;
    /// <summary>Figure-eight speed and limit words at $A7:CD73.</summary>
    public const int FigureEightMotionWords = 0xa7cd73;
    /// <summary>Eight random direction bytes at $A7:CDA5.</summary>
    public const int RandomDirectionBytes = 0xa7cda5;
    /// <summary>Mouth flame-pattern pointer words at $A7:CCFD.</summary>
    public const int MouthPatternPointerWords = 0xa7ccfd;
    /// <summary>Phantoon flame random-angle bytes at $86:98B4.</summary>
    public const int FlameAngleBytes = 0x8698b4;
    /// <summary>Phantoon flame-rain X-position bytes at $86:98F7.</summary>
    public const int FlameRainXBytes = 0x8698f7;
    /// <summary>Phantoon spiral-flame angle bytes at $86:9979.</summary>
    public const int SpiralAngleBytes = 0x869979;
}

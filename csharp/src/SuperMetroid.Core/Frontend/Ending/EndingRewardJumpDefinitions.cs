namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Bank-$8B jump, falling and landing actor definitions used after the reward gesture.</summary>
internal static class EndingRewardJumpDefinitions
{
    /// <summary>Initial upward 16.16 velocity applied when the reward jump begins.</summary>
    public const int LaunchVelocity = -16 * 65536;
    /// <summary>$8B:F65B adds $0000:3800 to the shared velocity before each actor's movement.</summary>
    public const int Gravity = 14336;
    /// <summary>Crossing this height switches the body to its falling sheet and deletes the old head.</summary>
    public const int SheetSwitchY = -80;
    /// <summary>The body stops here before its landed instruction list begins.</summary>
    public const int LandingY = 136;
    /// <summary>Number of reward icon chunks shown one a frame during the returning-body phase.</summary>
    public const int IconChunkCount = 16;
}

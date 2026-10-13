namespace SuperMetroid.Core.Frontend;

/// <summary>Bank-$8B pre-instructions owned by the reward jump; the actors otherwise run the shared no-op or none.</summary>
internal enum EndingRewardJumpPreInstruction : ushort
{
    /// <summary>$8B:F528, body flight pre-instruction, switches to the falling sheet above Y=-80.</summary>
    BodyFlight = 0xf528,
    /// <summary>$8B:F57F, head flight pre-instruction, deletes the old head above Y=-80.</summary>
    HeadFlight = 0xf57f,
    /// <summary>$8B:F5DD, returning body pre-instruction, queues graphics and lands at Y=136.</summary>
    Landing = 0xf5dd,
}

/// <summary>Bank-$8B jump, falling and landing actor definitions used after the reward gesture.</summary>
internal static class EndingRewardJumpDefinitions
{
    /// <summary>$8B:ED95 replaces the rising body animation when its ascent passes Y=-80.</summary>
    public const ushort FallingList = 0xed95;
    /// <summary>$8B:ED9D takes over after the body is clamped to its landing height.</summary>
    public const ushort LandedList = 0xed9d;
    /// <summary>Initial upward 16.16 velocity applied when the reward jump begins.</summary>
    public const int LaunchVelocity = -16 * 65536;
    /// <summary>$8B:F65B adds $0000:3800 to the shared velocity before each actor's movement.</summary>
    public const int Gravity = 0x3800;
    /// <summary>Crossing this height switches the body to its falling sheet and deletes the old head.</summary>
    public const int SheetSwitchY = -80;
    /// <summary>The body stops here before its landed instruction list begins.</summary>
    public const int LandingY = 136;
    /// <summary>Number of sequential graphics uploads queued during the returning-body phase.</summary>
    public const int UploadCount = 16;
}

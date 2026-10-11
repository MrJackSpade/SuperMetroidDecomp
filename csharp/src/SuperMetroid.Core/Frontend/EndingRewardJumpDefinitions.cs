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
    /// <summary>$8B:ED95/ED9D, falling and landing animation lists.</summary>
    public const ushort FallingList = 0xed95, LandedList = 0xed9d;
    public const int LaunchVelocity = -16 * 65536;
    /// <summary>$8B:F65B adds $0000:3800 to the shared velocity before each actor's movement.</summary>
    public const int Gravity = 0x3800;
    public const int SheetSwitchY = -80, LandingY = 136, UploadCount = 16;
}

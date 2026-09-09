namespace SuperMetroid.Core.Frontend;

/// <summary>Bank-$8B jump, falling and landing actor definitions used after the reward gesture.</summary>
internal static class EndingRewardJumpDefinitions
{
    /// <summary>$8B:EF3F, suitless jumping actor spawned by F51D.</summary>
    public const ushort SuitlessBody = 0xef3f;
    /// <summary>$8B:EF6F/EF75/EF7B, suited jump body and helmeted/helmetless heads spawned by F554.</summary>
    public const ushort SuitedBody = 0xef6f, HelmetedHead = 0xef75, HelmetlessHead = 0xef7b;
    /// <summary>$8B:F528, body flight pre-instruction, switches to the falling sheet above Y=-80.</summary>
    public const ushort BodyFlight = 0xf528;
    /// <summary>$8B:F57F, head flight pre-instruction, deletes the old head above Y=-80.</summary>
    public const ushort HeadFlight = 0xf57f;
    /// <summary>$8B:F5DD, returning body pre-instruction, queues graphics and lands at Y=136.</summary>
    public const ushort Landing = 0xf5dd;
    /// <summary>$8B:ED95/ED9D, falling and landing animation lists.</summary>
    public const ushort FallingList = 0xed95, LandedList = 0xed9d;
    /// <summary>$8B:F597/F5BA, reposition the adjacent head for preparation and takeoff.</summary>
    public const ushort PrepareHead = 0xf597, LaunchHead = 0xf5ba;
    /// <summary>$8B:F651 initializes the shared vertical velocity to -16 pixels/frame.</summary>
    public const ushort Launch = 0xf651;
    /// <summary>$8B:F604 requests the screen shot sequence and its sound after landing.</summary>
    public const ushort Shoot = 0xf604;
    public const int LaunchVelocity = -16 * 65536;
    /// <summary>$8B:F65B adds $0000:3800 to the shared velocity before each actor's movement.</summary>
    public const int Gravity = 0x3800;
    public const int SheetSwitchY = -80, LandingY = 136, UploadCount = 16;
}

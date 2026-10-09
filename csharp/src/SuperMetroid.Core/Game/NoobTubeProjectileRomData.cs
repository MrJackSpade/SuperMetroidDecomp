namespace SuperMetroid.Core.Game;

/// <summary>Bank-$86 tables and fixed-point constants for n00b-tube projectiles.</summary>
public static class NoobTubeProjectileRomData
{
    private static readonly short[] ShardXOffsetWords =
        [-56, -64, -20, -40, -64, -48, -24, -40, 0, -8];
    private static readonly short[] ShardYOffsetWords =
        [8, -12, -26, -24, -32, 28, 16, -8, -24, 16];
    private static readonly short[] ShardXVelocityWords =
        [-384, -384, -160, -288, -288, -320, -96, -352, 0, -64];
    private static readonly short[] ShardYVelocityWords =
        [320, -256, -416, -288, -288, 448, 576, -96, -288, 384];
    private static readonly ushort[] BubbleXOffsetWords = [40, 80, 104, 120, 152, 184];
    private static readonly ushort[] BubbleYOffsetWords = [80, 72, 84, 32, 64, 84];

    /// <summary>Signed X offsets from the tube crack for parameters $00-$12.</summary>
    public static ReadOnlySpan<short> ShardXOffsets => ShardXOffsetWords;

    /// <summary>Signed Y offsets from the tube crack for parameters $00-$12.</summary>
    public static ReadOnlySpan<short> ShardYOffsets => ShardYOffsetWords;

    /// <summary>Signed 8.8 initial horizontal velocities for parameters $00-$12.</summary>
    public static ReadOnlySpan<short> ShardXVelocities => ShardXVelocityWords;

    /// <summary>Signed 8.8 initial vertical velocities for parameters $00-$12.</summary>
    public static ReadOnlySpan<short> ShardYVelocities => ShardYVelocityWords;

    /// <summary>Bank-$86 animation-list pointers selected by shard parameter.</summary>
    public static NoobTubeShardProgramSequence ShardInstructionLists =>
        NoobTubeProjectileInstructionProgramDefinitions.ShardInstructionLists;

    /// <summary>Absolute block-origin X offsets for the six released-air bubbles.</summary>
    public static ReadOnlySpan<ushort> BubbleXOffsets => BubbleXOffsetWords;

    /// <summary>Absolute block-origin Y offsets for the six released-air bubbles.</summary>
    public static ReadOnlySpan<ushort> BubbleYOffsets => BubbleYOffsetWords;

    /// <summary>Whole-pixel X offset from the spawning PLM block origin to the tube crack.</summary>
    public const ushort CrackXOffset = 96;

    /// <summary>Whole-pixel Y offset from the spawning PLM block origin to the tube crack.</summary>
    public const ushort CrackYOffset = 48;

    /// <summary>Initial signed 8.8 bubble Y velocity, <c>$FB00</c> or five pixels upward per update.</summary>
    public const ushort BubbleInitialYVelocity = 0xfb00;

    /// <summary>Signed 8.8 downward velocity of the released tube crack, three quarters of a pixel per update.</summary>
    public const ushort CrackFallVelocity = 0x00c0;

    /// <summary>Signed 8.8 downward velocity assigned when a glass shard enters its falling arc.</summary>
    public const ushort ShardFallYVelocity = 0x00c0;

    /// <summary>Off-screen whole-pixel X coordinate assigned when a released actor is hidden.</summary>
    public const ushort HiddenXPosition = 0xee00;
}

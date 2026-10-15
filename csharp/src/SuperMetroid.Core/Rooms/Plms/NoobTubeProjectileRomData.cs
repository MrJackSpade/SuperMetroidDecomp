namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Bank-$86 tables and fixed-point constants for n00b-tube projectiles.</summary>
public static class NoobTubeProjectileRomData
{
    /// <summary>Ten signed launch offsets for the released glass shards, indexed by even parameters $00-$12.</summary>
    private static readonly Offset16<RoomPixels>[] ShardOffsetWords =
        [new(-56, 8), new(-64, -12), new(-20, -26), new(-40, -24), new(-64, -32), new(-48, 28), new(-24, 16), new(-40, -8), new(0, -24), new(-8, 16)];
    /// <summary>Matching 8.8 launch vectors; shard index selects this and the offset table together.</summary>
    private static readonly EightEightVector<RoomPixels>[] ShardVelocityWords =
        [new(-384, 320), new(-384, -256), new(-160, -416), new(-288, -288), new(-288, -288), new(-320, 448), new(-96, 576), new(-352, -96), new(0, -288), new(-64, 384)];
    /// <summary>Six fixed spawn locations for the air bubbles released when the tube breaks.</summary>
    private static readonly Offset16<RoomPixels>[] BubbleOffsetWords =
        [new(40, 80), new(80, 72), new(104, 84), new(120, 32), new(152, 64), new(184, 84)];

    /// <summary>Signed offsets from the tube crack for parameters $00-$12.</summary>
    public static ReadOnlySpan<Offset16<RoomPixels>> ShardOffsets => ShardOffsetWords;

    /// <summary>Signed 8.8 initial velocities for parameters $00-$12.</summary>
    public static ReadOnlySpan<EightEightVector<RoomPixels>> ShardVelocities => ShardVelocityWords;

    /// <summary>
    /// Twice the column a reflecting shard flicker mirrors about: on even frames the shard shows at this span less its
    /// own column.
    /// </summary>
    public const ushort ShardMirrorSpan = 256;

    /// <summary>Offsets from the spawning PLM block origin for the six released-air bubbles.</summary>
    public static ReadOnlySpan<Offset16<RoomPixels>> BubbleOffsets => BubbleOffsetWords;

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

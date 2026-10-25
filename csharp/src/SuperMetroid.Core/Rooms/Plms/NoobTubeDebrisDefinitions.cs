namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Where the n00b tube's crack, glass shards and released-air bubbles appear and how they move.</summary>
public static class NoobTubeDebrisDefinitions
{
    /// <summary>Ten signed launch offsets for the released glass shards, one per shard.</summary>
    private static readonly Offset16<RoomPixels>[] ShardOffsetValues =
        [new(-56, 8), new(-64, -12), new(-20, -26), new(-40, -24), new(-64, -32), new(-48, 28), new(-24, 16), new(-40, -8), new(0, -24), new(-8, 16)];
    /// <summary>Matching 8.8 launch vectors; the shard selects this and its offset together.</summary>
    private static readonly EightEightVector<RoomPixels>[] ShardVelocityValues =
        [new(-384, 320), new(-384, -256), new(-160, -416), new(-288, -288), new(-288, -288), new(-320, 448), new(-96, 576), new(-352, -96), new(0, -288), new(-64, 384)];
    /// <summary>Six fixed spawn locations for the air bubbles released when the tube breaks.</summary>
    private static readonly Offset16<RoomPixels>[] BubbleOffsetValues =
        [new(40, 80), new(80, 72), new(104, 84), new(120, 32), new(152, 64), new(184, 84)];

    /// <summary>Signed offsets from the tube crack, one per shard.</summary>
    public static ReadOnlySpan<Offset16<RoomPixels>> ShardOffsets => ShardOffsetValues;

    /// <summary>Signed 8.8 initial velocities, one per shard.</summary>
    public static ReadOnlySpan<EightEightVector<RoomPixels>> ShardVelocities => ShardVelocityValues;

    /// <summary>
    /// Twice the column a reflecting shard flicker mirrors about: on even frames the shard shows at this span less its
    /// own column.
    /// </summary>
    public const ushort ShardMirrorSpan = 256;

    /// <summary>Offsets from the spawning PLM block origin for the six released-air bubbles.</summary>
    public static ReadOnlySpan<Offset16<RoomPixels>> BubbleOffsets => BubbleOffsetValues;

    /// <summary>Whole-pixel X offset from the spawning PLM block origin to the tube crack.</summary>
    public const ushort CrackXOffset = 96;

    /// <summary>Whole-pixel Y offset from the spawning PLM block origin to the tube crack.</summary>
    public const ushort CrackYOffset = 48;

    /// <summary>The bubbles' initial vertical speed: five pixels upward per update.</summary>
    public static SnesSignedEightEight BubbleInitialYVelocity { get; } = SnesSignedEightEight.FromSignedRaw(-1280);

    /// <summary>The released crack's downward speed: three quarters of a pixel per update.</summary>
    public static SnesSignedEightEight CrackFallVelocity { get; } = SnesSignedEightEight.FromSignedRaw(192);

    /// <summary>The downward speed a glass shard takes when it enters its falling arc: three quarters of a pixel per update.</summary>
    public static SnesSignedEightEight ShardFallYVelocity { get; } = SnesSignedEightEight.FromSignedRaw(192);

    /// <summary>The whole-pixel X a hidden actor is moved to: 4608 pixels left of the room, far off screen.</summary>
    private const short HiddenX = -4608;

    /// <summary>The stored whole-pixel X of a hidden actor (<see cref="HiddenX"/> in the position's wrapping word).</summary>
    public static ushort HiddenXPosition { get; } = unchecked((ushort)HiddenX);
}

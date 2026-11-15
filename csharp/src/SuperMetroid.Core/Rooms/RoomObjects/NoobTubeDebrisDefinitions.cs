using System.Collections.Immutable;
using S = SuperMetroid.Core.Rooms.RoomObjects.NoobTubeProjectileSegment;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Where the n00b tube's crack, glass shards and released-air bubbles appear and how they move.</summary>
public static class NoobTubeDebrisDefinitions
{
    /// <summary>Where the tube cracks: 96 pixels right of and 48 below the top-left corner of the tube's block.</summary>
    private static readonly Offset16<RoomPixels> CrackOffset = new(96, 48);

    /// <summary>The bubbles' initial vertical speed: five pixels upward per update.</summary>
    public static SignedEightEight BubbleInitialYVelocity { get; } = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(-1280);

    /// <summary>The tube's crack, which spreads across the glass where the tube cracks.</summary>
    public static NoobTubePiece Crack { get; } =
        new(NoobTubeProjectileDefinition.Crack, CrackOffset, EightEightVector<RoomPixels>.Zero, S.Crack);

    /// <summary>
    /// The ten glass shards in the order they burst, each placed from the crack and flying off at its own speed in 256ths
    /// of a pixel an update.
    /// </summary>
    public static ImmutableArray<NoobTubePiece> Shards { get; } =
    [
        Shard(new(-56, 8), EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-384, 320), S.ShardDownOutward),
        Shard(new(-64, -12), EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-384, -256), S.ShardUpOutward),
        Shard(new(-20, -26), EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-160, -416), S.ShardUpSteep),
        Shard(new(-40, -24), EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-288, -288), S.ShardUpDiagonalInner),
        Shard(new(-64, -32), EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-288, -288), S.ShardUpDiagonalOuter),
        Shard(new(-48, 28), EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-320, 448), S.ShardDownDiagonal),
        Shard(new(-24, 16), EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-96, 576), S.ShardDownOuter),
        Shard(new(-40, -8), EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-352, -96), S.ShardOutward),
        Shard(new(0, -24), EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(0, -288), S.ShardUp),
        Shard(new(-8, 16), EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-64, 384), S.ShardDownInner),
    ];

    /// <summary>The six released-air bubbles in the order they appear, each placed from the tube's block.</summary>
    public static ImmutableArray<NoobTubePiece> Bubbles { get; } =
    [
        Bubble(new(40, 80)),
        Bubble(new(80, 72)),
        Bubble(new(104, 84)),
        Bubble(new(120, 32)),
        Bubble(new(152, 64)),
        Bubble(new(184, 84)),
    ];

    /// <summary>
    /// Twice the column a reflecting shard flicker mirrors about: on even frames the shard shows at this span less its
    /// own column.
    /// </summary>
    public const int ShardMirrorSpan = 256;

    /// <summary>The released crack's downward speed: three quarters of a pixel per update.</summary>
    public static SignedEightEight CrackFallVelocity { get; } = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(192);

    /// <summary>The downward speed a glass shard takes when it enters its falling arc: three quarters of a pixel per update.</summary>
    public static SignedEightEight ShardFallYVelocity { get; } = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(192);

    /// <summary>
    /// The random field a glass shard or air bubble starts its swaying fall from: the sample divided by 256, as the
    /// angle its sway starts at.
    /// </summary>
    internal static readonly RandomSampleField FallingSwayStart = new(8, 8);

    /// <summary>A shard <paramref name="fromCrack"/> from the crack, flying off at <paramref name="velocity"/>.</summary>
    private static NoobTubePiece Shard(Offset16<RoomPixels> fromCrack, EightEightVector<RoomPixels> velocity, NoobTubeProjectileSegment animation) =>
        new(NoobTubeProjectileDefinition.Shard, CrackOffset + fromCrack, velocity, animation);

    /// <summary>A bubble at <paramref name="offset"/> from the tube's block, rising at the bubbles' initial speed.</summary>
    private static NoobTubePiece Bubble(Offset16<RoomPixels> offset) =>
        new(NoobTubeProjectileDefinition.ReleasedAirBubble, offset,
            new EightEightVector<RoomPixels>(SignedEightEight.Zero, BubbleInitialYVelocity), S.ReleasedAirBubble);
}

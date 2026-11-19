using System.Collections.Immutable;
using P = SuperMetroid.Core.Rooms.RoomObjects.NoobTubeProjectileSegment;
using S = SuperMetroid.Core.Rooms.RoomObjects.NoobTubeShard;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Where the n00b tube's crack, glass shards and released-air bubbles appear and how they move.</summary>
public static class NoobTubeDebrisDefinitions
{
    /// <summary>Where the tube cracks: 96 pixels right of and 48 below the top-left corner of the tube's block.</summary>
    private static readonly Offset16<RoomPixels> CrackOffset = new(96, 48);

    /// <summary>The bubbles' initial vertical speed: five pixels upward per update.</summary>
    public static SignedEightEight BubbleInitialYVelocity { get; } = SignedEightEight.FromPixels(-5);

    /// <summary>The tube's crack, which spreads across the glass where the tube cracks.</summary>
    public static NoobTubePiece Crack { get; } =
        new(NoobTubeProjectileDefinition.Crack, CrackOffset, EightEightVector<RoomPixels>.Zero, new NoobTubePieceStart.AtSegment(P.Crack));

    /// <summary>
    /// The ten glass shards in the order they burst, each placed from the crack and flying off at its own speed.
    /// </summary>
    public static ImmutableArray<NoobTubePiece> Shards { get; } =
    [
        Shard(new(-56, 8), new EightEightVector<RoomPixels>(SignedEightEight.FromPixelFraction(-3, 2), SignedEightEight.FromPixelFraction(5, 4)), S.DownOutward),
        Shard(new(-64, -12), new EightEightVector<RoomPixels>(SignedEightEight.FromPixelFraction(-3, 2), SignedEightEight.FromPixels(-1)), S.UpOutward),
        Shard(new(-20, -26), new EightEightVector<RoomPixels>(SignedEightEight.FromPixelFraction(-5, 8), SignedEightEight.FromPixelFraction(-13, 8)), S.UpSteep),
        Shard(new(-40, -24), new EightEightVector<RoomPixels>(SignedEightEight.FromPixelFraction(-9, 8), SignedEightEight.FromPixelFraction(-9, 8)), S.UpDiagonalInner),
        Shard(new(-64, -32), new EightEightVector<RoomPixels>(SignedEightEight.FromPixelFraction(-9, 8), SignedEightEight.FromPixelFraction(-9, 8)), S.UpDiagonalOuter),
        Shard(new(-48, 28), new EightEightVector<RoomPixels>(SignedEightEight.FromPixelFraction(-5, 4), SignedEightEight.FromPixelFraction(7, 4)), S.DownDiagonal),
        Shard(new(-24, 16), new EightEightVector<RoomPixels>(SignedEightEight.FromPixelFraction(-3, 8), SignedEightEight.FromPixelFraction(9, 4)), S.DownOuter),
        Shard(new(-40, -8), new EightEightVector<RoomPixels>(SignedEightEight.FromPixelFraction(-11, 8), SignedEightEight.FromPixelFraction(-3, 8)), S.Outward),
        Shard(new(0, -24), new EightEightVector<RoomPixels>(SignedEightEight.Zero, SignedEightEight.FromPixelFraction(-9, 8)), S.Up),
        Shard(new(-8, 16), new EightEightVector<RoomPixels>(SignedEightEight.FromPixelFraction(-1, 4), SignedEightEight.FromPixelFraction(3, 2)), S.DownInner),
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
    public static SignedEightEight CrackFallVelocity { get; } = SignedEightEight.FromPixelFraction(3, 4);

    /// <summary>The downward speed a glass shard takes when it enters its falling arc: three quarters of a pixel per update.</summary>
    public static SignedEightEight ShardFallYVelocity { get; } = SignedEightEight.FromPixelFraction(3, 4);

    /// <summary>
    /// The random choice of the angle a glass shard's or air bubble's swaying fall starts at, any of the turn's 256 steps.
    /// </summary>
    internal static readonly RandomSampleField FallingSwayStart = new(8, 8);

    /// <summary>Shard <paramref name="shard"/>, <paramref name="fromCrack"/> from the crack, flying off at <paramref name="velocity"/>.</summary>
    private static NoobTubePiece Shard(Offset16<RoomPixels> fromCrack, EightEightVector<RoomPixels> velocity, NoobTubeShard shard) =>
        new(NoobTubeProjectileDefinition.Shard, CrackOffset + fromCrack, velocity, new NoobTubePieceStart.ShardFlight(shard));

    /// <summary>A bubble at <paramref name="offset"/> from the tube's block, rising at the bubbles' initial speed.</summary>
    private static NoobTubePiece Bubble(Offset16<RoomPixels> offset) =>
        new(NoobTubeProjectileDefinition.ReleasedAirBubble, offset,
            new EightEightVector<RoomPixels>(SignedEightEight.Zero, BubbleInitialYVelocity), new NoobTubePieceStart.AtSegment(P.ReleasedAirBubble));
}

using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The graphics and physical origins of Mother Brain's glass shards.</summary>
internal static class MotherBrainGlassShardDefinitions
{
    /// <summary>The random sample's field a shard's launch angle is: a whole turn of 256 steps.</summary>
    internal static readonly RandomSampleField LaunchAngle = new(0, 8);

    /// <summary>A launched shard's sideways speed along its launch angle's sine, in whole pixels an update.</summary>
    internal const int LaunchAcrossSpeed = 1;

    /// <summary>A launched shard's vertical speed along its launch angle's negative cosine, in whole pixels an update.</summary>
    internal const int LaunchDownSpeed = 4;

    /// <summary>
    /// A launched shard's jitter on each axis, 0 through 15 pixels less <see cref="ShardJitterCentre"/>.
    /// </summary>
    internal static readonly RandomSampleField ShardJitter = new(0, 4);

    /// <summary>
    /// The pixels taken from a launched shard's <see cref="ShardJitter"/> on each axis, so its jitter spans eight pixels
    /// before its emitter to seven past it.
    /// </summary>
    internal const int ShardJitterCentre = 8;

    /// <summary>
    /// The chance a falling shard sparkles each update, one in four: two even choices of the random number must both
    /// come up.
    /// </summary>
    internal static readonly RandomClearBitsChance ShardSparkles = RandomClearBitsChance.WhenAllClear(new RandomSampleField(5, 1), new RandomSampleField(10, 1));

    /// <summary>
    /// A sparkle's offset from its shard on each axis, 0 through 31 pixels less <see cref="SparkleOffsetCentre"/>.
    /// </summary>
    internal static readonly RandomSampleField SparkleOffset = new(0, 5);

    /// <summary>
    /// The pixels taken from a sparkle's <see cref="SparkleOffset"/> on each axis, so it shows from sixteen pixels before
    /// its shard to fifteen past it.
    /// </summary>
    internal const int SparkleOffsetCentre = 16;

    /// <summary>The bottom of the room's top screen, 256 pixels down: a shard that falls to it is removed.</summary>
    internal const int TopScreenBottomY = 256;

    /// <summary>The gravity a falling shard gains each update: an eighth of a pixel an update.</summary>
    internal static readonly SignedEightEight ShardGravity = SignedEightEight.FromPixelFraction(1, 8);

    /// <summary>
    /// The pictures the shards and sparkles show: her tank glass's pieces, in <paramref name="shading"/>, the
    /// encounter's shading.
    /// </summary>
    internal static SpriteGraphics Graphics(PictureShading shading) => new(SpawnedObjectOwner.MotherBrainTankGlass, shading);

    /// <summary>
    /// The three horizontal emitter positions on the glass: right, half a block right of the glass's origin block;
    /// left, two and a half blocks left; centre, one block left.
    /// </summary>
    internal static readonly Dictionary<MotherBrainGlassShard, int> EmitterXs = new()
    {
        [MotherBrainGlassShard.Right] = 8,
        [MotherBrainGlassShard.Left] = -40,
        [MotherBrainGlassShard.Centre] = -16,
    };

    /// <summary>Every emitter sits two blocks below the glass's origin block.</summary>
    internal const int EmitterY = 2 * 16;
}

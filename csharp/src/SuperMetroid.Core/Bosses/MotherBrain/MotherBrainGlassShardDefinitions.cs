using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The graphics and physical origins of Mother Brain's glass shards.</summary>
internal static class MotherBrainGlassShardDefinitions
{
    /// <summary>The random sample's field a shard's launch angle is: its low eight bits, a whole turn of 256 steps.</summary>
    internal static readonly RandomSampleField LaunchAngle = new(0, 8);

    /// <summary>A launched shard's jitter on each axis, 0 through 15 pixels less eight: the sample modulo 16.</summary>
    internal static readonly RandomSampleField ShardJitter = new(0, 4);

    /// <summary>One of the two bits a falling shard's sparkle check reads, bit five; the shard sparkles when both are clear.</summary>
    internal static readonly RandomSampleField SparkleCheckLowBit = new(5, 1);

    /// <summary>The other bit a falling shard's sparkle check reads, bit ten; the shard sparkles when both are clear.</summary>
    internal static readonly RandomSampleField SparkleCheckHighBit = new(10, 1);

    /// <summary>A sparkle's offset from its shard on each axis, 0 through 31 pixels less sixteen: the sample modulo 32.</summary>
    internal static readonly RandomSampleField SparkleOffset = new(0, 5);

    /// <summary>
    /// Graphics installed by the shard and sparkle initializers: from the sheet of her tank glass's pieces, in
    /// <paramref name="shading"/>, the encounter's sprite colours, which hold the glass shards' colours her room's entry loads.
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

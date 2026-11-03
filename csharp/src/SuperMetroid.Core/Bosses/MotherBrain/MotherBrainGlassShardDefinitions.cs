using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The graphics and physical origins of Mother Brain's glass shards.</summary>
internal static class MotherBrainGlassShardDefinitions
{
    /// <summary>
    /// Graphics installed by the shard and sparkle initializers: from Mother Brain's object characters, in
    /// <paramref name="shading"/>, the encounter's sprite colours, which hold the glass shards' colours her room's entry loads.
    /// </summary>
    internal static SpriteGraphics Graphics(PictureShading shading) => new(EnemyTileBaseDefinitions.MotherBrainObjectCharacters, shading);

    /// <summary>
    /// The three horizontal emitter positions on the glass: right, half a block right of the PLM origin;
    /// left, two and a half blocks left; centre, one block left.
    /// </summary>
    internal static readonly Dictionary<MotherBrainGlassShard, int> EmitterXs = new()
    {
        [MotherBrainGlassShard.Right] = 8,
        [MotherBrainGlassShard.Left] = -40,
        [MotherBrainGlassShard.Centre] = -16,
    };

    /// <summary>Every emitter sits two blocks below the PLM origin.</summary>
    internal const int EmitterY = 2 * 16;
}

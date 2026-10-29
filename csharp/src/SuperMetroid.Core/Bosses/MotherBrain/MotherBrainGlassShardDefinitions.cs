using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The graphics and physical origins of Mother Brain's glass shards.</summary>
internal static class MotherBrainGlassShardDefinitions
{
    /// <summary>Graphics installed by the shard and sparkle initializers: from Mother Brain's object characters, in the glass shards' colours.</summary>
    internal static SpriteGraphics Graphics { get; } = new(EnemyTileBaseDefinitions.MotherBrainObjectCharacters, new EncounterSpriteColouring(MotherBrainColourDefinitions.GlassShards));

    /// <summary>
    /// The three horizontal emitter positions on the glass: $86:CE61 right, half a block right of the PLM origin;
    /// $86:CE63 left, two and a half blocks left; $86:CE65 centre, one block left.
    /// </summary>
    internal static readonly Dictionary<MotherBrainGlassShard, int> EmitterXs = new()
    {
        [MotherBrainGlassShard.Right] = 8,
        [MotherBrainGlassShard.Left] = -40,
        [MotherBrainGlassShard.Centre] = -16,
    };

    /// <summary>Every emitter sits two blocks below the PLM origin.</summary>
    internal const short EmitterY = 2 * 16;
}

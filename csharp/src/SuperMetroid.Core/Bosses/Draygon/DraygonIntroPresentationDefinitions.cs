using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>What Draygon's opening shows the Evirs she releases with ($A5:871B).</summary>
internal static class DraygonIntroPresentationDefinitions
{
    /// <summary>The Evir tiles the opening shows.</summary>
    internal static SpriteArtworkSource EvirTiles { get; } = new EnemyTileCharacters(EnemyDefinitionId.Evir);
}

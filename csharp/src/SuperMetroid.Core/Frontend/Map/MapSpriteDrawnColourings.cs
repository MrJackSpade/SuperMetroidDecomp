using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>The colourings a map sprite is drawn in (#1513, #2556): its standard colouring and any others.</summary>
/// <param name="Standard">The colouring the map sprite sheet pictures the sprite in.</param>
/// <param name="Others">The other colourings it is drawn in, each through its drawn shade.</param>
internal sealed record MapSpriteDrawnColourings(MapSpriteColouring Standard, params IReadOnlyList<MapSpriteColouring> Others)
{
    /// <summary>Every colouring the sprite is drawn in, its standard one among them.</summary>
    internal IReadOnlyList<MapSpriteColouring> All { get; } = [Standard, .. Others];
}

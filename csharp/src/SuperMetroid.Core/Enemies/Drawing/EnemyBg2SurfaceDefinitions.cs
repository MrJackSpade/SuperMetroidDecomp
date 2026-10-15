namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>The enemy BG2 staging surface at <c>$7E:2000</c>, which large bosses draw into before NMI uploads it.</summary>
internal static class EnemyBg2SurfaceDefinitions
{
    /// <summary>The surface holds $800 words: a 64x32 BG2 map.</summary>
    internal const int WordCount = 2048;

    /// <summary>Tile $338 with plain attributes: the blank glyph bosses fill the surface with.</summary>
    internal static SnesBgTilemapWord BlankTile { get; } = SnesBgTilemapWord.Create(824, 0, priority: false);
}

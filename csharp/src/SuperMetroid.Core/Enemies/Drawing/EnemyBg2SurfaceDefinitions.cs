namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>The enemy BG2 staging surface at <c>$7E:2000</c>, which large bosses draw into before NMI uploads it.</summary>
internal static class EnemyBg2SurfaceDefinitions
{
    /// <summary>Tile $338 with plain attributes: the blank glyph bosses fill the surface with.</summary>
    internal static SnesBgTilemapWord BlankTile { get; } = SnesBgTilemapWord.Create(824, 0, priority: false);
}

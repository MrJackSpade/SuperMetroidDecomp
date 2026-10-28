using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>The enemy BG2 staging surface at <c>$7E:2000</c>, which large bosses draw into before NMI uploads it.</summary>
internal static class EnemyBg2SurfaceDefinitions
{
    /// <summary>The blank glyph bosses fill the surface with: tile 824 under colour variant zero, unmirrored, low priority.</summary>
    internal static BackgroundTile BlankTile { get; } = new(824, 0, highPriority: false);
}

using SuperMetroid.Core.Enemies.Drawing;
namespace SuperMetroid.Core.Enemies;

/// <summary>The colour set a frozen enemy draws with: the beam's that froze it (#1405).</summary>
internal static class EnemyFrozenPaletteDefinitions
{
    /// <summary>The frozen colour set's palette bits.</summary>
    internal static ushort PaletteBits => SnesObjPalettes.Palette(SpriteColourSetDefinitions.Frozen).PaletteBits;
}

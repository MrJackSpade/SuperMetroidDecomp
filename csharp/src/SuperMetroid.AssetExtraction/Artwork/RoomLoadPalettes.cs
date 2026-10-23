using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.World;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>The colours a room load leaves before any room content loads its own (#1392), which sheets compose over.</summary>
internal static class RoomLoadPalettes
{
    /// <summary>CGRAM as a room load leaves it: the starting image with the common and enemy-projectile sprite rows.</summary>
    internal static ScreenPalette Colours(GameplayBasePaletteCatalog palettes)
    {
        Ensure.NotNull(palettes);
        var cgram = new ScreenPalette();
        palettes.LoadInitial(cgram);
        palettes.LoadCommonSprites(cgram.SpriteRow(SpriteColourSetDefinitions.Common));
        palettes.LoadEnemyProjectileSprites(cgram.SpriteRow(SpriteColourSetDefinitions.EnemyProjectiles));
        return cgram;
    }
}

using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>The colours a room load leaves before any room content loads its own (#1392), which sheets compose over.</summary>
internal static class RoomLoadPalettes
{
    /// <summary>CGRAM as a room load leaves it: the starting image with the common and enemy-projectile sprite rows.</summary>
    internal static ScreenPalette Colours(GameplayBasePalette palettes)
    {
        Ensure.NotNull(palettes);
        var cgram = new ScreenPalette();
        palettes.LoadInitial(cgram.BackgroundSets, cgram.SpriteSets);
        palettes.LoadCommonSprites(cgram.SpriteSets.Set(SpriteColourSetDefinitions.Common));
        palettes.LoadEnemyProjectileSprites(cgram.SpriteSets.Set(SpriteColourSetDefinitions.EnemyProjectiles));
        return cgram;
    }
}

using SuperMetroid.Core.Rendering;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Video;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>The colours a room load leaves before any room content loads its own (#1392), which sheets compose over.</summary>
internal static class RoomLoadPalettes
{
    /// <summary>CGRAM as a room load leaves it: the starting image with the common and enemy-projectile sprite rows.</summary>
    internal static ScreenPalette Colours(GameplayBasePalette palettes)
    {
        Ensure.NotNull(palettes);
        var cgram = new ScreenPalette();
        BackgroundColourSetImage.LoadStarting(palettes, cgram);
        palettes.LoadCommonSprites(SpriteColourStore.Common.In(cgram.SpriteSets));
        palettes.LoadEnemyProjectileSprites(SpriteColourStore.EnemyProjectiles.In(cgram.SpriteSets));
        return cgram;
    }

    /// <summary>
    /// The eight sprite rows' colours, sixteen each, as a room load leaves them before any room content loads its own
    /// (#1512): the starting colours with the common and enemy-projectile rows.
    /// </summary>
    internal static RgbColor[] SpriteColours(GameplayBasePalette palettes) => Colours(palettes).SpriteSets.All.Colors.ToArray();
}

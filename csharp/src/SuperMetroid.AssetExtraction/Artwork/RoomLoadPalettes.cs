using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>The sprite colours a room load leaves before any room content loads its own (#1392), which sheets compose over.</summary>
internal static class RoomLoadPalettes
{
    /// <summary>The colours of one sprite colour group.</summary>
    private const int GroupColours = 16;

    /// <summary>The sprite colour groups a sheet's inks fall in.</summary>
    private const int Groups = 8;

    /// <summary>The group the common sprites' colours lie in, as the cartridge's room load places them.</summary>
    private const int CommonGroup = 0;

    /// <summary>The group the enemy projectiles' colours lie in, as the cartridge's room load places them.</summary>
    private const int EnemyProjectileGroup = 5;

    /// <summary>
    /// The sprite colours as a room load leaves them (#1512): the starting sprite colours, group after group, with the
    /// common sprites' and the enemy projectiles' groups loaded over them.
    /// </summary>
    internal static RgbColor[] SpriteColours(GameplayBasePalette palettes)
    {
        Ensure.NotNull(palettes);
        var colours = new RgbColor[Groups * GroupColours];
        for (int group = 0; group < Groups; group++)
            palettes.starting.Sprites.Set(group).CopyTo(colours.AsSpan(group * GroupColours, GroupColours));
        palettes.commonSprites.CopyTo(colours.AsSpan(CommonGroup * GroupColours, GroupColours));
        palettes.starting.EnemyProjectiles.CopyTo(colours.AsSpan(EnemyProjectileGroup * GroupColours, GroupColours));
        return colours;
    }
}

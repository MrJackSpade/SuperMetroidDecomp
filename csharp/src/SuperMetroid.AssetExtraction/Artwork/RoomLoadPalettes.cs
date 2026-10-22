using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.World;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>The colours a room load leaves before any room content loads its own (#1392), which sheets compose over.</summary>
internal static class RoomLoadPalettes
{
    /// <summary>CGRAM as a room load leaves it: the starting image with the common and enemy-projectile sprite rows.</summary>
    internal static SnesCgram Cgram(GameplayBasePaletteCatalog palettes)
    {
        Ensure.NotNull(palettes);
        var cgram = new SnesCgram();
        palettes.LoadInitial(cgram);
        palettes.LoadCommonSprites(cgram, RoomLoadingRomData.CommonGameplaySpritePaletteCgramIndex);
        palettes.LoadEnemyProjectileSprites(cgram, RoomLoadingRomData.InitialEnemyProjectilePaletteCgramIndex);
        return cgram;
    }
}

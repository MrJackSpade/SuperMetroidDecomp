using System.Collections.Frozen;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>
/// The shades the import pre-colours the Ceres cinematics' sprite sheets in (#1512): each colouring a Ceres actor draws
/// in. The gunship engine's flicker is a colour-effect shade (<see cref="Fx.ColourEffects.ColourEffectShades"/>). Which inks each
/// recolours, and to what, is the import's.
/// </summary>
public static partial class CeresShades
{
    /// <summary>
    /// The installed name of each colouring's shade (#2511), written out so renaming a colouring's identity never changes a
    /// name on disk.
    /// </summary>
    private static readonly FrozenDictionary<CeresSpriteColouring, string> DrawnKeys =
        new Dictionary<CeresSpriteColouring, string>
        {
            [CeresSpriteColouring.SpaceObjects] = "CeresSpaceObjects",
            [CeresSpriteColouring.Station] = "CeresStation",
            [CeresSpriteColouring.Zebes] = "CeresZebes",
            [CeresSpriteColouring.ZebesTitle] = "CeresZebesTitle",
            [CeresSpriteColouring.Explosions] = "CeresExplosions",
        }.ToFrozenDictionary();

    /// <summary>A Ceres actor's recolourable parts drawn in <paramref name="colouring"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The colouring has no installed name.</exception>
    public static PictureShade Drawn(CeresSpriteColouring colouring) => new(Ensure.Listed(DrawnKeys, colouring), 0);
}

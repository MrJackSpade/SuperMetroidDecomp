using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>
/// The shades the import pre-colours the Ceres cinematics' sprite sheets in (#1512): each colouring a Ceres actor draws
/// in. The gunship engine's flicker is a palette-effect shade (<see cref="Fx.ColourEffects.ColourEffectShades"/>). Which inks each
/// recolours, and to what, is the import's.
/// </summary>
public static class CeresShades
{
    /// <summary>A Ceres actor's palette-inheriting parts drawn in <paramref name="colouring"/>.</summary>
    public static PictureShade Drawn(CeresSpriteColouring colouring) => new($"Ceres{colouring}", 0);
}

using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>
/// Draws one cinematic family's frames from its screen's sprite sheet (#1392): each pose's frame, its recolourable
/// parts in the colouring a draw names, under the shading its screen gives that colouring (#1512), placed at the origin
/// wherever it lies (#1587).
/// </summary>
/// <param name="sheet">The screen's sprite sheet.</param>
/// <param name="family">The family whose frames are drawn.</param>
/// <param name="shading">The shading the screen shows a draw named in a colouring's drawn shade in: that shade with the screen's fades.</param>
internal sealed partial class CinematicSpriteSheet(SpriteSheet sheet, CinematicSpriteFamily family, Func<PictureShade, PictureShading> shading)
    : IIntroCinematicSpritePresentation
{

    /// <summary>The sheet's frame for <paramref name="family"/>'s composition of <paramref name="pose"/>.</summary>
    [InstalledKey]
    internal static string FrameName(CinematicSpriteFamily family, CinematicSpritePose pose) =>
        $"{CinematicSpriteFamilyKeys.Installed.Of(family)}.{CinematicSpritePoseKeys.Installed.Of(pose)}";

    /// <summary>Draws <paramref name="pose"/>'s frame at <paramref name="place"/> in its drawn shade's shading (#1512).</summary>
    public void Draw(CinematicSpritePose pose, ScreenSprites screenSprites, Offset16<ScreenPixels> place, PictureShade drawn) =>
        Draw(pose, screenSprites, place, Ensure.NotNull(shading)(drawn));

    /// <summary>Draws <paramref name="pose"/>'s frame in <paramref name="shown"/>, a shading its screen composed itself (#1512).</summary>
    public void Draw(CinematicSpritePose pose, ScreenSprites screenSprites, Offset16<ScreenPixels> place, PictureShading shown) =>
        Ensure.NotNull(screenSprites).AddImage(Ensure.NotNull(sheet).Frame(FrameName(family, pose)), place, shading: Ensure.NotNull(shown));
}

using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>
/// Draws one cinematic family's frames from its screen's sprite sheet (#1392): each pose's frame, its palette-inheriting
/// parts in the colouring a draw names, under the shading its screen gives that colouring (#1512), read from the origin
/// as the chosen spritemap loader reads it.
/// </summary>
/// <param name="sheet">The screen's sprite sheet.</param>
/// <param name="family">The family whose frames are drawn.</param>
/// <param name="shading">The shading the screen shows a draw named in a colouring's drawn shade in: that shade with the screen's fades.</param>
internal sealed class CinematicSpriteSheet(SpriteSheet sheet, CinematicSpriteFamily family, Func<PictureShade, PictureShading> shading)
    : IIntroCinematicSpritePresentation
{
    /// <summary>A family's frames shown in the drawn shade of their colouring alone.</summary>
    /// <param name="sheet">The screen's sprite sheet.</param>
    /// <param name="family">The family whose frames are drawn.</param>
    internal CinematicSpriteSheet(SpriteSheet sheet, CinematicSpriteFamily family)
        : this(sheet, family, PictureShading.Shaded)
    {
    }

    /// <summary>The sheet's frame for <paramref name="family"/>'s composition of <paramref name="pose"/>.</summary>
    internal static string FrameName(CinematicSpriteFamily family, CinematicSpritePose pose) => $"{family}.{pose.Index}";

    /// <inheritdoc/>
    public void Draw(CinematicSpritePose pose, ScreenSprites objects, Position16<SpritePixels> origin, PictureShade drawn, bool originIsOnScreen) =>
        Draw(pose, objects, origin, Ensure.NotNull(shading)(drawn), originIsOnScreen);

    /// <summary>Draws <paramref name="pose"/>'s frame through the on-screen loader, its parts in the drawn shade <paramref name="drawn"/>.</summary>
    public void Draw(CinematicSpritePose pose, ScreenSprites objects, Position16<SpritePixels> origin, PictureShade drawn) =>
        Draw(pose, objects, origin, drawn, originIsOnScreen: true);

    /// <summary>Draws <paramref name="pose"/>'s frame in <paramref name="shown"/>, a shading its screen composed itself (#1512).</summary>
    public void Draw(CinematicSpritePose pose, ScreenSprites objects, Position16<SpritePixels> origin, PictureShading shown, bool originIsOnScreen) =>
        Ensure.NotNull(objects).AddImage(Ensure.NotNull(sheet).Frame(FrameName(family, pose)), origin,
            side: originIsOnScreen ? SpriteOrigin.OnScreen : SpriteOrigin.OffScreen, shading: Ensure.NotNull(shown));
}

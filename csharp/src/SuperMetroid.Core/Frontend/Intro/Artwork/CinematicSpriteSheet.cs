using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>
/// Draws one cinematic family's frames from its screen's sprite sheet (#1392): each pose's frame, its palette-inheriting
/// parts in the row a draw's palette selects, read from the origin as the chosen spritemap loader reads it.
/// </summary>
/// <param name="sheet">The screen's sprite sheet.</param>
/// <param name="family">The family whose frames are drawn.</param>
internal sealed class CinematicSpriteSheet(SpriteSheet sheet, CinematicSpriteFamily family) : IIntroCinematicSpritePresentation
{
    /// <summary>The sheet's frame for <paramref name="family"/>'s composition of <paramref name="pose"/>.</summary>
    internal static string FrameName(CinematicSpriteFamily family, CinematicSpritePose pose) => $"{family}.{pose.Index}";

    /// <inheritdoc/>
    public void Draw(CinematicSpritePose pose, ScreenSprites objects, Position16<SpritePixels> origin, int palette, bool originIsOnScreen) =>
        Ensure.NotNull(objects).AddImage(Ensure.NotNull(sheet).Frame(FrameName(family, pose)), origin,
            FrontendSheetPalette.Override(palette), originIsOnScreen ? SpriteOrigin.OnScreen : SpriteOrigin.OffScreen);

    /// <summary>Draws <paramref name="pose"/>'s frame through the on-screen loader.</summary>
    public void Draw(CinematicSpritePose pose, ScreenSprites objects, Position16<SpritePixels> origin, int palette) =>
        Draw(pose, objects, origin, palette, originIsOnScreen: true);
}

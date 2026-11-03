using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>
/// Visual-only frame lookup for cinematic sprite actors. The actor still owns
/// its animation, timing, position and clipping branch; the presentation supplies only sprite parts.
/// </summary>
public interface IIntroCinematicSpritePresentation
{
    /// <summary>Appends the selected cinematic composition in authored drawing order without advancing the actor's animation, timers, or motion.</summary>
    /// <param name="pose">The cinematic pose whose composition to draw.</param>
    /// <param name="objects">This frame's destination sprite entries.</param>
    /// <param name="origin">Wrapped sixteen-bit screen origin in pixels.</param>
    /// <param name="drawn">The drawn shade of the colouring the palette-inheriting parts draw in (#1512), which the screen shows with its own fades.</param>
    /// <param name="originIsOnScreen">Selects the caller's ordinary origin-clipping path when true, or the off-screen Y-wrap path when false.</param>
    void Draw(CinematicSpritePose pose, ScreenSprites objects, Position16<SpritePixels> origin,
        PictureShade drawn, bool originIsOnScreen);
}

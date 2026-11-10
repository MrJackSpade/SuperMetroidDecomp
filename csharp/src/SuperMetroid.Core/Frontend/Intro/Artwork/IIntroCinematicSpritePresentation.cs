using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>
/// Visual-only frame lookup for cinematic sprite actors. The actor still owns its animation, timing and position; the
/// presentation supplies only the frame, published wherever its origin lies for the display to clip (#1587).
/// </summary>
public interface IIntroCinematicSpritePresentation
{
    /// <summary>Appends the selected cinematic composition in authored drawing order without advancing the actor's animation, timers, or motion.</summary>
    /// <param name="pose">The cinematic pose whose composition to draw.</param>
    /// <param name="objects">This frame's destination sprite entries.</param>
    /// <param name="origin">Screen origin in pixels, each coordinate read as signed.</param>
    /// <param name="drawn">The drawn shade of the colouring the palette-inheriting parts draw in (#1512), which the screen shows with its own fades.</param>
    void Draw(CinematicSpritePose pose, ScreenSprites objects, Position16<SpritePixels> origin, PictureShade drawn);
}

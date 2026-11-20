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
    /// <param name="screenSprites">This frame's destination sprites.</param>
    /// <param name="place">Where on the screen the composition's origin lies; left of or above the screen is negative (#1974).</param>
    /// <param name="drawn">The drawn shade of the colouring the recolourable parts draw in (#1512), which the screen shows with its own fades.</param>
    void Draw(CinematicSpritePose pose, ScreenSprites screenSprites, Offset16<ScreenPixels> place, PictureShade drawn);
}

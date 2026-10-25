using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>
/// Visual-only frame lookup for cartridge cinematic sprite actors. The actor still owns
/// its list, timing, position and clipping branch; the presentation supplies only OAM parts.
/// </summary>
public interface IIntroCinematicSpritePresentation
{
    /// <summary>Appends the selected cinematic composition in authored OAM order without advancing the actor's instruction list, timers, or motion.</summary>
    /// <param name="pose">The cinematic pose whose composition to draw.</param>
    /// <param name="objects">This frame's destination sprite entries.</param>
    /// <param name="origin">Native wrapped sixteen-bit screen origin in pixels.</param>
    /// <param name="palette">The sprite palette row inherited by parts that do not specify their own palette.</param>
    /// <param name="originIsOnScreen">Selects the caller's ordinary origin-clipping path when true, or native off-screen Y-wrap path when false.</param>
    void Draw(CinematicSpritePose pose, ScreenSprites objects, Position16<SpritePixels> origin,
        int palette, bool originIsOnScreen);
}

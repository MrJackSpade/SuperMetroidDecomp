using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Drawing the cinematic actors whose family shows pictures (#1392, #2197).</summary>
internal static class CinematicPoseActors
{
    /// <summary>
    /// Draws <paramref name="actor"/>'s current picture from <paramref name="installedArt"/> at its screen place minus
    /// <paramref name="scroll"/>, wherever that lies: the display clips a frame off the screen (#1587). The blank pose draws
    /// nothing.
    /// </summary>
    /// <exception cref="InvalidOperationException">The actor shows a picture and no installed artwork is given.</exception>
    internal static void Draw<TAnimation, TCue>(this CinematicActor<TAnimation, CinematicSpritePose, TCue> actor, ScreenSprites screenSprites,
        IIntroCinematicSpritePresentation installedArt, Offset16<ScreenPixels> scroll = default)
        where TAnimation : struct, Enum
        where TCue : notnull
    {
        Ensure.NotNull(installedArt);
        if (Ensure.NotNull(actor).Drawing(scroll) is not { } drawing || drawing.Shown == CinematicSpritePose.Blank)
            return;
        installedArt.Draw(drawing.Shown, screenSprites, drawing.Place, drawing.Colouring);
    }
}

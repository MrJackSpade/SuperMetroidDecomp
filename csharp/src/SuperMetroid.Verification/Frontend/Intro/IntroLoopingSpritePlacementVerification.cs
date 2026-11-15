using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>
/// Checks where a cinematic actor travelling a horizontal loop is drawn (#1965): the half of the loop before its middle
/// lies on the screen at its place along the loop, and the half past its middle lies left of the screen, at a negative
/// screen X, so the actor comes round into view again from the left.
/// </summary>
internal static class IntroLoopingSpritePlacementVerification
{
    /// <summary>
    /// Draws a Ceres asteroid on the 512-pixel loop at X 200 and at X 400 and checks the drawn origins: 200 on the screen,
    /// and 400 past the middle at -112, the loop's width to its left.
    /// </summary>
    internal static void VerifyLoopingSpritePlacement()
    {
        int loop = CeresDestructionSceneDefinitions.WorldWidth;
        AssertEqual(512, loop, "the Ceres scenes' horizontal loop is 512 pixels wide");
        AssertEqual(200, DrawnX(200, loop), "an actor before the loop's middle is drawn at its place along the loop");
        AssertEqual(400 - loop, DrawnX(400, loop), "an actor past the loop's middle is drawn left of the screen, at a negative X");
        AssertEqual(30, DrawnY(400, loop), "the loop leaves the actor's height alone");
        AssertEqual(-112, DrawnX(400 + loop, loop), "an actor a whole loop further on is drawn where it was");
    }

    /// <summary>The screen X the asteroid is drawn at from loop place <paramref name="x"/>.</summary>
    private static int DrawnX(int x, int loop) => Drawn(x, loop).X;

    /// <summary>The screen Y the asteroid is drawn at from loop place <paramref name="x"/>.</summary>
    private static int DrawnY(int x, int loop) => Drawn(x, loop).Y;

    /// <summary>The screen place the asteroid at loop place <paramref name="x"/>, 30 pixels down, is drawn at (#1974).</summary>
    private static Offset16<ScreenPixels> Drawn(int x, int loop)
    {
        var actor = new IntroDiscoverySprite(new(x, 30), IntroShades.Drawn(IntroSpriteColouring.DiscoveryActors), CinematicSpriteSegment.CeresSmallAsteroid)
        {
            HorizontalLoopWidth = loop,
        };
        actor.Step();
        var recorder = new OriginRecorder();
        actor.Draw(new ScreenSprites(), installedArt: recorder);
        return recorder.Origin ?? throw new InvalidOperationException("The looping asteroid drew no frame.");
    }

    /// <summary>A presentation that records the origin it is asked to draw at.</summary>
    private sealed class OriginRecorder : IIntroCinematicSpritePresentation
    {
        /// <summary>The screen place of the last draw.</summary>
        internal Offset16<ScreenPixels>? Origin { get; private set; }

        /// <inheritdoc/>
        public void Draw(CinematicSpritePose pose, ScreenSprites objects, Offset16<ScreenPixels> place, PictureShade drawn) => Origin = place;
    }
}

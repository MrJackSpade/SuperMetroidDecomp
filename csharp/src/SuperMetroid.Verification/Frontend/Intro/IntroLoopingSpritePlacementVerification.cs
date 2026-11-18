using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>
/// Checks where a cinematic actor drifting round the Ceres scenes' drift loop is drawn (#1965, #2418): on the loop's
/// first half, the screen, it is drawn at its place along the loop, and on its second half, the stretch of space left of
/// the screen, at a negative screen X, so the actor comes round into view again from the left.
/// </summary>
internal static class IntroLoopingSpritePlacementVerification
{
    /// <summary>
    /// Draws a Ceres asteroid on the 512-pixel drift loop at X 200 and at X 400 and checks the drawn origins: 200 on the
    /// screen, and 400, on the stretch left of the screen, at -112, the loop's width to its left.
    /// </summary>
    internal static void VerifyLoopingSpritePlacement()
    {
        int loop = CeresDestructionSceneDefinitions.DriftLoopWidth;
        AssertEqual(512, loop, "the Ceres scenes' drift loop is 512 pixels wide");
        AssertEqual(200, DrawnX(200, loop), "an actor on the screen's half of the loop is drawn at its place along the loop");
        AssertEqual(400 - loop, DrawnX(400, loop), "an actor on the loop's stretch left of the screen is drawn there, at a negative X");
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
        var actor = new CinematicActor<CeresAnimation, CinematicSpritePose, CeresCue>(new(x, 30), IntroShades.Drawn(IntroSpriteColouring.DiscoveryActors), CeresAnimations.Scenes, CeresAnimation.SmallAsteroid)
        {
            DriftLoopWidth = loop,
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
        public void Draw(CinematicSpritePose pose, ScreenSprites screenSprites, Offset16<ScreenPixels> place, PictureShade drawn) => Origin = place;
    }
}

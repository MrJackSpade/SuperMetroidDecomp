using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Runtime;

/// <summary>Checks which of Samus's colours a door's fade keeps lit (#2093).</summary>
internal static class DoorFadeSamusLitVerification
{
    /// <summary>
    /// Confirms a door's fade keeps nothing of Samus's lit while the room being left fades out and stays dark, her beams'
    /// shade through the room's fade in, and her visor's door-completion green after it once the door has opened; the
    /// next door's fade out keeps nothing lit again.
    /// </summary>
    internal static void VerifyDoorFadeKeepsSamusLit()
    {
        var colours = new SamusColours();
        colours.ShowBeams(SamusShades.Beam(SamusBeamCombination.IceWave));
        var counter = new SharedFadeStepCount();
        var fade = new DoorFade(colours);

        fade.FadeOut(new GradualFadeSteps(DoorTransitionColourDefinitions.FadeDenominator, counter), commonRooms: false);
        AssertEqual(0, LitOnSamus(fade).Length, "the room being left fades out with nothing of Samus's kept lit");
        fade.Darken();
        AssertEqual(0, LitOnSamus(fade).Length, "the dark display keeps nothing of Samus's lit");

        fade.FadeIn(new GradualFadeSteps(DoorTransitionColourDefinitions.FadeDenominator, counter));
        AssertTrue(LitOnSamus(fade).SequenceEqual([SamusShades.Beam(SamusBeamCombination.IceWave)]),
            "the room being entered fades in with Samus's beams lit");
        fade.DoorOpened();
        AssertTrue(LitOnSamus(fade).SequenceEqual([SamusShades.Beam(SamusBeamCombination.IceWave), SamusShades.DoorCompletionVisor]),
            "once the door has opened her visor's door-completion green stays lit after her beams");

        fade.FadeOut(new GradualFadeSteps(DoorTransitionColourDefinitions.FadeDenominator, counter), commonRooms: false);
        AssertEqual(0, LitOnSamus(fade).Length, "the next door's fade out keeps nothing of Samus's lit");
        Console.WriteLine("  Door fade: Samus's beams stay lit through the fade in and her visor once the door has opened.");
    }

    /// <summary>The shades the door's fade keeps lit on Samus's draws this frame, after the fade.</summary>
    private static ImmutableArray<PictureShade> LitOnSamus(DoorFade fade)
    {
        var frame = new GameplayShadingBuilder();
        fade.ShadeGameplay(frame);
        var roomColours = new PictureColours { Backdrop = ColourKeyConstants.Of(RgbColor.Black), Shades = ImmutableDictionary<PictureShade, PictureRecolouring>.Empty };
        return frame.Build(SpriteImageFixtures.Copied("Room", new(1, 1), [default(RgbaPixel)], roomColours))
            .OfSprite(PictureShading.Standard, isSamus: true).OverEffects;
    }
}

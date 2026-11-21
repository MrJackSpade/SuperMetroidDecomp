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

        fade.FadeOut(new GradualFadeSteps(DoorTransitionColourDefinitions.FadeLastStep, counter), commonRooms: false);
        AssertEqual(0, LitOnSamus(fade).Length, "the room being left fades out with nothing of Samus's kept lit");
        fade.Darken();
        AssertEqual(0, LitOnSamus(fade).Length, "the dark display keeps nothing of Samus's lit");

        var entering = new GradualFadeSteps(DoorTransitionColourDefinitions.FadeLastStep, counter);
        fade.FadeIn(entering);
        AssertTrue(LitOnSamus(fade).SequenceEqual([SamusShades.Beam(SamusBeamCombination.IceWave)]),
            "the room being entered fades in with Samus's beams lit");
        fade.DoorOpened(entering);
        AssertTrue(LitOnSamus(fade).SequenceEqual([SamusShades.Beam(SamusBeamCombination.IceWave), SamusShades.DoorCompletionVisor]),
            "once the door has opened her visor's door-completion green stays lit after her beams");

        fade.FadeOut(new GradualFadeSteps(DoorTransitionColourDefinitions.FadeLastStep, counter), commonRooms: false);
        AssertEqual(0, LitOnSamus(fade).Length, "the next door's fade out keeps nothing of Samus's lit");
        Console.WriteLine("  Door fade: Samus's beams stay lit through the fade in and her visor once the door has opened.");
    }

    /// <summary>
    /// Confirms a door's fades are typed fades (#2289): the room being left fades the display toward black and the HUD and
    /// the escape timer toward their dark shades by the same progress, each of the shared counter's values from zero's step
    /// on one of thirteen parts, the room stays dark at the dark shades, and the room being entered fades back from them;
    /// each fade still completes on the call after the step of counter value thirteen, its fifteenth.
    /// </summary>
    internal static void VerifyDoorFadeIsTypedFade()
    {
        var counter = new SharedFadeStepCount();
        var fade = new DoorFade(new SamusColours());
        var leaving = new GradualFadeSteps(DoorTransitionColourDefinitions.FadeLastStep, counter);
        fade.FadeOut(leaving, commonRooms: false);
        for (int call = 0; call < 7; call++)
            AssertTrue(!leaving.Step(), "the fade out takes a step on each of its first seven calls");
        GameplayShading shown = Shown(fade);
        AssertEqual<ColourEffect?>(new ColourFade(RgbColor.Black, GradualFadeDirection.Toward, 6, 13), shown.Fade?.Effect,
            "after the steps of counter values zero through six the display has faded six of thirteen parts toward black");
        AssertTrue(shown.Of(GameplayPictures.Hud).Fades.SequenceEqual(
                [new PictureShadeFade(PictureShading.Standard, PictureShading.Shaded(DoorFadeShades.HudDark), 6, 13)]),
            "the HUD fades as far toward its dark shade");
        AssertTrue(shown.Of(GameplayPictures.EscapeTimer).Fades.SequenceEqual(
                [new PictureShadeFade(PictureShading.Standard, PictureShading.Shaded(DoorFadeShades.EscapeTimerDark), 6, 13)]),
            "the escape timer fades as far toward its dark shade");
        int calls = 7;
        while (!leaving.Step())
            calls++;
        AssertEqual(15, calls + 1, "the fade out completes on its fifteenth call, the one after counter value thirteen's step");

        fade.Darken();
        AssertTrue(Shown(fade).Of(GameplayPictures.Hud).Shades.SequenceEqual([DoorFadeShades.HudDark]),
            "the dark display shows the HUD's dark shade");

        var entering = new GradualFadeSteps(DoorTransitionColourDefinitions.FadeLastStep, counter);
        fade.FadeIn(entering);
        AssertTrue(!entering.Step() && !entering.Step(), "the fade in takes a step on each of its first two calls");
        shown = Shown(fade);
        AssertEqual<ColourEffect?>(new ColourFade(RgbColor.Black, GradualFadeDirection.From, 1, 13), shown.Fade?.Effect,
            "after the steps of counter values zero and one the display has faded one of thirteen parts in from black");
        AssertTrue(shown.Of(GameplayPictures.Hud).Fades.SequenceEqual(
                [new PictureShadeFade(PictureShading.Shaded(DoorFadeShades.HudDark), PictureShading.Standard, 1, 13)]),
            "the HUD fades as far back from its dark shade");
        Console.WriteLine("  Door fade: the display, the HUD and the escape timer show one typed fade's progress; completion is unchanged.");
    }

    /// <summary>The frame's shading the door's fade shows.</summary>
    private static GameplayShading Shown(DoorFade fade)
    {
        var frame = new GameplayShadingBuilder();
        fade.ShadeGameplay(frame);
        var roomColours = new PictureColours { Backdrop = ColourKeyConstants.Of(RgbColor.Black), Shades = ImmutableDictionary<PictureShade, PictureRecolouring>.Empty };
        return frame.Build(SpriteImageFixtures.Copied("Room", new(1, 1), [default(RgbaPixel)], roomColours));
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

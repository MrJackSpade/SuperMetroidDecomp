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
    /// Confirms a door's fades are typed fades (#2289, #3036): the room being left fades the display toward black and every
    /// background picture and the escape timer toward their dark shades by the same progress, each of the shared counter's values from zero's step
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
        AssertEqual<ColourEffect?>(new ColourFade(RgbColor.Black, GradualFadeDirection.Toward, new FadeLevel(6, 13) { Arithmetic = FadeArithmetic.GradualColourChange }), shown.Fade?.Effect,
            "after the steps of counter values zero through six the display has faded six of thirteen parts toward black");
        AssertTrue(shown.Of(GameplayPictures.Hud).Fades.SequenceEqual(
                [new PictureShadeFade(PictureShading.Standard, PictureShading.Shaded(DoorFadeShades.BackgroundDark), new FadeLevel(6, 13) { Arithmetic = FadeArithmetic.GradualColourChange })]),
            "the HUD fades as far toward its dark shade");
        foreach (GameplayPictures pictures in GameplayPictureDefinitions.Backgrounds)
        {
            // Every background picture fades toward the dark shade the HUD does, which keeps the colours the cartridge's
            // fade target keeps lit wherever they show, and not with the display's fade (#3036).
            PictureShading background = shown.Of(pictures);
            AssertTrue(background.Effects.IsEmpty && background.Fades.SequenceEqual(
                    [new PictureShadeFade(PictureShading.Standard, PictureShading.Shaded(DoorFadeShades.BackgroundDark), new FadeLevel(6, 13) { Arithmetic = FadeArithmetic.GradualColourChange })]),
                $"the {pictures} pictures fade as far toward the background's dark shade, without the display's fade");
        }
        AssertTrue(shown.Of(GameplayPictures.EscapeTimer).Fades.SequenceEqual(
                [new PictureShadeFade(PictureShading.Standard, PictureShading.Shaded(DoorFadeShades.EscapeTimerDark), new FadeLevel(6, 13) { Arithmetic = FadeArithmetic.GradualColourChange })]),
            "the escape timer fades as far toward its dark shade");
        int calls = 7;
        while (!leaving.Step())
            calls++;
        AssertEqual(15, calls + 1, "the fade out completes on its fifteenth call, the one after counter value thirteen's step");

        fade.Darken();
        AssertTrue(Shown(fade).Of(GameplayPictures.Hud).Shades.SequenceEqual([DoorFadeShades.BackgroundDark]),
            "the dark display shows the HUD's dark shade");
        AssertTrue(Shown(fade).Of(GameplayPictures.Room).Shades.SequenceEqual([DoorFadeShades.BackgroundDark]),
            "the dark display shows the room's pictures in the background's dark shade, which keeps the doors' lit colours");

        var entering = new GradualFadeSteps(DoorTransitionColourDefinitions.FadeLastStep, counter);
        fade.FadeIn(entering);
        AssertTrue(!entering.Step() && !entering.Step(), "the fade in takes a step on each of its first two calls");
        shown = Shown(fade);
        AssertEqual<ColourEffect?>(new ColourFade(RgbColor.Black, GradualFadeDirection.From, new FadeLevel(1, 13) { Arithmetic = FadeArithmetic.GradualColourChange }), shown.Fade?.Effect,
            "after the steps of counter values zero and one the display has faded one of thirteen parts in from black");
        AssertTrue(shown.Of(GameplayPictures.Hud).Fades.SequenceEqual(
                [new PictureShadeFade(PictureShading.Shaded(DoorFadeShades.BackgroundDark), PictureShading.Standard, new FadeLevel(1, 13) { Arithmetic = FadeArithmetic.GradualColourChange })]),
            "the HUD fades as far back from its dark shade");
        Console.WriteLine("  Door fade: the display, every background picture and the escape timer show one typed fade's progress; completion is unchanged.");

        // The fades on the shared counter run the cartridge's gradual colour change (#3036): each call moves a channel by
        // what is left divided by the parts left, in eight-bit fractions it drops (CalculateTheAth_TransitionalColorComponent),
        // worked here by hand for a whole channel over the door's thirteen parts. Out, the drop of a fraction takes a step
        // more; in, it takes nothing, so the fade in starts slowly and catches up at its end.
        int[] fadingOut = [31, 28, 25, 22, 19, 16, 13, 10, 8, 6, 4, 2, 0, 0];
        int[] fadingIn = [0, 2, 4, 6, 8, 10, 13, 16, 19, 22, 25, 28, 31, 31];
        for (int call = 0; call <= DoorTransitionColourDefinitions.FadeLastStep; call++)
        {
            AssertEqual(fadingOut[call], SuperMetroid.Core.Compatibility.GradualColourChange.Channel(31, 0, call, DoorTransitionColourDefinitions.FadeLastStep),
                $"a channel fading out from full shows the cartridge's step after call {call}");
            AssertEqual(fadingIn[call], SuperMetroid.Core.Compatibility.GradualColourChange.Channel(0, 31, call, DoorTransitionColourDefinitions.FadeLastStep),
                $"a channel fading in to full shows the cartridge's step after call {call}");
        }
        Console.WriteLine("  Door fade: its colours move as the cartridge's gradual colour change moves them.");
    }

    /// <summary>The frame's shading the door's fade shows.</summary>
    private static GameplayShading Shown(DoorFade fade)
    {
        var frame = new GameplayShadingBuilder();
        fade.ShadeGameplay(frame);
        var roomColours = new PictureColours { Backdrop = ColourKeyConstants.Of(RgbColor.Black), Shades = ImmutableDictionary<PictureShade, PictureRecolouring>.Empty };
        return frame.BuildOverFixture(SpriteImageFixtures.Copied("Room", new(1, 1), [default(RgbaPixel)], roomColours));
    }

    /// <summary>The shades the door's fade keeps lit on Samus's draws this frame, after the fade.</summary>
    private static ImmutableArray<PictureShade> LitOnSamus(DoorFade fade)
    {
        var frame = new GameplayShadingBuilder();
        fade.ShadeGameplay(frame);
        var roomColours = new PictureColours { Backdrop = ColourKeyConstants.Of(RgbColor.Black), Shades = ImmutableDictionary<PictureShade, PictureRecolouring>.Empty };
        return frame.BuildOverFixture(SpriteImageFixtures.Copied("Room", new(1, 1), [default(RgbaPixel)], roomColours))
            .OfSprite(PictureShading.Standard, isSamus: true).OverEffects;
    }
}

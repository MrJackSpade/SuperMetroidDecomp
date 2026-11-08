using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>
/// Checks that each ending scene shows its own colours (#1544): a scene's whole screen of colours starts it, so nothing an
/// earlier scene showed shows in it, while the scene's own loads, fades and effects stay; cleared palette effects stop
/// showing their records.
/// </summary>
internal static class EndingSceneColoursVerification
{
    /// <summary>Runs the scene colour checks on the ending's colours alone, without an installation.</summary>
    internal static void VerifyEndingSceneColours()
    {
        var black = RgbColor.Black;
        var escape = new RgbColor(1, 2, 3);
        var explosion = new RgbColor(4, 5, 6);
        PictureShade gunshipRecord = PaletteFxShades.Of(PaletteFxColours.GunshipEmergingFromExplosion, new PaletteFxFrame(0));

        var colours = new EndingColours();
        colours.Load(EndingColourLoad.Escape, escape);
        colours.Fade(EndingFade.ExplosionBackground, 3);
        colours.PaletteFx(PaletteFxColours.GunshipEmergingFromExplosion, new PaletteFxFrame(0), null);
        colours.Flash(EndingFlash.SilhouetteBackdrop, black);
        colours.Load(EndingColourLoad.Explosion, explosion);
        AssertTrue(colours.Shading.Shades.SequenceEqual([EndingShades.Loaded(EndingColourLoad.Explosion)]),
            "a scene's whole screen of colours shows nothing an earlier scene showed");
        AssertEqual(explosion, colours.Backdrop, "a scene shows its own backdrop");

        colours.Fade(EndingFade.ExplodingPlanet, 5);
        colours.PaletteFx(PaletteFxColours.GunshipEmergingFromExplosion, new PaletteFxFrame(0), null);
        colours.Load(EndingColourLoad.ExplosionBackgrounds, explosion);
        AssertTrue(colours.Shading.Shades.SequenceEqual([EndingShades.Loaded(EndingColourLoad.Explosion), EndingShades.Faded(EndingFade.ExplodingPlanet, 5),
                gunshipRecord, EndingShades.Loaded(EndingColourLoad.ExplosionBackgrounds)]),
            "a load within a scene shows over the scene's own fades and effects");

        colours.EndPaletteFx();
        AssertTrue(colours.Shading.Shades.SequenceEqual([EndingShades.Loaded(EndingColourLoad.Explosion), EndingShades.Faded(EndingFade.ExplodingPlanet, 5),
                EndingShades.Loaded(EndingColourLoad.ExplosionBackgrounds)]),
            "cleared palette effects stop showing their records, and nothing else");

        colours.Load(EndingColourLoad.PostCredits, black);
        AssertTrue(colours.Shading.Shades.SequenceEqual([EndingShades.Loaded(EndingColourLoad.PostCredits)]),
            "the post-credits scenes keep nothing of the scenes before them");
        Console.WriteLine("  Ending scene colours: each scene shows its own colours, loads and effects within a scene, cleared effects end.");
    }

    /// <summary>Runs the ending scene colour checks.</summary>
    [VerificationCommand("--ending-scene-colours")]
    internal static int EndingSceneColoursCommand()
    {
        Suite(nameof(VerifyEndingSceneColours), VerifyEndingSceneColours);
        return 0;
    }
}

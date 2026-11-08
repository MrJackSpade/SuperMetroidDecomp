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
/// showing their records; each actor, background picture and floor shows only the effects authored for it.
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

        // Each actor shows only the colour effects authored for it (#1544): the animals' escape pod, which once shared the
        // planet's colour group, shows none of the planet's fade, glow or fade-out, and no actor shows the gunship's record.
        var actors = new EndingColours();
        actors.Load(EndingColourLoad.Explosion, explosion);
        actors.Fade(EndingFade.ExplodingPlanet, 5);
        actors.Flash(EndingFlash.PlanetGlow, null);
        PictureShade planetFadeOut = PaletteFxShades.Of(PaletteFxColours.ZoomedOutExplodingZebes, new PaletteFxFrame(0));
        actors.PaletteFx(PaletteFxColours.ZoomedOutExplodingZebes, new PaletteFxFrame(0), null);
        actors.PaletteFx(PaletteFxColours.GunshipEmergingFromExplosion, new PaletteFxFrame(0), null);
        PictureShade pod = EndingShades.Drawn(EndingSpriteColouring.AnimalEscape);
        PictureShade planet = EndingShades.Drawn(EndingSpriteColouring.ExplodingPlanet);
        PictureShade explosionColours = EndingShades.Loaded(EndingColourLoad.Explosion);
        AssertTrue(actors.Drawn(pod).Shades.SequenceEqual([pod, EndingShades.Under(pod, explosionColours)]),
            "the escape pod shows its scene's colours and none of the planet's effects");
        AssertTrue(actors.Drawn(planet).Shades.SequenceEqual([planet, EndingShades.Under(planet, explosionColours),
                EndingShades.Under(planet, EndingShades.Faded(EndingFade.ExplodingPlanet, 5)),
                EndingShades.Under(planet, EndingShades.Flashed(EndingFlash.PlanetGlow)), EndingShades.Under(planet, planetFadeOut)]),
            "the planet shows its fade, its glow's flash and its fade-out, in the order shown");
        // Each background picture and floor shows only the effects authored for it (#1544): the gunship's floor shows its
        // scene's colours and its emergence, none of the planet's effects, and the explosion's pictures not the gunship's.
        AssertTrue(actors.SceneryShading(EndingScenery.FlyawayFloor).Shades.SequenceEqual([explosionColours, gunshipRecord]),
            "the gunship's floor shows its scene's colours and its emergence, and none of the planet's effects");
        AssertTrue(actors.SceneryShading(EndingScenery.ExplosionPictures).Shades.SequenceEqual([explosionColours]),
            "the explosion's pictures show their scene's colours and not the gunship's emergence");

        // The post-credits shot's turning floor is drawn in the shot's colours, so it keeps the shot's fade and restore,
        // and none of Samus shooting's fade.
        var shot = new EndingColours();
        shot.Load(EndingColourLoad.PostCredits, black);
        shot.Fade(EndingFade.Shot, 7);
        shot.Fade(EndingFade.ShootingSamus, 3);
        AssertTrue(shot.SceneryShading(EndingScenery.PostCreditsShotFloor).Shades.SequenceEqual(
                [EndingShades.Loaded(EndingColourLoad.PostCredits), EndingShades.Faded(EndingFade.Shot, 7)]),
            "the post-credits shot's floor fades with the shot, not with Samus shooting");
        shot.Load(EndingColourLoad.RestoredShot, null);
        AssertEqual(EndingShades.Loaded(EndingColourLoad.RestoredShot), shot.SceneryShading(EndingScenery.PostCreditsShotFloor).Shades[^1],
            "the post-credits shot's floor shows the shot's restored colours");
        Console.WriteLine("  Ending scene colours: each scene shows its own colours, loads and effects within a scene, cleared effects end, "
            + "each actor, picture and floor shows only its own effects.");
    }

    /// <summary>Runs the ending scene colour checks.</summary>
    [VerificationCommand("--ending-scene-colours")]
    internal static int EndingSceneColoursCommand()
    {
        Suite(nameof(VerifyEndingSceneColours), VerifyEndingSceneColours);
        return 0;
    }
}

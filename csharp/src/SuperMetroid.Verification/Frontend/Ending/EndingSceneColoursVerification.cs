using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>
/// Checks that each ending scene shows its own colours, derived from its current colour state (#1544, #1597): a scene's
/// whole screen of colours starts it, so nothing an earlier scene showed shows in it, while the scene's own loads, fades,
/// flashes and effects stay; a load within a scene ends the effects whose colours it restores; cleared palette effects
/// stop showing their records; each actor, background picture and floor shows only the effects authored for it.
/// </summary>
internal static class EndingSceneColoursVerification
{
    /// <summary>The conditions a frontend screen's colour effects run under.</summary>
    private static readonly PaletteFxConditions Screen = new() { SamusY = 0, EquippedItems = 0, AreaMiniBossDefeated = false };

    /// <summary>Runs the scene colour checks on the ending's colours alone, without an installation.</summary>
    internal static void VerifyEndingSceneColours()
    {
        var black = RgbColor.Black;
        var escape = new RgbColor(8, 16, 24);
        var explosion = new RgbColor(33, 41, 49);

        var colours = new EndingColours();
        colours.Load(EndingColourLoad.Escape, escape);
        colours.Fade(EndingFade.ExplosionBackground, 3);
        colours.Flash(EndingFlash.SilhouetteBackdrop, black);
        colours.PaletteFx.Start(EndingPaletteFxDefinitions.GunshipEmergence, 0);
        colours.PaletteFx.StepScreen(Screen);
        colours.Load(EndingColourLoad.Explosion, explosion);
        colours.ResetPaletteFx();
        AssertTrue(colours.Shades.SequenceEqual([EndingShades.Loaded(EndingColourLoad.Explosion)]),
            "a scene's whole screen of colours shows nothing an earlier scene showed");
        AssertEqual(explosion, colours.Backdrop(null), "a scene shows its own backdrop");

        colours.Fade(EndingFade.ExplosionBackground, 5);
        colours.Fade(EndingFade.ExplodingPlanet, 5);
        colours.Load(EndingColourLoad.ExplosionBackgrounds, null);
        PictureShade[] restoredScene = [EndingShades.Loaded(EndingColourLoad.Explosion), EndingShades.Loaded(EndingColourLoad.ExplosionBackgrounds),
            EndingShades.Faded(EndingFade.ExplodingPlanet, 5)];
        AssertTrue(colours.Shades.SequenceEqual(restoredScene),
            "restoring the explosion's backgrounds ends the far background's fade-out and keeps the scene's other fades");

        colours.PaletteFx.Start(EndingPaletteFxDefinitions.GunshipEmergence, 0);
        colours.PaletteFx.StepScreen(Screen);
        PictureShade gunshipRecord = PaletteFxShades.Of(PaletteFxColours.GunshipEmergingFromExplosion,
            colours.PaletteFx.FrameOf(PaletteFxColours.GunshipEmergingFromExplosion)
            ?? throw new InvalidOperationException("The gunship's emergence showed no record on its first step."));
        AssertTrue(colours.Shades.SequenceEqual([.. restoredScene, gunshipRecord]), "a running palette effect shows the record it holds");
        AssertThrows<InvalidOperationException>(() => colours.Load(EndingColourLoad.FinalGunship, null),
            "the final gunship's colours cannot restore colours the gunship's emergence still animates");

        colours.ResetPaletteFx();
        AssertTrue(colours.Shades.SequenceEqual(restoredScene), "cleared palette effects stop showing their records, and nothing else");

        colours.Load(EndingColourLoad.PostCredits, black);
        AssertTrue(colours.Shades.SequenceEqual([EndingShades.Loaded(EndingColourLoad.PostCredits)]),
            "the post-credits scenes keep nothing of the scenes before them");

        // Each actor shows only the colour effects authored for it (#1544): the animals' escape pod, which once shared the
        // planet's colour group, shows none of the planet's fade, glow or fade-out, and no actor shows the gunship's record.
        var actors = new EndingColours();
        actors.Load(EndingColourLoad.Explosion, explosion);
        actors.Fade(EndingFade.ExplodingPlanet, 5);
        actors.Flash(EndingFlash.PlanetGlow, null);
        actors.PaletteFx.Start(EndingPaletteFxDefinitions.FadePlanet, 0);
        actors.PaletteFx.Start(EndingPaletteFxDefinitions.GunshipEmergence, 0);
        actors.PaletteFx.StepScreen(Screen);
        PictureShade planetFadeOut = PaletteFxShades.Of(PaletteFxColours.ZoomedOutExplodingZebes,
            actors.PaletteFx.FrameOf(PaletteFxColours.ZoomedOutExplodingZebes)
            ?? throw new InvalidOperationException("The planet's fade-out showed no record on its first step."));
        PictureShade actorsGunship = PaletteFxShades.Of(PaletteFxColours.GunshipEmergingFromExplosion,
            actors.PaletteFx.FrameOf(PaletteFxColours.GunshipEmergingFromExplosion)
            ?? throw new InvalidOperationException("The gunship's emergence showed no record on its first step."));
        PictureShade pod = EndingShades.Drawn(EndingSpriteColouring.AnimalEscape);
        PictureShade planet = EndingShades.Drawn(EndingSpriteColouring.ExplodingPlanet);
        PictureShade explosionColours = EndingShades.Loaded(EndingColourLoad.Explosion);
        AssertTrue(actors.Drawn(pod).Shades.SequenceEqual([pod, EndingShades.Under(pod, explosionColours)]),
            "the escape pod shows its scene's colours and none of the planet's effects");
        AssertTrue(actors.Drawn(planet).Shades.SequenceEqual([planet, EndingShades.Under(planet, explosionColours),
                EndingShades.Under(planet, EndingShades.Faded(EndingFade.ExplodingPlanet, 5)),
                EndingShades.Under(planet, EndingShades.Flashed(EndingFlash.PlanetGlow)), EndingShades.Under(planet, planetFadeOut)]),
            "the planet shows its fade, its glow's flash and its fade-out, in the order the scene brings them in");
        // Each background picture and floor shows only the effects authored for it (#1544): the gunship's floor shows its
        // scene's colours and its emergence, none of the planet's effects, and the explosion's pictures not the gunship's.
        AssertTrue(actors.SceneryShading(EndingScenery.FlyawayFloor).Shades.SequenceEqual([explosionColours, actorsGunship]),
            "the gunship's floor shows its scene's colours and its emergence, and none of the planet's effects");
        AssertTrue(actors.SceneryShading(EndingScenery.ExplosionPictures).Shades.SequenceEqual([explosionColours]),
            "the explosion's pictures show their scene's colours and not the gunship's emergence");

        // The post-credits shot's turning floor is drawn in the shot's colours, so it keeps the shot's fade, which the
        // restored shot ends, and none of Samus shooting's fade.
        var shot = new EndingColours();
        shot.Load(EndingColourLoad.PostCredits, black);
        shot.Fade(EndingFade.Shot, 7);
        shot.Fade(EndingFade.ShootingSamus, 3);
        AssertTrue(shot.SceneryShading(EndingScenery.PostCreditsShotFloor).Shades.SequenceEqual(
                [EndingShades.Loaded(EndingColourLoad.PostCredits), EndingShades.Faded(EndingFade.Shot, 7)]),
            "the post-credits shot's floor fades with the shot, not with Samus shooting");
        shot.Load(EndingColourLoad.RestoredShot, null);
        AssertTrue(shot.SceneryShading(EndingScenery.PostCreditsShotFloor).Shades.SequenceEqual(
                [EndingShades.Loaded(EndingColourLoad.PostCredits), EndingShades.Loaded(EndingColourLoad.RestoredShot)]),
            "the restored shot ends the shot's fade-out on its floor");

        // The backdrop is the scene's own until its flashes cover it, each over those the scene brings in before it.
        var backdrop = new EndingColours();
        backdrop.Load(EndingColourLoad.Explosion, explosion);
        backdrop.Flash(EndingFlash.SilhouetteBackdrop, EndingScreenDefinitions.WhiteColor);
        AssertEqual(EndingScreenDefinitions.WhiteColor, backdrop.Backdrop(null), "the silhouette whitens the backdrop");
        backdrop.Flash(EndingFlash.ExplosionWhiteout, EndingScreenDefinitions.WhiteColor);
        backdrop.Flash(EndingFlash.WhiteoutCleared, black);
        AssertEqual(black, backdrop.Backdrop(null), "the flyaway clears the whiteout's backdrop to black");
        backdrop.Load(EndingColourLoad.Credits, escape);
        AssertEqual(escape, backdrop.Backdrop(null), "the next scene shows its own backdrop");
        Console.WriteLine("  Ending scene colours: each scene shows its own colours, loads, fades, flashes and effects within a scene, "
            + "restores end the effects they restore, cleared effects end, each actor, picture and floor shows only its own effects.");
    }

    /// <summary>Runs the ending scene colour checks.</summary>
    [VerificationCommand("--ending-scene-colours")]
    internal static int EndingSceneColoursCommand()
    {
        Suite(nameof(VerifyEndingSceneColours), VerifyEndingSceneColours);
        return 0;
    }
}

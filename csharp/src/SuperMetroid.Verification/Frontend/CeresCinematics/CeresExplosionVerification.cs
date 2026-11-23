using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using static SuperMetroid.Verification.Samus.Palettes.PowerBombGlowColorsVerification;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Frontend.CeresCinematics;

namespace SuperMetroid.Verification.Frontend.CeresCinematics;

/// <summary>Checks the Ceres destruction blast against the shared Power Bomb color-math timeline.</summary>
internal static class CeresExplosionVerification
{
    /// <summary>Compares blast windows, RGB colors, phase coverage, and cleanup through the scene timeline, including installed-color ROM isolation.</summary>
    internal static void VerifyCeresExplosionTimeline(InstalledPowerBombGlowColors? colors = null)
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var guarded = NewForbiddenPowerBombColorBus(bus);
        ImportIntroCinematicArtwork artwork = InstalledFixtures.IntroArtwork;
        InstalledPowerBombGlowColors stockColors = colors ?? PowerBombGlowColorFile.Read(
            new MemoryStream(PowerBombGlowColorExtractor.Extract(bus)));
        var audio = AudioQueues.Standalone();
        var scene = new CeresDestructionCinematicState(audio, glowColors: stockColors, artwork: artwork.BoundCatalog,
            spriteSheets: CinematicSheetFixtures.WithIntro(artwork), outcome: SuperMetroid.Core.Frontend.CeresCinematics.CeresDestructionOutcome.SamusEscaped, language: TextLanguage.English);
        var referenceGlowColors = stockColors;
        var reference = new SamusPowerBombExplosionState();
        var phases = new HashSet<PowerBombExplosionPhase>();
        bool spawned = false, finished = false;
        int compared = 0;
        for (int frame = 0; frame < 1200 && !finished; frame++)
        {
            // Bank $82 runs HDMA before bank $8B dispatch. The reference is seeded
            // only when C345 transfers the station map and switches flight handler.
            reference.StepFrame(referenceGlowColors);
            var previous = scene.Phase;
            { scene.Step(); audio.AdvanceFrame(default); }
            if (previous == CeresDestructionPhase.ApproachExplosion &&
                scene.Phase == CeresDestructionPhase.FlyingAwayFromExplosion)
            {
                AssertTrue(!spawned, "station explosion spawns only once");
                reference.Spawn(
                    new(unchecked((ushort)(CeresDestructionSceneDefinitions.CeresFloorCenter.X - scene.FloorScroll.X.WholePixels)), unchecked((ushort)(CeresDestructionSceneDefinitions.CeresFloorCenter.Y - scene.FloorScroll.Y.WholePixels))));
                spawned = true;
            }

            var actual = scene.ComposeScene()
                .LayersOf<ColourAddWindowLayer>().SingleOrDefault();
            // The station's blast covers the screen from its top line, where a gameplay Power Bomb's starts below the HUD.
            var glow = GameplayEffectCapture.CapturePowerBombGlow(
                PowerBombRenderSnapshot.Capture(reference), new Position16<RoomPixels>(0, 0));
            var expected = glow is null || glow.Window == ScreenWindow.Closed ? glow
                : new ColourAddWindowLayer(new ScreenWindow(glow.Window.Shapes), glow.Colour, glow.Steps);
            AssertEqual(expected is null, actual is null, "Ceres blast window lifetime");
            if (expected is not null && actual is not null)
            {
                AssertEqual(expected, actual, $"Ceres blast frame {frame}: oval, colour and lines from the screen's top");
                compared++;
                phases.Add(reference.Blast.RenderedPhase);
            }
            finished = spawned && !reference.IsActive;
        }
        AssertTrue(finished, "station blast completes native cleanup");
        if (colors is not null)
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                "installed Ceres explosion does not read native Power Bomb colors");
        foreach (var phase in new PowerBombExplosionPhase[] { new PowerBombExplosionPhase.Running.PreExplosionWhite(),
        new PowerBombExplosionPhase.Running.PreExplosionYellow(), new PowerBombExplosionPhase.Running.ExplosionYellow(),
        new PowerBombExplosionPhase.Running.ExplosionWhite(), new PowerBombExplosionPhase.Running.Afterglow() })
            AssertTrue(phases.Contains(phase), $"station blast includes {phase}");
        Console.WriteLine($"  Ceres explosion: {compared} frames match shared cartridge curves/colors, fixed screen origin, phase timing and cleanup.");
    }
}

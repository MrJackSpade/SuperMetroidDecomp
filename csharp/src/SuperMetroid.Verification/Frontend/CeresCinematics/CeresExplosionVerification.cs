using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Samus.Palettes.PowerBombFixedColorsVerification;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.TestSupport.CpuBusTestDoubles;

namespace SuperMetroid.Verification.Frontend.CeresCinematics;

/// <summary>Checks the Ceres destruction blast against the shared Power Bomb color-math timeline.</summary>
internal static class CeresExplosionVerification
{
    /// <summary>Compares blast windows, RGB colors, phase coverage, and cleanup through the scene timeline, including installed-color ROM isolation.</summary>
    internal static void VerifyCeresExplosionTimeline(PowerBombFixedColorCatalog? colors = null)
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var guarded = NewForbiddenPowerBombColorBus(bus);
        using var artDirectoryScratch = new TestScratchDirectory("ceres-explosion");
        string artDirectory = artDirectoryScratch.Root;
        try
        {
            IntroCinematicArtworkFiles.Extract(bus, artDirectory, SupportedCartridge.Sha256);
            IntroCinematicArtworkCatalog artwork = IntroCinematicArtworkFiles.Load(artDirectory, null);
            PowerBombFixedColorCatalog stockColors = colors ?? PowerBombFixedColorCatalog.Load(
                new MemoryStream(PowerBombFixedColorExtractor.Extract(bus)));
            var scene = new CeresDestructionCinematicState(colors is null ? bus : guarded,
                fixedColors: stockColors, artwork: artwork);
            var reference = new SamusPowerBombExplosionState { PresentationColors = stockColors };
            var phases = new HashSet<PowerBombExplosionPhase>();
            bool spawned = false, finished = false;
            int compared = 0;
            for (int frame = 0; frame < 1200 && !finished; frame++)
            {
                // Bank $82 runs HDMA before bank $8B dispatch. The reference is seeded
                // only when C345 transfers the station map and switches flight handler.
                reference.StepFrame();
                var previous = scene.Phase;
                scene.Step();
                if (previous == CeresDestructionPhase.ApproachExplosion &&
                    scene.Phase == CeresDestructionPhase.FlyingAwayFromExplosion)
                {
                    AssertTrue(!spawned, "station explosion spawns only once");
                    reference.Spawn(
                        new(unchecked((ushort)(CeresDestructionRomData.Rendering.CeresCenterX - scene.Background.X.Whole)), unchecked((ushort)(CeresDestructionRomData.Rendering.CeresCenterY - scene.Background.Y.Whole))));
                    spawned = true;
                }

                var actual = scene.CaptureRenderSnapshot()
                    .LayersOf<ScanlineColorAddRenderLayer>().SingleOrDefault();
                var expected = SnesGameplayFrameRenderer.CapturePowerBombColorMath(
                    PowerBombRenderSnapshot.Capture(reference), new Position16<RoomPixels>(0, 0), firstVisibleScanline: 0);
                AssertEqual(expected is null, actual is null, "Ceres blast window lifetime");
                if (expected is not null && actual is not null)
                {
                    for (int y = 0; y < expected.Windows.Length; y++)
                        AssertEqual(expected.Windows[y], actual.Windows[y],
                            $"Ceres blast frame {frame} line {y}: endpoints and RGB");
                    compared++;
                    phases.Add(reference.RenderedPhase);
                }
                finished = spawned && !reference.IsActive;
            }
            AssertTrue(finished, "station blast completes native cleanup");
            if (colors is not null)
                AssertEqual(0, guarded.ForbiddenReadAttempts,
                    "installed Ceres explosion does not read native Power Bomb colors");
            foreach (var phase in new[] { PowerBombExplosionPhase.PreExplosionWhite,
            PowerBombExplosionPhase.PreExplosionYellow, PowerBombExplosionPhase.ExplosionYellow,
            PowerBombExplosionPhase.ExplosionWhite, PowerBombExplosionPhase.Afterglow })
                AssertTrue(phases.Contains(phase), $"station blast includes {phase}");
            Console.WriteLine($"  Ceres explosion: {compared} frames match shared cartridge curves/colors, fixed screen origin, phase timing and cleanup.");
        }
        finally
        {
            if (Directory.Exists(artDirectory)) Directory.Delete(artDirectory, recursive: true);
        }
    }
}

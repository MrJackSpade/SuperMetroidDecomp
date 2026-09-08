using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    private static void VerifyCeresExplosionTimeline()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var scene = new CeresDestructionCinematicState(bus);
        var reference = new SamusPowerBombExplosionState();
        var phases = new HashSet<PowerBombExplosionPhase>();
        bool spawned = false, finished = false;
        int compared = 0;
        for (int frame = 0; frame < 1200 && !finished; frame++)
        {
            // Bank $82 runs HDMA before bank $8B dispatch. The reference is seeded
            // only when C345 transfers the station map and switches flight handler.
            reference.StepFrame(bus);
            var previous = scene.Phase;
            scene.Step();
            if (previous == CeresDestructionPhase.ApproachExplosion &&
                scene.Phase == CeresDestructionPhase.FlyingAwayFromExplosion)
            {
                AssertTrue(!spawned, "station explosion spawns only once");
                reference.Spawn(
                    unchecked((ushort)(CeresDestructionRomData.Rendering.CeresCenterX - scene.BackgroundX)),
                    unchecked((ushort)(CeresDestructionRomData.Rendering.CeresCenterY - scene.BackgroundY)));
                spawned = true;
            }

            var actual = scene.CaptureRenderSnapshot().Layers.ToArray()
                .OfType<ScanlineColorAddRenderLayer>().SingleOrDefault();
            var expected = SnesGameplayFrameRenderer.CapturePowerBombColorMath(
                bus, reference, 0, 0, firstVisibleScanline: 0);
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
        foreach (var phase in new[] { PowerBombExplosionPhase.PreExplosionWhite,
            PowerBombExplosionPhase.PreExplosionYellow, PowerBombExplosionPhase.ExplosionYellow,
            PowerBombExplosionPhase.ExplosionWhite, PowerBombExplosionPhase.Afterglow })
            AssertTrue(phases.Contains(phase), $"station blast includes {phase}");
        Console.WriteLine($"  Ceres explosion: {compared} frames match shared cartridge curves/colors, fixed screen origin, phase timing and cleanup.");
    }
}

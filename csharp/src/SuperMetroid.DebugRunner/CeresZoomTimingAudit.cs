using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

/// <summary>Checks each approach call, not merely eventual cinematic completion.</summary>
internal static class CeresZoomTimingAudit
{
    public static int Run(string romPath, string outputDirectory)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var scene = new CeresDestructionCinematicState(bus);
        Directory.CreateDirectory(outputDirectory);
        int mismatches = 0, approach = 0;
        for (int frame = 0; frame < 1000; frame++)
        {
            var beforePhase = scene.Phase;
            int before = scene.Zoom;
            scene.Step();
            int after = scene.Zoom;
            if (beforePhase == CeresDestructionPhase.ApproachExplosion)
            {
                approach++;
                if (scene.Phase == CeresDestructionPhase.ApproachExplosion && after != before + 1)
                    mismatches++;
            }
            if (frame is 220 or 270 || scene.Phase == CeresDestructionPhase.FlyingAwayFromExplosion)
                PngWriter.WriteRgba(Path.Combine(outputDirectory, $"frame-{frame:D4}-{scene.Phase}.png"), 256, 224, scene.Render());
            if (scene.Phase == CeresDestructionPhase.FlyingAwayFromExplosion)
            {
                Console.WriteLine($"Ceres departure frame {frame}; approach calls {approach}; incorrect scale steps {mismatches}.");
                if (mismatches != 0 || approach == 0)
                    throw new InvalidDataException("Ceres approach must increment Mode 7 scale once per call, as $8B:C345 does.");
                return 0;
            }
        }
        throw new InvalidDataException("Ceres departure was not reached.");
    }
}

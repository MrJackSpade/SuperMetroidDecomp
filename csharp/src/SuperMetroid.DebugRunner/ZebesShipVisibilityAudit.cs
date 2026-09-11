using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

/// <summary>Verifies the visible Mode 7 ship disappears at the cartridge's OBJ-only handoff.</summary>
internal static class ZebesShipVisibilityAudit
{
    public static int Run(string romPath, string outputDirectory)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var scene = new CeresDestructionCinematicState(bus);
        Directory.CreateDirectory(outputDirectory);
        int visibleApproachFrames = 0, hiddenFrames = 0, failedFrames = 0;
        int holdFrames = 0, slideFrames = 0;
        for (int frame = 0; frame < 2000 && !scene.Finished; frame++)
        {
            scene.Step();
            bool approach = scene.Phase == CeresDestructionPhase.FlyingTowardZebesC;
            bool hidden = scene.Phase is CeresDestructionPhase.HoldCloseZebes or
                CeresDestructionPhase.SlideZebesSceneAway;
            if (!approach && !hidden) continue;
            var packet = scene.CaptureRenderSnapshot();
            var objOnly = new LayeredRenderSnapshot(packet.Memory,
                packet.Layers.ToArray().Where(layer => layer is not Mode7RenderLayer).ToArray(),
                packet.ObjectSelection, packet.Brightness);
            var expected = SoftwareLayeredSnapshotRenderer.Render(objOnly);
            var actual = scene.Render();
            var captured = SoftwareLayeredSnapshotRenderer.Render(packet);
            int different = actual.Where((pixel, index) => pixel != expected[index]).Count();
            if (approach && different > 0) visibleApproachFrames++;
            if (hidden)
            {
                hiddenFrames++;
                if (different != 0 || !captured.SequenceEqual(expected) ||
                    packet.Layers.ToArray().Any(layer => layer is Mode7RenderLayer)) failedFrames++;
                if (scene.Phase == CeresDestructionPhase.HoldCloseZebes) holdFrames++;
                else slideFrames++;
            }
            if ((approach && visibleApproachFrames == 1) ||
                (hidden && (holdFrames == 1 || slideFrames == 1)))
            {
                string name = $"{frame:D4}-{scene.Phase}";
                PngWriter.WriteRgba(Path.Combine(outputDirectory, name + ".png"), 256, 224, actual);
                PngWriter.WriteRgba(Path.Combine(outputDirectory, name + "-obj-only.png"), 256, 224, expected);
                Console.WriteLine($"{name}: visible Mode 7 contribution {different} pixels.");
            }
        }
        Console.WriteLine($"Ship visibility: {visibleApproachFrames} approach frames; " +
            $"{holdFrames} hold/{slideFrames} slide frames; {failedFrames} incorrect hidden frames.");
        if (visibleApproachFrames == 0 || holdFrames != 64 || slideFrames == 0 ||
            hiddenFrames == 0 || failedFrames != 0)
            throw new InvalidDataException("Zebes ship visibility does not honor the native OBJ-only handoff.");
        return 0;
    }
}

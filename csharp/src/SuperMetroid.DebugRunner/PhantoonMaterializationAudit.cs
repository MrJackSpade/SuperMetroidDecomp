using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;

/// <summary>#555: capture the real room's initial materialization without driving its AI phases.</summary>
internal static class PhantoonMaterializationAudit
{
    public static int Run(string rom, string directory)
    {
        Directory.CreateDirectory(directory);
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xcd13);
        runtime.InitializeDebugGroundedSamus(64, 192, 12);
        runtime.Samus!.InputLocked = false;
        int waveFrames = 0, varyingRows = 0;
        using var trace = new StreamWriter(Path.Combine(directory, "frames.csv"));
        trace.WriteLine("frame,phase,amplitude,layerFlags,bg2X,bg2Y,scrollRows,distinctScrolls");
        for (int frame = 0; frame < 1000; frame++)
        {
            runtime.StepFrame(0);
            var boss = runtime.Enemies.Phantoon ?? throw new InvalidDataException("Missing Phantoon encounter.");
            var packet = GameplayDisplayCapture.CaptureOrdinaryBase(runtime);
            var layer = (OrdinaryGameplayRenderLayer)packet.Layers[0];
            int distinct = layer.HorizontalScrolls.ToArray().Distinct().Count();
            trace.WriteLine($"{frame},{boss.Body.VariableF:X4},{boss.Mouth!.VariableD},{boss.SemiTransparencyLayerFlags:X4},{layer.Registers.Bg2X},{layer.Registers.Bg2Y},{layer.HorizontalScrolls.Length},{distinct}");
            if (boss.Body.VariableF == (ushort)PhantoonAiFunction.WavyFadeIn && boss.Mouth.VariableD >= 256)
            {
                waveFrames++;
                if (distinct > 1) varyingRows++;
                var pixels = SoftwareLayeredSnapshotRenderer.Render(packet);
                PngWriter.WriteRgba(Path.Combine(directory, $"wave-{frame:D4}.png"), 256, 224, pixels);
            }
        }
        Console.WriteLine($"Phantoon materialization: {waveFrames} active-amplitude frames, {varyingRows} with per-row scroll variation.");
        if (waveFrames == 0 || varyingRows == 0)
            throw new InvalidDataException("Phantoon's real-room materialization does not publish a visible per-scanline wave.");
        return 0;
    }
}

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
        var runtime = CreateEncounter(rom);
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
                if (distinct > 1)
                {
                    var flatLayer = new OrdinaryGameplayRenderLayer(layer.Registers, [], layer.VerticalScrolls.ToArray());
                    var flat = SoftwareLayeredSnapshotRenderer.Render(new LayeredRenderSnapshot(
                        packet.Memory, [flatLayer], packet.ObjectSelection, packet.Brightness));
                    if (pixels.AsSpan().SequenceEqual(flat))
                        throw new InvalidDataException("Wave table varies but actual body pixels do not move.");
                    if (!pixels.AsSpan(0, 256 * 32).SequenceEqual(flat.AsSpan(0, 256 * 32)))
                        throw new InvalidDataException("Phantoon wave moved the HUD.");
                }
                PngWriter.WriteRgba(Path.Combine(directory, $"wave-{frame:D4}.png"), 256, 224, pixels);
            }
        }
        Console.WriteLine($"Phantoon materialization: {waveFrames} active-amplitude frames, {varyingRows} with per-row scroll variation.");
        if (waveFrames != 165 || varyingRows != 162)
            throw new InvalidDataException("Phantoon's real-room materialization does not publish a visible per-scanline wave.");
        return 0;
    }

    internal static SuperMetroidRuntime CreateEncounter(string rom)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        // Finish the bootstrap room's pending graphics transfer before loading another
        // room. Its $4000..$4FFF startup upload otherwise overwrites the new boss BG2 map
        // at the next NMI, a fixture artifact normal door loading has already drained.
        runtime.VramWrites.DrainTo(runtime.Vram, runtime.AddressSpace);
        runtime.LoadCartridgeRoomForDebug(0xcd13);
        runtime.InitializeDebugGroundedSamus(64, 192, 12);
        runtime.Samus!.InputLocked = false;
        return runtime;
    }
}

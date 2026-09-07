using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Rendering.Direct3D11;

internal static class RetailKraidCaptureTests
{
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug); runtime.RunNmi(0, true);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomThroughDoorForVerification(CartridgeDoorHeader.Load(bus, KraidCaptureDefinitions.IncomingDoor), 0, 256);
        var samus = runtime.Samus!;
        // Match the existing Kraid rise audit's stationary approach position.
        samus.XPosition = 128; samus.YPosition = 456;
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
        var boss = runtime.Enemies.Kraid ?? throw new InvalidOperationException("Kraid did not initialize.");
        var body = runtime.Enemies.Slots[0];
        var phases = new HashSet<ushort>();
        int samples = 0;
        bool completed = false, ownedBackground = false;
        for (int tick = 0; tick < 1200; tick++)
        {
            bool changed = phases.Add(body.VariableA);
            ownedBackground |= boss.OwnsBg2Tilemap;
            bool mainLoop = body.VariableA == (ushort)KraidAiFunction.MainloopThinking;
            if (changed || tick % 8 == 0 || mainLoop)
            {
                var expected = SuperMetroidRuntimeFrameRenderer.Render(runtime);
                var packet = new RenderFrameSnapshot(new(++samples, 1, (ushort)tick), GameplayDisplayCapture.TryCaptureFrame(runtime)!);
                packet = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotCodec.Serialize(packet));
                PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet),
                    $"{device.Kind}: Kraid rise tick={tick}, AI={body.VariableA:X4}");
            }
            if (mainLoop) { completed = true; break; }
            runtime.StepFrame(0);
        }
        if (!completed || !ownedBackground || phases.Count < 2)
            throw new InvalidOperationException("Kraid capture missed rise, BG2 ownership or first-phase handoff.");
        Console.WriteLine($"{device.Kind}: Kraid rise reaches first-phase main loop; {samples} exact samples across {phases.Count} AI states.");
    }
}

internal static class KraidCaptureDefinitions
{
    /// <summary>Retail incoming door $83:91B6 used by the existing Kraid rise audit.</summary>
    internal const ushort IncomingDoor = 0x91b6;
}

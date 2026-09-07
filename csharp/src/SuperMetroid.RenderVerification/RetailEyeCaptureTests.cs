using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Rendering.Direct3D11;

internal static class RetailEyeCaptureTests
{
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var runtime = new SuperMetroidRuntime(SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
        runtime.InitializeHud(HudSnapshot.CeresDebug); runtime.RunNmi(0, true);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.MorphBallRoom,
            EyeCaptureFixture.CameraX, EyeCaptureFixture.CameraY);
        var body = runtime.Enemies.Slots[1];
        var samus = runtime.Samus!;
        samus.CollectedItems = (ushort)SamusEquipmentFlags.MorphBall;
        samus.XPosition = unchecked((ushort)(body.XPosition + 64));
        samus.YPosition = body.YPosition;
        samus.InputLocked = true;
        bool firstBeam = false, fullBeam = false;
        for (int tick = 0; tick < 96; tick++)
        {
            runtime.StepFrame(0);
            firstBeam |= runtime.DisplayedMorphBallEyeBeam is { Phase: MorphBallEyeBeamPhase.Widening, AngularWidth: 0 };
            fullBeam |= runtime.DisplayedMorphBallEyeBeam is { Phase: MorphBallEyeBeamPhase.Full };
            var expected = SuperMetroidRuntimeFrameRenderer.Render(runtime);
            var packet = new RenderFrameSnapshot(new(tick + 1, 1, (ushort)tick), GameplayDisplayCapture.TryCaptureFrame(runtime)!);
            packet = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotCodec.Serialize(packet));
            PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet), $"{device.Kind}: retail eye frame {tick}");
        }
        if (!firstBeam || !fullBeam) throw new InvalidOperationException("Eye fixture missed first visible beam or full-width beam.");
        Console.WriteLine($"{device.Kind}: 96 retail eye activation/widening/full-beam frames match exactly, including first visible publication.");
    }
}

internal static class EyeCaptureFixture
{
    /// <summary>World camera X used by the production Morph Ball eye apex regression.</summary>
    internal const ushort CameraX = 0x0380;
    /// <summary>World camera Y used by the production Morph Ball eye apex regression.</summary>
    internal const ushort CameraY = 0x01c0;
}

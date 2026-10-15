using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.RenderVerification.Bosses;
using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.RepositoryInstallation;

/// <summary>Checks D3D11 output against production rendering during the retail Morph Ball eye-beam sequence.</summary>
internal static class RetailEyeCaptureTests
{
    /// <summary>Advances 96 eye activation frames and compares captured, serialized render packets with software-rendered pixels, including first and full beam visibility.</summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var runtime = RepositoryInstallation.CreateRuntime(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug); runtime.NmiHandler.RunNmi(0, true);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.MorphBallRoom,
            new Position16<RoomPixels>(EyeCaptureFixture.CameraX, EyeCaptureFixture.CameraY));
        var body = runtime.Enemies.SlotTable.Slots[1];
        var samus = runtime.Player.IntroducedSamus;
        samus.CollectedItems = (ushort)SamusEquipmentFlags.MorphBall;
        samus.Position = samus.Position.WithWhole(new(unchecked((ushort)(body.Position.X.Whole + 64)), body.Position.Y.Whole));
        samus.InputLocked = true;
        bool firstBeam = false, fullBeam = false;
        for (int tick = 0; tick < 96; tick++)
        {
            runtime.StepFrame(0);
            firstBeam |= runtime.Display.DisplayedMorphBallEyeBeam is { Phase: MorphBallEyeBeamPhase.Widening, AngularWidth: 0 };
            fullBeam |= runtime.Display.DisplayedMorphBallEyeBeam is { Phase: MorphBallEyeBeamPhase.Full };
            var expected = SuperMetroidRuntimeFrameRenderer.Render(runtime.PublishDisplayFrame());
            var packet = new RenderFrameSnapshot(new(tick + 1, 1, (ushort)tick), GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()));
            packet = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotWriter.Serialize(packet));
            PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet), $"{device.Kind}: retail eye frame {tick}");
        }
        if (!firstBeam || !fullBeam) throw new InvalidOperationException("Eye fixture missed first visible beam or full-width beam.");
        Console.WriteLine($"{device.Kind}: 96 retail eye activation/widening/full-beam frames match exactly, including first visible publication.");
    }
}

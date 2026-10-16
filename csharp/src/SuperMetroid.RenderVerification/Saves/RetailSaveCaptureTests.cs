using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
namespace SuperMetroid.RenderVerification.Saves;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.RepositoryInstallation;

/// <summary>Checks every captured frame of a retail save station cycle, including electricity and control release.</summary>
internal static class RetailSaveCaptureTests
{
    /// <summary>Stages the Crateria station and compares GPU frames until save completion unlocks Samus.</summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = RepositoryInstallation.CreateRuntime(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug); runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointersTooling.CrateriaSaveStation, new Position16<RoomPixels>(0, 0));
        var station = runtime.Plms.StationSnapshots.Single(s => s.Kind == StationKind.Save);
        var level = runtime.Room.LoadedLevelData;
        var samus = runtime.Player.IntroducedSamus;
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        samus.AnimationFrame = 0;
        samus.PoseMetadata.RefreshCollisionRadii(); samus.Presentation.InitializeAnimation();
        samus.Position = samus.Position.WithWhole(new((ushort)(station.BlockIndex % level.Size.X * 16 + 13), (ushort)(station.BlockIndex / level.Size.X * 16 - samus.Kinematics.Radius.Y)));
        samus.InputLocked = false; samus.Presentation.PrimeGraphics();
        if (!runtime.Plms.Stations.TryNotifyStationCollision(station.BlockIndex, new RoomBlockBehavior((byte)StationAccessBehavior.SaveFloor),
            samus.Pose, horizontal: false, movingPositive: true, roomWidthInBlocks: level.Size.X))
            throw new InvalidOperationException("Save fixture could not trigger resident station.");
        bool confirmation = false, electricity = false, completion = false, finished = false;
        bool pressedLastFrame = false;
        int frames = 0;
        for (; frames < 700; frames++)
        {
            // Rising edges acknowledge only the actual message input phase. All PLM,
            // projectile animation, OAM and NMI publication work remains production code.
            bool press = runtime.MessageBox.Phase == GameplayMessageBoxPhase.AwaitingInput && !pressedLastFrame;
            runtime.StepFrame(press ? SnesButton.A : (ushort)0);
            pressedLastFrame = press;
            confirmation |= runtime.MessageBox.IsActive && runtime.MessageBox.MessageId == GameplayMessageIds.SaveConfirmation;
            completion |= runtime.MessageBox.IsActive && runtime.MessageBox.MessageId == GameplayMessageIds.SaveCompleted;
            if (!electricity && runtime.Enemies.Projectiles.EnemyProjectiles.Any(p => p.IsActive && p.Kind == RoomEnemyProjectileKind.SaveStationElectricity))
            {
                if (runtime.Enemies.Projectiles.EnemyProjectiles.Any(p => p.IsActive && p.Kind != RoomEnemyProjectileKind.SaveStationElectricity))
                    throw new InvalidOperationException("Unrelated projectile contaminates electricity visibility assertion.");
                var oam = new OamBuffer(); oam.BeginFrame();
                runtime.Enemies.DrawEnemyProjectiles(oam, runtime.Room.LoadedCamera.PixelPosition);
                oam.FinalizeFrame();
                electricity = SnesObjRenderer.RenderResolved(oam, runtime.Vram, runtime.Cgram,
                    GameplayRenderDefinitions.ObjectSelection).Pixels.Any(p => p.A != 0 && (p.R != 0 || p.G != 0 || p.B != 0));
            }
            var expected = FrontendScenePixelsAccess.Pixels(runtime.PublishDisplayFrame());
            var packet = new RenderFrameSnapshot(new(frames + 1, 1, (ushort)frames), GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()));
            packet = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotWriter.Serialize(packet));
            PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet), $"{device.Kind}: save animation frame {frames}");
            var current = runtime.Plms.StationSnapshots.Single(s => s.Kind == StationKind.Save);
            if (completion && current.SaveStationLockedOut && current.SavePhase == SaveStationPhase.Idle && !samus.InputLocked)
            { finished = true; break; }
        }
        if (!finished || !confirmation || !electricity || !completion)
            throw new InvalidOperationException("Save fixture missed confirmation/electricity/completion/control release.");
        Console.WriteLine($"{device.Kind}: {frames + 1} exact save-station frames through confirmation, electricity, completion and control release.");
    }
}

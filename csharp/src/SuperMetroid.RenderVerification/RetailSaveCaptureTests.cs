using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Rendering.Direct3D11;

internal static class RetailSaveCaptureTests
{
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug); runtime.RunNmi(0, true);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.CrateriaSaveStation, 0, 0);
        var station = runtime.Plms.Stations.Single(s => s.Kind == StationKind.Save);
        var level = runtime.LevelData!;
        var samus = runtime.Samus!;
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.AnimationFrame = 0;
        samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
        samus.XPosition = (ushort)(station.BlockIndex % level.WidthInBlocks * 16 + 13);
        samus.YPosition = (ushort)(station.BlockIndex / level.WidthInBlocks * 16 - samus.Kinematics.YRadius);
        samus.InputLocked = false; samus.PrimeGraphics(bus);
        if (!runtime.Plms.TryNotifyStationCollision(station.BlockIndex, (byte)StationAccessBehavior.SaveFloor,
            samus.Pose, horizontal: false, movingPositive: true, roomWidthInBlocks: level.WidthInBlocks))
            throw new InvalidOperationException("Save fixture could not trigger resident station.");
        bool confirmation = false, electricity = false, completion = false, finished = false;
        bool pressedLastFrame = false;
        int frames = 0;
        for (; frames < 700; frames++)
        {
            // Rising edges acknowledge only the actual message input phase. All PLM,
            // projectile animation, OAM and NMI publication work remains production code.
            bool press = runtime.MessageBox.Phase == GameplayMessageBoxPhase.AwaitingInput && !pressedLastFrame;
            runtime.StepFrame(press ? (ushort)SnesButton.A : (ushort)0);
            pressedLastFrame = press;
            confirmation |= runtime.MessageBox.IsActive && runtime.MessageBox.MessageId == GameplayMessageIds.SaveConfirmation;
            completion |= runtime.MessageBox.IsActive && runtime.MessageBox.MessageId == GameplayMessageIds.SaveCompleted;
            if (!electricity && runtime.Enemies.EnemyProjectiles.Any(p => p.IsActive && p.Kind == RoomEnemyProjectileKind.SaveStationElectricity))
            {
                if (runtime.Enemies.EnemyProjectiles.Any(p => p.IsActive && p.Kind != RoomEnemyProjectileKind.SaveStationElectricity))
                    throw new InvalidOperationException("Unrelated projectile contaminates electricity visibility assertion.");
                var oam = new OamBuffer(); oam.BeginFrame();
                runtime.Enemies.DrawEnemyProjectiles(oam, runtime.Camera!.XPosition, runtime.Camera.YPosition);
                oam.FinalizeFrame();
                electricity = SnesObjRenderer.RenderResolved(oam, runtime.Vram, runtime.Cgram,
                    GameplayRenderDefinitions.ObjectSelection).Pixels.Any(p => p.A != 0 && (p.R != 0 || p.G != 0 || p.B != 0));
            }
            var expected = SuperMetroidRuntimeFrameRenderer.Render(runtime);
            var packet = new RenderFrameSnapshot(new(frames + 1, 1, (ushort)frames), GameplayDisplayCapture.TryCaptureFrame(runtime)!);
            packet = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotCodec.Serialize(packet));
            PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet), $"{device.Kind}: save animation frame {frames}");
            var current = runtime.Plms.Stations.Single(s => s.Kind == StationKind.Save);
            if (completion && current.SaveStationLockedOut && current.SavePhase == SaveStationPhase.Idle && !samus.InputLocked)
            { finished = true; break; }
        }
        if (!finished || !confirmation || !electricity || !completion)
            throw new InvalidOperationException("Save fixture missed confirmation/electricity/completion/control release.");
        Console.WriteLine($"{device.Kind}: {frames + 1} exact save-station frames through confirmation, electricity, completion and control release.");
    }
}

using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyXrayInput()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(ShutterRidingRomData.XrayScopeRoom);
        var samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        // The platform's lowest position is a Morph-only tunnel. Standing there
        // embeds the torso in terrain and cannot validate a visible X-ray beam.
        // Use the open floor in the Scope chamber instead.
        samus.XPosition = 128;
        samus.YPosition = 139;
        samus.EquippedItems = (ushort)SamusEquipmentFlags.XrayScope;
        samus.Missiles = samus.SuperMissiles = samus.PowerBombs = 0;
        runtime.StepFrame(0);
        runtime.StepFrame((ushort)SnesButton.Select);
        AssertEqual(SamusXrayRomData.SelectedHudItem, samus.SelectedHudItem, "Select reaches the equipped X-ray through ordinary HUD input");
        runtime.StepFrame(runtime.ControllerBindings.Dash);
        AssertTrue(samus.Xray.IsActive, "held Run activates selected X-ray without diagnostic activation");
        AssertTrue(runtime.TimeIsFrozen, "X-ray activation freezes gameplay");
        for (int frame = 0; frame < 90; frame++)
        {
            runtime.StepFrame(runtime.ControllerBindings.Dash);
            VerifyXrayWindowGeometry(bus, samus);
        }
        AssertEqual(XrayBeamPhase.Full, samus.Xray.BeamPhase, "held Run widens the X-ray beam fully");
        var horizontal = GameplayDisplayCapture.TryCaptureFrame(runtime)!;
        var horizontalPixels = SoftwareLayeredSnapshotRenderer.Render(horizontal);
        var probe = runtime.LevelData!.GetPlmCollisionBlockByIndex(7 * runtime.LevelData.WidthInBlocks + 12);
        AssertEqual((ushort)255, XrayRevealTable.Find(bus, probe.CollisionType, probe.Behavior)!.Value.TopLeft,
            "native air definition replaces decorative wall art with the blank metatile");
        var ordinaryPixels = SoftwareLayeredSnapshotRenderer.Render(GameplayDisplayCapture.CaptureOrdinaryBase(runtime));
        int revealedAirPixel = 120 * 256 + 200;
        AssertTrue(ordinaryPixels[revealedAirPixel] != new Rgba32(24, 24, 24, 255), "fixture starts with visible decorative wall art");
        AssertEqual(new Rgba32(24, 24, 24, 255), horizontalPixels[revealedAirPixel],
            "live X-ray removes decorative air graphics inside its horizontal beam");
        if (Environment.GetCommandLineArgs().Contains("--xray-input"))
        {
            Directory.CreateDirectory("csharp/test-temp/issue-348-xray");
            PngWriter.WriteRgba("csharp/test-temp/issue-348-xray/horizontal.png", 256, 224, horizontalPixels);
            File.WriteAllBytes("csharp/test-temp/issue-348-xray/horizontal.smframe",
                RenderFrameSnapshotCodec.Serialize(new(new(1, 1, 0), horizontal)));
        }
        for (int frame = 0; frame < 240; frame++)
        {
            runtime.StepFrame((ushort)(runtime.ControllerBindings.Dash | (ushort)(frame < 80 ? SnesButton.Up : SnesButton.Down)));
            VerifyXrayWindowGeometry(bus, samus);
        }
        var captured = GameplayDisplayCapture.TryCaptureFrame(runtime)!;
        var reveal = captured.Layers.ToArray().OfType<XrayGameplayRenderLayer>().Single();
        AssertTrue(reveal.Lines.ToArray().Any(line => line.Left <= line.Right), "full X-ray publishes a nonempty visible window");
        var pixels = SoftwareLayeredSnapshotRenderer.Render(captured);
        AssertTrue(pixels.SequenceEqual(SuperMetroidRuntimeFrameRenderer.Render(runtime)), "immediate software entrypoint uses the same X-ray display operation");
        AssertTrue(reveal.RevealBlocks, "ordinary X-ray room selects terrain revelation");
        AssertEqual((byte)0x73, (byte)reveal.ColorMath, "native reveal-room CGADSUB");
        AssertEqual(XrayRoomDisplayRules.ActiveBackdrop, captured.Memory.Cgram[0], "setup backdrop reaches the packet");
        var frozenMap = XraySetupMemory.ReadReveal(bus);
        for (int i = 0; i < frozenMap.Length; i++)
        {
            int offset = (SnesPpuLayout.GameplayBg2TilemapWord + i) * 2;
            AssertEqual(frozenMap[i], (ushort)(captured.Memory.Vram[offset] | captured.Memory.Vram[offset + 1] << 8), "captured BG2 is the setup-owned reveal map");
        }
        if (Environment.GetCommandLineArgs().Contains("--xray-input"))
            PngWriter.WriteRgba("csharp/test-temp/issue-348-xray/aimed.png", 256, 224, pixels);
        var beforeRelease = RenderFrameSnapshotCodec.Serialize(new(new(1, 1, 0), captured));
        for (int frame = 0; frame < 30; frame++) runtime.StepFrame(0);
        AssertTrue(!samus.Xray.IsActive && !runtime.TimeIsFrozen, "releasing Run restores ordinary gameplay");
        AssertTrue(!GameplayDisplayCapture.TryCaptureFrame(runtime)!.Layers.ToArray().Any(layer => layer is XrayGameplayRenderLayer),
            "release removes the X-ray display window");
        AssertTrue(pixels.SequenceEqual(SoftwareLayeredSnapshotRenderer.Render(captured)),
            "a queued X-ray frame remains immutable after gameplay resumes");
        AssertTrue(beforeRelease.SequenceEqual(RenderFrameSnapshotCodec.Serialize(new(new(1, 1, 0), captured))),
            "a queued X-ray packet retains its reveal memory after gameplay resumes");
        foreach (ushort roomPointer in new[] { XrayRoomDisplayRules.ExcludedRoomA66A, XrayRoomDisplayRules.ExcludedRoomWithHiddenBg2 })
        {
            runtime.LoadCartridgeRoomForDebug(roomPointer);
            samus.InputLocked = false;
            samus.Pose = SamusPoseIds.FacingRightNormalPose;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.Kinematics.YSpeed = samus.Kinematics.YSubspeed = 0;
            // This fixture checks display selection only; X-ray freezes the seeded
            // position so it need not drive an unrelated room's entrance sequence.
            AssertTrue(samus.Xray.TryBegin(bus, samus, samus.ReadMovementType(bus)), "excluded-room capture fixture activates");
            for (int frame = 0; frame < 90; frame++) runtime.StepFrame(runtime.ControllerBindings.Dash);
            var baseFrame = GameplayDisplayCapture.CaptureOrdinaryBase(runtime);
            var excludedFrame = GameplayDisplayCapture.TryCaptureFrame(runtime)!;
            var excluded = (XrayGameplayRenderLayer)excludedFrame.Layers[0];
            AssertTrue(!excluded.RevealBlocks, "native excluded room never substitutes the reveal map");
            AssertTrue(baseFrame.Memory.Vram.SequenceEqual(excludedFrame.Memory.Vram), "excluded room retains original VRAM");
            AssertTrue((excluded.ColorMath & SnesColorMathControl.Obj) == 0, "excluded-room CGADSUB leaves Samus color unchanged");
            AssertEqual(roomPointer != XrayRoomDisplayRules.ExcludedRoomWithHiddenBg2,
                excluded.Gameplay.Registers.MainScreenLayers.HasFlag(SnesMainScreenLayers.Bg2), "CEFB alone removes BG2");
            for (int frame = 0; frame < 8; frame++) runtime.StepFrame(0);
        }
        Console.WriteLine("  X-ray input: normal Select/Run activation, full beam and release restore gameplay.");
    }
}

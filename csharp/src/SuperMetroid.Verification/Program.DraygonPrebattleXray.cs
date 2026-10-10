using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>Verifies that pre-battle X-Ray keeps Draygon's offscreen body clipped by the native HDMA window and that the body appears when combat begins.</summary>
    private static void VerifyDraygonPrebattleXray()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        byte[] nativeOffscreenTm = [0x1f, 0x04, 0x81, 0x11, 0x00];
        AssertSequenceEqual(nativeOffscreenTm, Enumerable.Range(0, nativeOffscreenTm.Length)
            .Select(offset => bus.ReadCartridgeByte(0x88e01a + offset)),
            "native offscreen HDMA table enables BG1/OBJ without BG2 below the HUD");
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xda60, cameraX: 256, cameraY: 288);
        var samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.Pose = SamusPoseIds.FacingLeftNormalPose;
        samus.XPosition = 456; samus.YPosition = 459;
        samus.EquippedItems = (ushort)SamusEquipmentFlags.XrayScope;
        samus.SelectedHudItem = SamusXrayRomData.SelectedHudItem;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        runtime.StepFrame(0);
        runtime.StepFrame(runtime.ControllerBindings.Dash);
        AssertTrue(samus.Xray.IsActive, "bottom-right arena position activates X-Ray through held Run");
        for (int frame = 0; frame < 40; frame++) runtime.StepFrame(runtime.ControllerBindings.Dash);
        var boss = runtime.Enemies.Draygon!;
        AssertEqual(DraygonAiFunction.IntroInitialDelay, boss.Function, "X-Ray preserves the pre-battle phase");
        var scene = GameplayDisplayCapture.TryCaptureFrame(runtime)!;
        var math = (GameplayColorMathRenderLayer)scene.Layers[0];
        var r = math.Gameplay.Registers;
        AssertTrue(!math.RevealBlocks, "native boss exclusion preserves the original BG2 artwork");
        AssertEqual(32, r.Bg2FirstScanline, "native offscreen TM table begins below HUD");
        AssertEqual(32, r.Bg2EndScanline, "native offscreen TM table hides all Draygon BG2 lines");
        var pixels = SoftwareLayeredSnapshotRenderer.Render(scene);
        var hidden = RenderWithRegisters(r with { MainScreenLayers = r.MainScreenLayers & ~SnesMainScreenLayers.Bg2 });
        var unmasked = RenderWithRegisters(r with { Bg2FirstScanline = 32, Bg2EndScanline = 224 });
        int exposed = pixels.Zip(hidden).Count(pair => pair.First != pair.Second);
        int wrapped = unmasked.Zip(hidden).Count(pair => pair.First != pair.Second);
        Console.WriteLine($"Pre-battle Draygon: body={boss.Body.XPosition:X4}/{boss.Body.YPosition:X4}, Samus={samus.XPosition}/{samus.YPosition}, camera={r.Bg1X}/{r.Bg1Y}; exposed={exposed}, unmasked={wrapped} pixels.");
        const string output = "csharp/test-temp/issue-1260-draygon-xray";
        Directory.CreateDirectory(output);
        File.WriteAllBytes(Path.Combine(output, "frame-000.smframe"), RenderFrameSnapshotCodec.Serialize(new(new(1, 1, runtime.NmiFrameCounter), scene)));
        PngWriterTooling.WriteRgba(Path.Combine(output, "prebattle.png"), 256, 224, pixels);
        AssertTrue(wrapped > 0, "the reported corner visibly exposes repeated body art without native clipping");
        AssertEqual(0, exposed, "pre-battle X-Ray hides the offscreen body exactly like native TM HDMA");

        // End X-Ray, then stage the native end of the intro rather than replaying the dance.
        for (int frame = 0; frame < 8; frame++) runtime.StepFrame(0);
        AssertTrue(!samus.Xray.IsActive, "release restores ordinary gameplay");
        boss.FunctionTimer = 0x100;
        runtime.StepFrame(0); // Initial-delay exit records the native reset positions and acceleration.
        boss.FunctionTimer = DraygonIntroDanceDefinitions.DurationFrames;
        runtime.StepFrame(0); // Dance completion selects the first swoop.
        runtime.StepFrame(0); // Build the authored swoop path.
        int descentFrames = boss.SwoopPathEntryCount;
        for (int frame = 0; frame < descentFrames + 2; frame++) runtime.StepFrame(0);
        var combat = GameplayDisplayCapture.CaptureOrdinaryBase(runtime);
        var ordinary = (OrdinaryGameplayRenderLayer)combat.Layers[0];
        var combatWithoutBg2 = new LayeredRenderSnapshot(combat.Memory,
            new RenderLayer[] { new OrdinaryGameplayRenderLayer(ordinary.Registers with
                { MainScreenLayers = ordinary.Registers.MainScreenLayers & ~SnesMainScreenLayers.Bg2 }, ordinary.HorizontalScrolls, ordinary.VerticalScrolls) },
            combat.ObjectSelection, combat.Brightness);
        int visible = SoftwareLayeredSnapshotRenderer.Render(combat).Zip(SoftwareLayeredSnapshotRenderer.Render(combatWithoutBg2)).Count(pair => pair.First != pair.Second);
        Console.WriteLine($"Battle-start Draygon body: {visible} visible pixels.");
        AssertTrue(visible > 0, "battle start still displays Draygon's body");
        File.WriteAllBytes(Path.Combine(output, "frame-001.smframe"), RenderFrameSnapshotCodec.Serialize(new(new(2, 1, runtime.NmiFrameCounter), combat)));

        Rgba32[] RenderWithRegisters(OrdinaryGameplayRegisters registers)
        {
            var gameplay = new OrdinaryGameplayRenderLayer(registers, math.Gameplay.HorizontalScrolls, math.Gameplay.VerticalScrolls, math.Gameplay.MainScreenLayersByLine);
            var changed = new GameplayColorMathRenderLayer(gameplay, math.Lines, math.RevealBlocks, math.ColorMath,
                math.AddSubscreen, math.FixedRed, math.FixedGreen, math.FixedBlue, math.Subscreen, math.SubscreenUsesBg2);
            return SoftwareLayeredSnapshotRenderer.Render(new(scene.Memory, new RenderLayer[] { changed }, scene.ObjectSelection, scene.Brightness));
        }
    }
}

using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.Draygon;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Checks Draygon's pre-battle X-Ray masking and later combat visibility.</summary>
internal static class DraygonPrebattleXrayVerification
{
    /// <summary>Verifies that the native TM HDMA range hides wrapped body art during X-Ray without hiding the combat body.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the Draygon offscreen HDMA table")]
    internal static void VerifyDraygonPrebattleXray()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        byte[] nativeOffscreenTm = [0x1f, 0x04, 0x81, 0x11, 0x00];
        AssertSequenceEqual(nativeOffscreenTm, Enumerable.Range(0, nativeOffscreenTm.Length)
            .Select(offset => bus.ReadCartridgeByte(0x88e01a + offset)),
            "native offscreen HDMA table enables BG1/OBJ without BG2 below the HUD");
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Draygon, camera: new Position16<RoomPixels>(256, 288));
        var samus = runtime.Player.IntroducedSamus;
        samus.Control.Unlock();
        samus.Pose = SamusPoseId.FacingLeftNormalPose;
        samus.Position = samus.Position.WithWhole(new(456, 459));
        samus.EquippedItems = SamusEquipmentFlags.XrayScope;
        samus.SelectedHudItem = HudItem.XRay;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        runtime.StepFrame(0);
        runtime.StepFrame(runtime.Options.ControllerBindings.Dash);
        AssertTrue(samus.Xray.IsActive, "bottom-right arena position activates X-Ray through held Run");
        for (int frame = 0; frame < 40; frame++) runtime.StepFrame(runtime.Options.ControllerBindings.Dash);
        var boss = runtime.Enemies.DraygonBehavior.Parts.Draygon;
        AssertEqual(DraygonAiFunction.IntroInitialDelay, (boss ?? throw new InvalidOperationException("Verification requires boss.")).Function, "X-Ray preserves the pre-battle phase");
        var scene = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
        var math = (GameplayColorMathRenderLayer)scene.Layers[0];
        var r = math.Gameplay.View;
        AssertTrue(!math.RevealBlocks, "native boss exclusion preserves the original BG2 artwork");
        AssertEqual(32, r.Bg2FirstScanline, "native offscreen TM table begins below HUD");
        AssertEqual(32, r.Bg2EndScanline, "native offscreen TM table hides all Draygon BG2 lines");
        var pixels = DisplayRendering.Render(scene);
        var hidden = RenderWithRegisters(r with { MainScreenLayers = r.MainScreenLayers & ~ScreenLayers.Bg2 });
        var unmasked = RenderWithRegisters(r with { Bg2FirstScanline = 32, Bg2EndScanline = 224 });
        int exposed = pixels.Zip(hidden, (first, second) => first != second).Count(differs => differs);
        int wrapped = unmasked.Zip(hidden, (first, second) => first != second).Count(differs => differs);
        Console.WriteLine($"Pre-battle Draygon: body={boss.Body.Position.X.Whole:X4}/{boss.Body.Position.Y.Whole:X4}, Samus={samus.Position.X.Whole}/{samus.Position.Y.Whole}, camera={r.Bg1Scroll.X}/{r.Bg1Scroll.Y}; exposed={exposed}, unmasked={wrapped} pixels.");

        AssertTrue(wrapped > 0, "the reported corner visibly exposes repeated body art without native clipping");
        AssertEqual(0, exposed, "pre-battle X-Ray hides the offscreen body exactly like native TM HDMA");

        // End X-Ray, then stage the native end of the intro rather than replaying the dance.
        for (int frame = 0; frame < 8; frame++) runtime.StepFrame(0);
        AssertTrue(!samus.Xray.IsActive, "release restores ordinary gameplay");
        boss.FunctionTimer = WrappingCountdown.Start(0x100);
        runtime.StepFrame(0); // Initial-delay exit records the native reset positions and acceleration.
        boss.FunctionTimer = WrappingCountdown.Start(DraygonIntroDanceDefinitions.DurationFrames);
        runtime.StepFrame(0); // Dance completion selects the first swoop.
        runtime.StepFrame(0); // Build the authored swoop path.
        int descentFrames = boss.SwoopPathEntryCount;
        for (int frame = 0; frame < descentFrames + 2; frame++) runtime.StepFrame(0);
        var combat = GameplayDisplayCapture.CaptureOrdinaryBase(runtime.PublishDisplayFrame());
        var ordinary = (OrdinaryGameplayRenderLayer)combat.Layers[0];
        var combatWithoutBg2 = new LayeredScene(combat.Paint,
            new RenderLayer[] { new OrdinaryGameplayRenderLayer(ordinary.View with
                { MainScreenLayers = ordinary.View.MainScreenLayers & ~ScreenLayers.Bg2 }, ordinary.Rooms, ordinary.Bg2LineScrolls) }, combat.Brightness);
        int visible = DisplayRendering.Render(combat).Zip(DisplayRendering.Render(combatWithoutBg2), (first, second) => first != second).Count(differs => differs);
        Console.WriteLine($"Battle-start Draygon body: {visible} visible pixels.");
        AssertTrue(visible > 0, "battle start still displays Draygon's body");

        RgbaPixel[] RenderWithRegisters(OrdinaryGameplayView registers)
        {
            var gameplay = new OrdinaryGameplayRenderLayer(registers, math.Gameplay.Rooms, math.Gameplay.Bg2LineScrolls, math.Gameplay.MainScreenLayersByLine);
            var changed = new GameplayColorMathRenderLayer(gameplay, math.Lines, math.RevealBlocks, math.Blend, math.Subscreen, math.SubscreenUsesBg2);
            return DisplayRendering.Render(new LayeredScene(scene.Paint, new RenderLayer[] { changed }, scene.Brightness));
        }
    }

    /// <summary>Runs the Draygon pre-battle masking regression.</summary>
    [VerificationCommand("--draygon-prebattle-xray")]
    internal static int DraygonPrebattleXrayCommand()
    {
     VerifyDraygonPrebattleXray(); return 0; 
    }
}

using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Drawing;
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
    /// <summary>The rendered screen's width in pixels, one row of the render's pixels.</summary>
    private const int ScreenWidth = 256;

    /// <summary>The farthest across or down from its area's corner Draygon's body pictures reach, in pixels.</summary>
    private const int BodyAreaExtent = 128;

    /// <summary>
    /// Verifies that pre-battle X-Ray in the arena's corner shows none of Draygon's offscreen body, with or without the native
    /// background-line range, and that the body still displays at battle start.
    /// </summary>
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
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.Draygon, camera: new Position16<RoomPixels>(256, 288));
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
        AssertEqual(DraygonPhase.IntroInitialDelay, (boss ?? throw new InvalidOperationException("Verification requires boss.")).Phase, "X-Ray preserves the pre-battle phase");
        var scene = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
        var math = (GameplayBlendLayer)scene.Layers[0];
        var r = math.Gameplay.View;
        AssertTrue(!math.RevealBlocks, "native boss exclusion preserves the original BG2 artwork");
        AssertEqual(32, r.BackgroundFirstLine, "native offscreen TM table begins below HUD");
        AssertEqual(32, r.BackgroundEndLine, "native offscreen TM table hides all Draygon BG2 lines");
        var pixels = DisplayRendering.Render(scene);
        var hidden = RenderWithRegisters(r with { ShownLayers = r.ShownLayers & ~ScreenLayers.Background });
        var unmasked = RenderWithRegisters(r with { BackgroundFirstLine = 32, BackgroundEndLine = 224 });
        int exposed = pixels.Zip(hidden, (first, second) => first != second).Count(differs => differs);
        int wrapped = unmasked.Zip(hidden, (first, second) => first != second).Count(differs => differs);
        Console.WriteLine($"Pre-battle Draygon: body={boss.Body.Position.X.Whole:X4}/{boss.Body.Position.Y.Whole:X4}, Samus={samus.Position.X.Whole}/{samus.Position.Y.Whole}, camera={r.ForegroundScroll.X}/{r.ForegroundScroll.Y}; exposed={exposed}, unmasked={wrapped} pixels.");

        // The reported exposure (#1260) was the body layer's art repeating into the corner. Since #1617 the body reaches the
        // display placed at the body's own room position, which before the battle lies off the view, so no repeated art
        // reaches the corner even with the background lines unclipped; the battle-start check below shows the same body
        // placement does display once it is in view.
        AssertEqual(0, wrapped, "the pre-battle body, placed at its own position off the view, shows nothing even unclipped");
        AssertEqual(0, exposed, "pre-battle X-Ray shows none of the offscreen body");

        // End X-Ray, then stage the native end of the intro rather than replaying the dance.
        for (int frame = 0; frame < 8; frame++) runtime.StepFrame(0);
        AssertTrue(!samus.Xray.IsActive, "release restores ordinary gameplay");
        boss.IntroFrames = 256;
        runtime.StepFrame(0); // Initial-delay exit records the native reset positions and acceleration.
        boss.IntroFrames = DraygonIntroDanceDefinitions.DurationFrames;
        runtime.StepFrame(0); // Dance completion selects the first swoop.
        runtime.StepFrame(0); // Build the authored swoop path.
        int descentFrames = (boss.SwoopPath ?? throw new InvalidOperationException("The first swoop plans its path.")).EntryCount;
        for (int frame = 0; frame < descentFrames + 2; frame++) runtime.StepFrame(0);
        var combat = GameplayDisplayCapture.CaptureOrdinaryBase(runtime.PublishDisplayFrame());
        var ordinary = (GameplaySceneLayer)combat.Layers[0];
        // The HUD is its own layer after the room (#1466); both renders keep it.
        var combatWithoutBg2 = new LayeredScene(combat.Paint,
            new DisplayLayer[] { new GameplaySceneLayer(ordinary.View with
                { ShownLayers = ordinary.View.ShownLayers & ~ScreenLayers.Background }, ordinary.Rooms, ordinary.BackgroundLineScrolls),
                combat.Layers[1] }, combat.Brightness);
        RgbaPixel[] withBody = DisplayRendering.Render(combat), withoutBody = DisplayRendering.Render(combatWithoutBg2);
        int[] shown = [.. Enumerable.Range(0, withBody.Length).Where(index => withBody[index] != withoutBody[index])];
        // #1680: his body's pictures lie once in the room at his parts' area, whose top-left corner is the published origin;
        // the screen shows them there against the room pixel at its top-left corner, starting at the area's corner.
        DraygonBodyPlacement placement = runtime.Display.Latched.Bosses.DraygonBody ?? throw new InvalidOperationException("Draygon publishes his body placement.");
        Offset16<ScreenPixels> corner = placement.BodyOrigin.RelativeToCamera(ordinary.View.ForegroundScroll);
        Console.WriteLine($"Battle-start Draygon body: {shown.Length} visible pixels; body={boss.Body.Position.X.Whole}/{boss.Body.Position.Y.Whole}, " +
            $"its area's corner on screen {corner.X}/{corner.Y}.");
        AssertTrue(shown.Length > 0, "battle start still displays Draygon's body");
        AssertEqual(corner.X, shown.Min(index => index % ScreenWidth), "the body's leftmost shown pixel is its area's left edge");
        AssertEqual(corner.Y, shown.Min(index => index / ScreenWidth), "the body's topmost shown pixel is its area's top edge");
        AssertTrue(shown.Max(index => index % ScreenWidth) < corner.X + BodyAreaExtent && shown.Max(index => index / ScreenWidth) < corner.Y + BodyAreaExtent,
            "the body shows within its parts' area around his body, not a repeat of the layer away from it");

        RgbaPixel[] RenderWithRegisters(GameplaySceneView registers)
        {
            var gameplay = new GameplaySceneLayer(registers, math.Gameplay.Rooms, math.Gameplay.BackgroundLineScrolls, math.Gameplay.Bands);
            var changed = new GameplayBlendLayer(gameplay, math.Window, math.RevealBlocks, math.Blend, math.OverlayBehind, math.BackgroundBehind);
            // The scene's later layers, the HUD's among them (#1466), stay as captured.
            DisplayLayer[] layers = scene.Layers.ToArray();
            layers[0] = changed;
            return DisplayRendering.Render(new LayeredScene(scene.Paint, layers, scene.Brightness));
        }
    }

    /// <summary>Runs the Draygon pre-battle masking regression.</summary>
    [VerificationCommand("--draygon-prebattle-xray")]
    internal static int DraygonPrebattleXrayCommand()
    {
     VerifyDraygonPrebattleXray(); return 0; 
    }
}

using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Samus.Abilities;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Checks X-Ray activation, beam display, release, and excluded-room input routes.</summary>
internal static class XrayInputVerification
{
    /// <summary>Exercises ordinary Select/Run X-ray activation, beam display, release, and excluded-room behavior.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifyXrayInput()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.XrayScope);
        var samus = runtime.Player.IntroducedSamus;
        samus.Control.Unlock();
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        // The platform's lowest position is a Morph-only tunnel. Standing there
        // embeds the torso in terrain and cannot validate a visible X-ray beam.
        // Use the open floor in the Scope chamber instead.
        samus.Position = samus.Position.WithWhole(new(128, 139));
        samus.EquippedItems = SamusEquipmentFlags.XrayScope;
        samus.Missiles = samus.SuperMissiles = samus.PowerBombs = 0;
        runtime.StepFrame(0);
        runtime.StepFrame(SnesButton.Select);
        AssertEqual(HudItem.XRay, samus.SelectedHudItem, "Select reaches the equipped X-ray through ordinary HUD input");
        runtime.StepFrame(runtime.Options.ControllerBindings.Dash);
        AssertTrue(samus.Xray.IsActive, "held Run activates selected X-ray without diagnostic activation");
        AssertTrue(runtime.Freeze.TimeIsFrozen, "X-ray activation freezes gameplay");
        for (int frame = 0; frame < 90; frame++)
        {
            runtime.StepFrame(runtime.Options.ControllerBindings.Dash);
            if (frame < 8)
            {
                var startup = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
                AssertEqual(frame != 0, startup.Layers[0] is GameplayColorMathRenderLayer,
                    "setup pre-instruction starts blending one call after the instruction list installs it");
                if (frame != 0)
                {
                    AssertTrue(((GameplayColorMathRenderLayer)startup.Layers[0]).Lines.All(line => line.Left > line.Right),
                        "setup keeps the beam closed while copying tilemaps");
                    AssertEqual(frame == 7 ? XrayRoomDisplayRules.ActiveBackdrop : Bgr555.Black, startup.Paint.Colors[0],
                        "only setup stage eight installs the X-ray backdrop");
                }
            }
        }
        AssertEqual(XrayBeamPhase.Full, samus.Xray.BeamPhase, "held Run widens the X-ray beam fully");
        var horizontal = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
        var horizontalPixels = DisplayRendering.Render(horizontal);
        var probe = runtime.Room.LoadedLevelData.GetPlmCollisionBlockByIndex(7 * runtime.Room.LoadedLevelData.Size.X + 12);
        AssertEqual((ushort)255, (XrayRevealTable.Find(probe.CollisionType, probe.Bts.Value) ?? throw new InvalidOperationException("Verification requires XrayRevealTable.Find(probe.CollisionType, probe.Bts.Value).")).TopLeft,
            "native air definition replaces decorative wall art with the blank metatile");
        var ordinaryPixels = DisplayRendering.Render(GameplayDisplayCapture.CaptureOrdinaryBase(runtime.PublishDisplayFrame()));
        int revealedAirPixel = 120 * 256 + 200;
        AssertTrue(ordinaryPixels[revealedAirPixel] != new Rgba32(24, 24, 24, 255), "fixture starts with visible decorative wall art");
        AssertEqual(new Rgba32(24, 24, 24, 255), horizontalPixels[revealedAirPixel],
            "live X-ray removes decorative air graphics inside its horizontal beam");
        if (Environment.GetCommandLineArgs().Contains("--xray-input"))
        {

        }
        for (int frame = 0; frame < 240; frame++)
            runtime.StepFrame((runtime.Options.ControllerBindings.Dash | (frame < 80 ? SnesButton.Up : SnesButton.Down)));
        var captured = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
        var reveal = captured.LayersOf<GameplayColorMathRenderLayer>().Single();
        AssertTrue(reveal.Lines.Any(line => line.Left <= line.Right), "full X-ray publishes a nonempty visible window");
        var pixels = DisplayRendering.Render(captured);
        AssertTrue(reveal.RevealBlocks, "ordinary X-ray room selects terrain revelation");
        AssertEqual(ColorBlend.Bg1 | ColorBlend.Bg2 | ColorBlend.Sprites | ColorBlend.Backdrop | ColorBlend.Half, reveal.Blend.Layers,
            "a reveal room halves its added blend over both backgrounds, the sprites and the backdrop");
        AssertEqual(XrayRoomDisplayRules.ActiveBackdrop, captured.Paint.Colors[0], "setup backdrop reaches the packet");
        // BG2 shows the setup-owned reveal map, as the blocks its words lay out (#1403).
        var frozenMap = XraySetupMemory.ReadReveal(runtime.Xray);
        RevealedBlock?[] frozenBlocks = XrayRevealPlane.BlocksOf(frozenMap,
            new RevealedBlockIndex(runtime.Room.LoadedLevelData.ForegroundPlane.Definitions));
        XrayRevealPlane shownReveal = reveal.Gameplay.Rooms.XrayReveal
            ?? throw new InvalidOperationException("The captured X-ray frame shows no reveal.");
        AssertTrue(Enumerable.SequenceEqual(shownReveal.Blocks.ToArray(), frozenBlocks), "captured BG2 is the setup-owned reveal map");

        RevealedBlock?[] beforeRelease = shownReveal.Blocks.ToArray();
        runtime.StepFrame(0);
        var releaseEdge = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
        AssertTrue(releaseEdge.Layers[0] is GameplayColorMathRenderLayer,
            "release edge retains the beam until native deactivation executes on the following frame");
        AssertTrue(((GameplayColorMathRenderLayer)releaseEdge.Layers[0]).Lines.SequenceEqual(reveal.Lines),
            "release edge retains the last HDMA window endpoints");
        for (int frame = 0; frame < 2; frame++)
        {
            runtime.StepFrame(0);
            var restoring = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
            AssertTrue(restoring.Layers[0] is GameplayColorMathRenderLayer, "BG2 restore retains X-ray layer ownership");
            AssertTrue(((GameplayColorMathRenderLayer)restoring.Layers[0]).Lines.All(line => line.Left > line.Right),
                "deactivation closes the beam while BG2 restore completes");
            AssertEqual(0, restoring.Paint.Colors[0], "deactivation resets the backdrop before unfreezing");
        }
        runtime.StepFrame(0);
        AssertTrue(!runtime.Freeze.TimeIsFrozen, "phase five releases gameplay time");
        var finishFrame = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
        AssertTrue(finishFrame.Layers[0] is GameplayColorMathRenderLayer,
            "phase five still publishes X-ray blending before the next HDMA pass resets configuration");
        AssertEqual(0, ((GameplayColorMathRenderLayer)finishFrame.Layers[0]).Blend.FixedColor.Red,
            "phase five clears the non-Fireflea fixed color");
        for (int frame = 0; frame < 26; frame++) runtime.StepFrame(0);
        AssertTrue(!samus.Xray.IsActive && !runtime.Freeze.TimeIsFrozen, "releasing Run restores ordinary gameplay");
        AssertTrue(!GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers.Any(layer => layer is GameplayColorMathRenderLayer),
            "release removes the X-ray display window");
        AssertTrue(pixels.SequenceEqual(RenderForComparison(captured)),
            "a queued X-ray frame remains immutable after gameplay resumes");
        AssertTrue(Enumerable.SequenceEqual(beforeRelease, captured.LayersOf<GameplayColorMathRenderLayer>().Single().Gameplay.Rooms.XrayReveal?.Blocks.ToArray() ?? []),
            "a queued X-ray frame retains its reveal after gameplay resumes");
        foreach (RoomHeaderDefinition excludedRoom in new[] { RoomHeaderDefinitions.Statues, RoomHeaderDefinitions.GlassTunnel })
        {
            runtime.LoadCartridgeRoomForDebug(excludedRoom);
            samus.Control.Unlock();
            samus.Pose = SamusPoseId.FacingRightNormalPose;
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            SamusPresentation.InitializeAnimation(samus);
            samus.Kinematics.YSpeed = default;
            // This fixture checks display selection only; X-ray freezes the seeded
            // position so it need not drive an unrelated room's entrance sequence.
            AssertTrue(samus.Xray.TryBegin(samus, SamusPoseMetadata.ReadMovementType(samus), new XrayBeginConditions { GameState = SuperMetroidGameState.MainGameplay }), "excluded-room capture fixture activates");
            for (int frame = 0; frame < 90; frame++) runtime.StepFrame(runtime.Options.ControllerBindings.Dash);
            var baseFrame = GameplayDisplayCapture.CaptureOrdinaryBase(runtime.PublishDisplayFrame());
            var excludedFrame = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
            var excluded = (GameplayColorMathRenderLayer)excludedFrame.Layers[0];
            AssertTrue(!excluded.RevealBlocks, "native excluded room never substitutes the reveal map");
            var baseRooms = ((OrdinaryGameplayRenderLayer)baseFrame.Layers[0]).Rooms;
            AssertTrue(excluded.Gameplay.Rooms.XrayReveal is null && excluded.Gameplay.Rooms.Bg2 == baseRooms.Bg2
                    && excluded.Gameplay.Rooms.Bg2Image == baseRooms.Bg2Image, "excluded room retains the room's own BG2");
            AssertTrue((excluded.Blend.Layers & ColorBlend.Sprites) == 0, "an excluded room's blend leaves Samus color unchanged");
            AssertEqual(excludedRoom != RoomHeaderDefinitions.GlassTunnel,
                excluded.Gameplay.View.MainScreenLayers.HasFlag(ScreenLayers.Bg2), "CEFB alone removes BG2");
            for (int frame = 0; frame < 8; frame++) runtime.StepFrame(0);
        }
        Console.WriteLine("  X-ray input: normal Select/Run activation, full beam and release restore gameplay.");
    }

    /// <summary>Runs the X-ray input and display verification suite.</summary>
    [VerificationCommand("--xray-input")]
    internal static int XrayInputCommand()
    {
        Suite(nameof(VerifyXrayInput), () => VerifyXrayInput());
        return 0;
    }
}

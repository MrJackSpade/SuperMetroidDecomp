using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Earthquakes;
namespace SuperMetroid.Verification.Areas.Tourian;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.TestSupport.FrameRendering;
/// <summary>Checks the four-statues room's special water FX in live physics and displayed BG3 composition.</summary>
internal static class TourianStatueWaterVerification
{
    /// <summary>Drives the retail room through water motion, freeze, and independent statue descent.</summary>
    internal static int VerifyTourianStatueWater()
    {
        var game=SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.CreateGame(new SuperMetroid.Core.Saves.SaveSlotStore(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps));
        var projectiles=InstalledFixtures.Projectiles;
        InstalledFixtures.BindGame(game);
        game.Session.CreateGameplayRuntime(GameplayRuntimePurpose.SelectedGame);
        var runtime = game.Session.GameplayRuntime;
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();runtime.RoomLoader.InitializeCeresStartSamus(runtime.Room.Loaded);
        runtime.System.SetEvent(StoryEvent.ZebesAwake);
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.Statues);
        AssertEqual(RoomFxType.TourianEntranceStatue, runtime.RoomFxOverlay.Type, "Actual four-statues room owns special FX 26");
        AssertEqual((ushort)0xb0, runtime.RoomFxOverlay.NamedBaseY.Whole, "Retail statues water begins at Y=B0");
        AssertEqual(true, runtime.RoomFxOverlay.IsRenderable, "Statue special FX must supply its water plane");
        var samus = runtime.Player.Samus;
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        samus.Position = samus.Position.WithWhole(new(64, 120));
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        samus.CommitPoseHistory();
        samus.Control.Unlock();
        runtime.Room.LoadedCamera.SetPosition(new Position16<RoomPixels>(0, 0));
        game.Session.Enter(SuperMetroidGameState.MainGameplay);
        for (int frame = 0; frame < 3; frame++) game.Step(0);
        AssertEqual(RoomFxType.TourianEntranceStatue, samus.LiquidPhysics.FxType, "Samus retains native special FX identity");
        AssertEqual(0xb0, samus.LiquidPhysics.WaterSurfaceY, "Water physics receives the visible surface");
        samus.Position = samus.Position.WithWholeY(190);
        samus.LiquidPhysics.InitializeRememberedMedium(samus);
        AssertEqual(SamusLiquidMedium.Water, samus.LiquidPhysics.MediumLastEntered,
            "Native low-nibble FX dispatch selects water below the statue surface");
        var displayed = runtime.Display.DisplayedRoomFxOverlay ?? throw new InvalidOperationException("No displayed water snapshot");
        AssertEqual(0xb0, displayed.WaterSurfaceScreenY, "Displayed water surface stays at native room height");
        AssertEqual(RoomEffectBlendDefinitions.OverlayBrightens, displayed.Blend, "the statues' water brightens the room");
        var basis = runtime.PublishDisplayFrame().OrdinaryBaseScene;
        var full = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
        AssertEqual(1, full.Layers.Count(layer => layer is OverlayBlendLayer), "One BG3 water plane accompanies the statue");
        var dryPixels = DisplayRendering.Render(basis);
        var wetPixels = DisplayRendering.Render(full);
        for (int y = 32; y < 169; y++)
            for (int x = 0; x < 256; x++)
                AssertEqual(dryPixels[y * 256 + x], wetPixels[y * 256 + x], "BG3 water does not tint pixels above its native surface tile");
        AssertTrue(Enumerable.Range(176 * 256, (224 - 176) * 256).Any(index => dryPixels[index] != wetPixels[index]),
            "Actual water artwork visibly changes submerged room pixels");
        AssertTrue(GameplayEffectCapture.BuildWaterBackgroundHorizontalScrolls(displayed, Offset16<BackgroundPixels>.Zero) is null,
            "Special statue FX does not install ordinary-water BG2 X-wave HDMA");
        runtime.TourianStatues.Room.Descent = SignedSixteenSixteen.FromWholePixels(64);
        runtime.TourianStatues.LatchDisplay();
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        var descending = runtime.PublishDisplayFrame().OrdinaryBase.Room;
        // The view shows the background row below the descent's camera row at the screen's top (#1514): with the camera at
        // the room's top, the descent puts that row above the background's origin, a negative scroll, not a wrapped one
        // (#2305).
        AssertEqual(RoomView.ShownRow(runtime.Display.DisplayedGameplayScrolls.ForegroundPosition.Y + runtime.TourianStatues.DisplayedVerticalOffset),
            (int)descending.View.BackgroundScroll.Y, "Statue descent retains its independent BG2 vertical scroll");
        AssertTrue(descending.View.BackgroundScroll.Y < 0, "the statues' descent scrolls the background above its origin");
        AssertEqual(0xb0, (runtime.Display.DisplayedRoomFxOverlay ?? throw new InvalidOperationException("Verification requires a published room FX snapshot.")).WaterSurfaceScreenY,
            "Statue motion does not move its water surface");
        runtime.RoomFxOverlay.MoveLiquidTo(0xb4, SignedEightEight.FromPixels(1), 1);
        game.Step(0);
        AssertEqual(0xb0, runtime.RoomFxOverlay.NamedSurface, "Water motion first arms its native wait");
        game.Step(0);
        AssertEqual(0xb0, runtime.RoomFxOverlay.NamedSurface, "Water wait expires before movement");
        runtime.AudioRequests.BeginRecordedUpdate();
        game.Step(0);
        AssertEqual(0xb1, runtime.RoomFxOverlay.NamedSurface, "Special water follows the native one-pixel velocity");
        AssertEqual(0xb1, samus.LiquidPhysics.WaterSurfaceY, "The moved water surface reaches live Samus physics");
        AssertEqual<EarthquakeShake?>(null, runtime.RoomFxOverlay.SustainedEarthquake,
            "Water motion does not invoke the lava/acid earthquake branch");
        AssertEqual(0, runtime.AudioRequests.SoundRequests.Count, "Water motion does not invoke lava/acid sound feedback");
        runtime.RoomFxOverlay.Step(runtime.Room.LoadedScope.ArtworkFrames, new RoomFxOverlayFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = true });
        AssertEqual(0xb1, runtime.RoomFxOverlay.NamedSurface, "Frozen HDMA retains the current water surface");
        Console.WriteLine("Four-statues water: actual FX, liquid publication, visible BG3 pixels and independent statue descent verified.");
        return 0;
    }

    /// <summary>Runs the four-statues water fixture as a verification command.</summary>
    [VerificationCommand("--tourian-statue-water")]
    internal static int TourianStatueWaterCommand()
    {
        return VerifyTourianStatueWater();
    }
}

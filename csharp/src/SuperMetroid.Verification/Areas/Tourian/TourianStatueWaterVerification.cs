using SuperMetroid.Core;
using System.Reflection;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Areas.Tourian;
using SuperMetroid.Verification.Harness;
/// <summary>Checks the four-statues room's special water FX in live physics and displayed BG3 composition.</summary>
internal static class TourianStatueWaterVerification
{
    /// <summary>Drives the retail room through water motion, freeze, and independent statue descent.</summary>
    internal static int VerifyTourianStatueWater()
    {
        const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
        var installation = RepositoryInstallation.Installation;
        var bus=installation.OpenRuntimeAddressSpace();
        var game=new SuperMetroidGame(bus);
        var projectiles=InstalledFixtures.Projectiles;
        InstalledFixtures.BindGame(game);
        game.Session.CreateGameplayRuntime(false);
        var runtime=game.Session.runtime;
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebesAwake);
        runtime.LoadCartridgeRoomForDebug(0xa66a);
        AssertEqual(RoomFxType.TourianEntranceStatue, runtime.RoomLayer3Fx.Type, "Actual four-statues room owns special FX 26");
        AssertEqual((ushort)0xb0, runtime.RoomLayer3Fx.BaseYPosition, "Retail statues water begins at Y=B0");
        AssertEqual(true, runtime.RoomLayer3Fx.IsRenderable, "Statue special FX must supply its water plane");
        var samus = runtime.Player.IntroducedSamus;
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        samus.Position = samus.Position.WithWhole(new(64, 120));
        samus.PoseMetadata.RefreshCollisionRadii(bus);
        samus.Presentation.InitializeAnimation(bus);
        samus.CommitPoseHistory(bus);
        samus.InputLocked = false;
        runtime.Room.LoadedCamera.SetPosition(new Position16<RoomPixels>(0, 0));
        game.GameState = SuperMetroidGameState.MainGameplay;
        for (int frame = 0; frame < 3; frame++) game.Step(0);
        AssertEqual(RoomFxType.TourianEntranceStatue, samus.LiquidPhysics.FxType, "Samus retains native special FX identity");
        AssertEqual((ushort)0xb0, samus.LiquidPhysics.FxYPosition, "Water physics receives the visible surface");
        samus.Position = samus.Position.WithWholeY(190);
        samus.LiquidPhysics.InitializeRememberedMedium(samus);
        AssertEqual(SamusLiquidMedium.Water, samus.LiquidPhysics.LiquidPhysicsType,
            "Native low-nibble FX dispatch selects water below the statue surface");
        var displayed = runtime.Display.DisplayedRoomLayer3Fx ?? throw new InvalidOperationException("No displayed water snapshot");
        AssertEqual((short)0xb0, displayed.WaterSurfaceScreenY, "Displayed water surface stays at native room height");
        AssertEqual(LayerBlendingConfiguration.LiquidOrFogAdditive, displayed.LayerBlendConfiguration, "Native configuration 18");
        var basis = GameplayDisplayCapture.CaptureOrdinaryBase(runtime.PublishDisplayFrame());
        var full = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
        AssertEqual(1, full.Layers.Count(layer => layer is Bg2BppColorMathRenderLayer), "One BG3 water plane accompanies the statue");
        var dryPixels = SoftwareLayeredSnapshotRenderer.Render(basis);
        var wetPixels = SoftwareLayeredSnapshotRenderer.Render(full);
        for (int y = 32; y < 169; y++)
            for (int x = 0; x < 256; x++)
                AssertEqual(dryPixels[y * 256 + x], wetPixels[y * 256 + x], "BG3 water does not tint pixels above its native surface tile");
        AssertTrue(Enumerable.Range(176 * 256, (224 - 176) * 256).Any(index => dryPixels[index] != wetPixels[index]),
            "Actual water artwork visibly changes submerged room pixels");
        var direct = dryPixels.ToArray();
        SnesGameplayFrameRendererConstants.ApplyRoomLayer3FxColorMath(direct, runtime.Vram, runtime.Cgram, displayed);
        AssertTrue(direct.SequenceEqual(wetPixels), "Immediate and captured composition agree for the actual water room");
        AssertEqual<ushort[]?>(null, SnesGameplayFrameRenderer.BuildWaterBg2HorizontalScrolls(displayed, new Position16<Bg2Pixels>(0, 0)),
            "Special statue FX does not install ordinary-water BG2 X-wave HDMA");
        runtime.TourianStatues.descent = 64 << 16;
        runtime.TourianStatues.LatchDisplay();
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        var descending = (OrdinaryGameplayRenderLayer)GameplayDisplayCapture.CaptureOrdinaryBase(runtime.PublishDisplayFrame()).Layers[0];
        AssertEqual(unchecked((ushort)(runtime.Display.DisplayedGameplayPpu.Layer1Position.Y + runtime.TourianStatues.DisplayedVerticalOffset)),
            descending.Registers.Bg2Scroll.Y, "Statue descent retains its independent BG2 vertical scroll");
        AssertEqual((short)0xb0, (runtime.Display.DisplayedRoomLayer3Fx ?? throw new InvalidOperationException("Verification requires a published room FX snapshot.")).WaterSurfaceScreenY,
            "Statue motion does not move its water surface");
        runtime.RoomLayer3Fx.ApplyCartridgeMotionWrites(targetYPosition: 0xb4,
            packedYVelocity: 0x0100, timer: 1);
        game.Step(0);
        AssertEqual((ushort)0xb0, runtime.RoomLayer3Fx.CurrentYPosition, "Water motion first arms its native wait");
        game.Step(0);
        AssertEqual((ushort)0xb0, runtime.RoomLayer3Fx.CurrentYPosition, "Water wait expires before movement");
        game.Step(0);
        AssertEqual((ushort)0xb1, runtime.RoomLayer3Fx.CurrentYPosition, "Special water follows the native one-pixel velocity");
        AssertEqual((ushort)0xb1, samus.LiquidPhysics.FxYPosition, "The moved water surface reaches live Samus physics");
        AssertEqual<RoomFxEarthquakeRequest?>(null, runtime.RoomLayer3Fx.EarthquakeRequest,
            "Water motion does not invoke the lava/acid earthquake branch");
        AssertEqual(0, runtime.RoomLayer3Fx.SoundRequests.Count, "Water motion does not invoke lava/acid sound feedback");
        runtime.RoomLayer3Fx.Step(bus, runtime.Vram, new RoomLayer3FxFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = true, MainGameLoopCarry = true });
        AssertEqual((ushort)0xb1, runtime.RoomLayer3Fx.CurrentYPosition, "Frozen HDMA retains the current water surface");
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

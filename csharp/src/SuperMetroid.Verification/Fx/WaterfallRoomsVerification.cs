using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.Core;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Fx;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
/// <summary>Checks installed waterfall room color math and combined BG2 subscreen capture.</summary>
internal static class WaterfallRoomsVerification
{
    /// <summary>Loads both reported rooms, confirms native TM/TS/CGADSUB values, and checks BG2 joins the subscreen without a second overlay.</summary>
    internal static void VerifyWaterfallRooms()
    {
        var installation = RepositoryInstallation.Installation;
        var bus=installation.OpenRuntimeAddressSpace();
        var game=new SuperMetroidGame(new SuperMetroid.Core.Saves.SaveSlotStore());
        InstalledFixtures.BindGame(game);
        game.Session.CreateGameplayRuntime(GameplayRuntimePurpose.SelectedGame);
        var runtime = game.Session.GameplayRuntime;
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebesAwake);
        foreach (ushort room in new ushort[] { 0xd72a, 0xd913 })
        {
            runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Get(room));
            if (runtime.RoomLayer3Fx.LayerBlendConfiguration != LayerBlendingConfiguration.WaterfallSubtractive)
                throw new InvalidDataException($"Reported waterfall room {room:X4} lost configuration $16.");
            runtime.ScreenLayers.Choose(runtime.Room.LoadedScope);
            runtime.RoomLayer3Fx.Step(runtime.Room.LoadedScope.ArtworkFrames, new RoomLayer3FxFrame { Camera = runtime.Room.LoadedCamera.PixelPosition, TimeIsFrozen = false, MainGameLoopCarry = true });
            runtime.FrameBoundary.BeginAcceptedFrame(0);
            AssertEqual(ScreenLayers.Bg1 | ScreenLayers.Sprites, runtime.ScreenLayers.Displayed, "waterfall main-screen layers");
            var snapshot = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame())
                ?? throw new InvalidDataException("Waterfall room capture is absent.");
            var math = snapshot.Layers[0] as GameplayColorMathRenderLayer
                ?? throw new InvalidDataException("Waterfall room still captures an ordinary opaque background.");
            AssertTrue(math.SubscreenUsesBg2, "waterfall capture admits BG2 subscreen");
            AssertEqual(ColorBlend.Bg1 | ColorBlend.Sprites | ColorBlend.Backdrop | ColorBlend.Subtract, math.Blend.Layers,
                "the waterfall subtracts from the foreground, the sprites and the backdrop, unhalved");
            AssertEqual(ScreenLayers.Bg1 | ScreenLayers.Sprites,
                math.Gameplay.View.MainScreenLayers, "waterfall main excludes BG2");
            AssertTrue(!snapshot.Layers.Any(layer => layer is FxPlaneBlendRenderLayer),
                "waterfall has no second post-composition liquid overlay");
            var pixels = DisplayRendering.Render(snapshot);
            AssertEqual(256 * 224, pixels.Length, "reported room composes full frame");
            Console.WriteLine($"Waterfall room {room:X4}: TM=11 TS=06 CGADSUB=B1, combined subscreen capture passed.");
        }
    }

    /// <summary>Runs the focused two-room waterfall composition check.</summary>
    [VerificationCommand("--waterfall-rooms")]
    internal static int WaterfallRoomsCommand()
    {
        Suite(nameof(VerifyWaterfallRooms), () => VerifyWaterfallRooms());
        return 0;
    }
}

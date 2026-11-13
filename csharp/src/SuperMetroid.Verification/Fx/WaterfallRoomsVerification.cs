using SuperMetroid.Core;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Assets;
namespace SuperMetroid.Verification.Fx;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.TestSupport.FrameRendering;
/// <summary>Checks installed waterfall room color math and combined BG2 subscreen capture.</summary>
internal static class WaterfallRoomsVerification
{
    /// <summary>Loads both reported rooms, confirms native TM/TS/CGADSUB values, and checks BG2 joins the subscreen without a second overlay.</summary>
    internal static void VerifyWaterfallRooms()
    {
        var game=new SuperMetroidGame(new SuperMetroid.Core.Saves.SaveSlotStore());
        InstalledFixtures.BindGame(game);
        game.Session.CreateGameplayRuntime(GameplayRuntimePurpose.SelectedGame);
        var runtime = game.Session.GameplayRuntime;
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetEvent(StoryEvent.ZebesAwake);
        foreach (ushort room in new ushort[] { 0xd72a, 0xd913 })
        {
            runtime.LoadCartridgeRoomForDebug(RoomDefinitions.Get(room));
            if (runtime.RoomFxOverlay.Blend != RoomEffectBlendDefinitions.WaterfallDarkensBehind)
                throw new InvalidDataException($"Reported waterfall room {room:X4} lost its waterfall blend.");
            runtime.ScreenLayers.Choose(runtime.Room.LoadedScope);
            runtime.RoomFxOverlay.Step(runtime.Room.LoadedScope.ArtworkFrames, new RoomFxOverlayFrame { Camera = runtime.Room.LoadedCamera.PixelPosition, TimeIsFrozen = false, EarthquakeSoundDelay = SuperMetroid.Core.Compatibility.PowerOnEarthquakeSoundDelay.OneFrame });
            runtime.FrameBoundary.BeginAcceptedFrame(0);
            AssertEqual(ScreenLayers.Foreground | ScreenLayers.Sprites, runtime.ScreenLayers.Displayed, "waterfall shown layers");
            var snapshot = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame())
                ?? throw new InvalidDataException("Waterfall room capture is absent.");
            var math = snapshot.Layers[0] as GameplayBlendLayer
                ?? throw new InvalidDataException("Waterfall room still captures an ordinary opaque background.");
            AssertTrue(math.BackgroundBehind && math.OverlayBehind is not null, "the waterfall's background and liquid lie behind what it blends");
            AssertEqual(BlendedLayers.Foreground | BlendedLayers.Actors | BlendedLayers.Backdrop, math.Blend.Blended,
                "the waterfall darkens the foreground, the actors and the backdrop");
            AssertTrue(math.Blend.Operation == BlendOperation.Subtract && !math.Blend.Halved, "the waterfall subtracts unhalved");
            AssertEqual(ScreenLayers.Foreground | ScreenLayers.Sprites,
                math.Gameplay.View.ShownLayers, "the waterfall shows its background only through its blend");
            AssertTrue(!snapshot.Layers.Any(layer => layer is OverlayBlendLayer),
                "waterfall has no second post-composition liquid overlay");
            var pixels = DisplayRendering.Render(snapshot);
            AssertEqual(256 * 224, pixels.Length, "reported room composes full frame");
            Console.WriteLine($"Waterfall room {room:X4}: its background and liquid darken the foreground, the actors and the backdrop.");
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

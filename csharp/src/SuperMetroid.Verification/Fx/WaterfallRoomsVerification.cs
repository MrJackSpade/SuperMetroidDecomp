using SuperMetroid.Core;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rendering;
using System.Reflection;
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
/// <summary>Checks installed waterfall room color math and combined BG2 subscreen capture.</summary>
internal static class WaterfallRoomsVerification
{
    /// <summary>Loads both reported rooms, confirms native TM/TS/CGADSUB values, and checks BG2 joins the subscreen without a second overlay.</summary>
    internal static void VerifyWaterfallRooms()
    {
        const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
        var installation = RepositoryInstallation.Installation;
        var bus=installation.OpenRuntimeAddressSpace();
        var game=new SuperMetroidGame(bus, new SuperMetroid.Core.Saves.SaveSlotStore());
        InstalledFixtures.BindGame(game);
        game.Session.CreateGameplayRuntime(false);
        var runtime=game.Session.runtime;
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebesAwake);
        foreach (ushort room in new ushort[] { 0xd72a, 0xd913 })
        {
            runtime.LoadCartridgeRoomForDebug(room);
            if (runtime.RoomLayer3Fx.LayerBlendConfiguration != LayerBlendingConfiguration.WaterfallSubtractive)
                throw new InvalidDataException($"Reported waterfall room {room:X4} lost configuration $16.");
            runtime.GameplayWindows.PrepareGameplayWindowRegisters();
            runtime.RoomLayer3Fx.Step(runtime.Vram, new RoomLayer3FxFrame { Camera = runtime.Room.LoadedCamera.PixelPosition, TimeIsFrozen = false, MainGameLoopCarry = true });
            runtime.FrameBoundary.BeginAcceptedFrame(0);
            var registers = runtime.GameplayWindows.DisplayedGameplayWindowRegisters;
            AssertEqual((byte)0x11, registers.MainScreen, "waterfall native TM");
            AssertEqual((byte)0x06, runtime.CachedGameplaySubscreen, "waterfall native TS");
            var snapshot = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame())
                ?? throw new InvalidDataException("Waterfall room capture is absent.");
            var math = snapshot.Layers[0] as GameplayColorMathRenderLayer
                ?? throw new InvalidDataException("Waterfall room still captures an ordinary opaque background.");
            AssertTrue(math.SubscreenUsesBg2, "waterfall capture admits BG2 subscreen");
            AssertEqual((byte)0xb1, (byte)math.ColorMath, "waterfall native CGADSUB");
            AssertEqual(SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Obj,
                math.Gameplay.Registers.MainScreenLayers, "waterfall main excludes BG2");
            AssertTrue(!snapshot.Layers.Any(layer => layer is Bg2BppColorMathRenderLayer),
                "waterfall has no second post-composition liquid overlay");
            var pixels = SoftwareLayeredSnapshotRenderer.Render(snapshot);
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

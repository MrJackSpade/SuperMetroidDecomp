using System.Buffers.Binary;
using System.Runtime.InteropServices;
using static SuiteTimingVerification;
using SuperMetroid.Core.Rendering;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
/// <summary>Represents waterfall rooms verification.</summary>
internal static class WaterfallRoomsVerification
{
    /// <summary>verify waterfall rooms.</summary>
    internal static void VerifyWaterfallRooms()
    {
        const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
        var installation = RepositoryInstallation.Installation;
        var bus=installation.OpenRuntimeAddressSpace();
        var game=new SuperMetroidGame(bus);
        InstalledFixtures.BindGame(game);
        typeof(SuperMetroidGame).GetMethod("CreateGameplayRuntime",flags)!.Invoke(game,new object[]{false});
        var runtime=(SuperMetroidRuntime)typeof(SuperMetroidGame).GetField("runtime",flags)!.GetValue(game)!;
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebesAwake);
        foreach (ushort room in new ushort[] { 0xd72a, 0xd913 })
        {
            runtime.LoadCartridgeRoomForDebug(room);
            if (runtime.RoomLayer3Fx.LayerBlendConfiguration != LayerBlendingConfiguration.WaterfallSubtractive)
                throw new InvalidDataException($"Reported waterfall room {room:X4} lost configuration $16.");
            runtime.GameplayWindows.PrepareGameplayWindowRegisters();
            runtime.RoomLayer3Fx.Step(bus, runtime.Vram, new RoomLayer3FxFrame { Camera = runtime.Room.Camera!.PixelPosition, TimeIsFrozen = false, MainGameLoopCarry = true });
            runtime.NmiHandler.RunNmi(0, mainLoopRequestedNmi: true);
            var registers = runtime.GameplayWindows.DisplayedGameplayWindowRegisters;
            AssertEqual((byte)0x11, registers.MainScreen, "waterfall native TM");
            AssertEqual((byte)0x06, runtime.CachedGameplaySubscreen, "waterfall native TS");
            var snapshot = GameplayDisplayCapture.TryCaptureFrame(runtime)
                ?? throw new InvalidDataException("Waterfall room capture is absent.");
            var math = snapshot.Layers[0] as GameplayColorMathRenderLayer
                ?? throw new InvalidDataException("Waterfall room still captures an ordinary opaque background.");
            AssertTrue(math.SubscreenUsesBg2, "waterfall capture admits BG2 subscreen");
            AssertEqual((byte)0xb1, (byte)math.ColorMath, "waterfall native CGADSUB");
            AssertEqual(SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Obj,
                math.Gameplay.Registers.MainScreenLayers, "waterfall main excludes BG2");
            AssertTrue(!snapshot.Layers.ToArray().Any(layer => layer is Bg2BppColorMathRenderLayer),
                "waterfall has no second post-composition liquid overlay");
            var pixels = SoftwareLayeredSnapshotRenderer.Render(snapshot);
            AssertEqual(256 * 224, pixels.Length, "reported room composes full frame");
            Console.WriteLine($"Waterfall room {room:X4}: TM=11 TS=06 CGADSUB=B1, combined subscreen capture passed.");
        }
    }

    /// <summary>waterfall rooms command.</summary>
    [VerificationCommand("--waterfall-rooms")]
    internal static int WaterfallRoomsCommand()
    {
        Suite(nameof(VerifyWaterfallRooms), () => VerifyWaterfallRooms());
        return 0;
    }
}
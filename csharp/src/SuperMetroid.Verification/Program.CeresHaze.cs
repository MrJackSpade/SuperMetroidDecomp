using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyCeresHazeLifecycle()
    {
        var haze = new CeresHazeState();
        haze.Load(true, false, false);
        haze.Step();
        AssertEqual(0, haze.Intensity, "haze waits for room fade-in");
        for (int frame = 0; frame < 16; frame++)
        {
            haze.Step(roomFadeIn: true);
            AssertEqual(frame, haze.Intensity, "native haze fade-in table counter");
            var pixels = new Rgba32[256 * 224];
            SnesGameplayFrameRenderer.ApplyCeresHaze(pixels, haze.IsRed, haze.Intensity);
            for (int band = 0; band < 16; band++)
            {
                int y = band == 0 ? 32 : 64 + (band - 1) * 8;
                int component = Math.Max(0, frame - (15 - band));
                AssertEqual((component << 3) | (component >> 2), pixels[y * 256].B,
                    "native HDMA band at fade frame");
                AssertEqual(0, pixels[y * 256].R, "blue room does not gain red haze");
            }
        }
        haze.Step(); // Counter sixteen changes pre-instruction without rewriting the table.
        haze.Step(roomFadeOut: true);
        AssertEqual(15, haze.Intensity, "fade-out selection retains prior table");
        for (int frame = 16; frame > 0; frame--)
        {
            haze.Step();
            AssertEqual(frame, haze.Intensity, "native fade-out table before decrement");
        }

        var runtime = new SuperMetroidRuntime(SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.CeresRidleyRoom);
        AssertTrue(runtime.CeresHaze.Enabled && !runtime.CeresHaze.IsRed, "Ridley room spawns blue haze");
        runtime.System.SetBossBits(runtime.ActiveRoom!.AreaIndex, BossBits.AreaBoss);
        for (int frame = 0; frame < 30; frame++) runtime.CeresHaze.Step();
        AssertTrue(!runtime.CeresHaze.IsRed, "defeat cannot replace the current room's HDMA channel");
        runtime.LevelData!.ResolveDoorCollision(runtime.AddressSpace, 0, runtime.Samus!.Pose);
        var transition = new DoorTransitionState();
        var audio = new CartridgeAudioState();
        transition.Begin(runtime);
        bool sawDestinationFade = false;
        for (int frame = 0; transition.IsActive && frame < 600; frame++)
        {
            bool fadingIn = transition.Phase == DoorTransitionPhase.FadeInDestinationPalette;
            transition.Step(runtime, audio, 0);
            if (runtime.ActiveRoom!.Pointer == RoomHeaderPointers.CeresRidleyRoom)
                AssertTrue(!runtime.CeresHaze.IsRed, "source room remains blue through actual door fade");
            if (fadingIn)
            {
                sawDestinationFade = true;
                AssertTrue(runtime.CeresHaze.IsRed, "destination fade uses newly selected red channel");
            }
        }
        AssertTrue(!transition.IsActive && sawDestinationFade, "retail escape door completes its haze fade handoff");
        AssertEqual(RoomHeaderPointers.CeresFinalHallway, runtime.ActiveRoom!.Pointer, "retail exit enters final hallway");
        AssertTrue(runtime.CeresHaze.Enabled && runtime.CeresHaze.IsRed, "next room selects escape red on load");
        Console.WriteLine("Ceres haze: native ramps, source fade and retail room-owned blue/red selection agree.");
    }
}

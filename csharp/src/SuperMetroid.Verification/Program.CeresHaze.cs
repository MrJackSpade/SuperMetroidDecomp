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
    private static void VerifyCeresHazePhaseControl(ISnesAddressSpace rom)
    {
        // Independently transcribed instruction trace from pinned bank_88.asm,
        // checked against the supported cartridge before comparing production state.
        AssertEqual((ushort)0xde2d, ReadVerificationWord(rom, 0x88de28), "Waiting selects fade-in");
        AssertEqual((byte)0x9d, rom.ReadByte(0x88de2a), "Selection stores pre-instruction");
        AssertEqual((byte)0xa0, rom.ReadByte(0x88de2d), "Fade-in immediately follows selection, without RTL");
        AssertEqual((ushort)16, ReadVerificationWord(rom, 0x88de43), "Native fade-in stop");
        AssertEqual((byte)0xfe, rom.ReadByte(0x88de69), "Fade-in increments after table write");
        AssertEqual((ushort)0xde74, ReadVerificationWord(rom, 0x88de6e), "Counter16 selects holding");
        AssertEqual((ushort)0xde96, ReadVerificationWord(rom, 0x88de90), "Holding selects fade-out");
        AssertEqual((byte)0x6b, rom.ReadByte(0x88de95), "Fade-out selection returns before writing");
        AssertEqual((byte)0xf0, rom.ReadByte(0x88deab), "Zero fade-out counter branches before table write");
        AssertEqual((byte)0xde, rom.ReadByte(0x88decf), "Fade-out decrements after table write");
        // Visible last-written amplitudes, not the next counter. These are original
        // trace evidence; do not replace them with the production counter formula.
        int[] entering = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15];
        int[] leaving = [16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1];
        foreach (bool dead in new[] { false, true })
        foreach (bool immediate in new[] { false, true })
        {
            var haze = new CeresHazeState();
            haze.Load(true, dead, immediate);
            AssertEqual(immediate ? 15 : 0, haze.Intensity, "Load projects initial/completed fade state");
            if (!immediate)
            {
                haze.Step(roomFadeOut: true);
                haze.Step();
                AssertEqual(0, haze.Intensity, "Waiting ignores fade-out and absent fade-in");
                foreach (int expected in entering)
                {
                    haze.Step(roomFadeIn: expected == 0, roomFadeOut: true);
                    AssertEqual(expected, haze.Intensity, "Native fade-in ignores further transition signals");
                }
                haze.Step(roomFadeOut: true);
                AssertEqual(15, haze.Intensity, "Counter16 selects holding without writing or consuming fade-out");
            }
            haze.Step(roomFadeIn: true);
            AssertEqual(15, haze.Intensity, "Holding ignores fade-in");
            haze.Step(roomFadeOut: true);
            AssertEqual(15, haze.Intensity, "Fade-out selection retains previous table for one call");
            foreach (int expected in leaving)
            {
                haze.Step(roomFadeIn: true, roomFadeOut: true);
                AssertEqual(expected, haze.Intensity, "Native fade-out writes before decrement");
            }
            haze.Step();
            haze.Step(roomFadeIn: true, roomFadeOut: true);
            AssertEqual(1, haze.Intensity, "Zero counter retains final table without restarting");
            AssertEqual(dead, haze.IsRed, "Selected channel is room-owned throughout the trace");
            AssertTrue(haze.Enabled, "Fade completion does not disable the object");
            haze.Load(false, !dead, immediate);
            haze.Step(roomFadeIn: true, roomFadeOut: true);
            AssertEqual(immediate ? 15 : 0, haze.Intensity, "Disabled projection cannot advance");
            AssertTrue(!haze.Enabled && haze.IsRed == !dead, "Reload replaces enable/channel state");
        }
    }

    private static void VerifyCeresHazeLifecycle()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SuperMetroid.AssetExtraction.SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres haze oracle revision");
        VerifyCeresHazeNativeRamp(rom);
        VerifyCeresHazePhaseControl(rom);

        var runtime = CreateRetailRuntimeFixture(rom);
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

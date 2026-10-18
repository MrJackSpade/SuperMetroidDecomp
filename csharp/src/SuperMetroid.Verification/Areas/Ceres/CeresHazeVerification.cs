using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Areas.Ceres.LookupCeresHazeRampVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>Checks native Ceres haze ramp control and room-owned blue/red handoff.</summary>
internal static class CeresHazeVerification
{
    /// <summary>Compares waiting, fade, and hold writes against the pinned bank-$88 instruction trace.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Ceres haze phase control")]
    internal static void VerifyCeresHazePhaseControl(ISnesAddressSpace rom)
    {
        // Independently transcribed instruction trace from pinned bank_88.asm,
        // checked against the supported cartridge before comparing production state.
        AssertEqual((ushort)0xde2d, ReadVerificationWord(rom, 0x88de28), "Waiting selects fade-in");
        AssertEqual((byte)0x9d, rom.ReadCartridgeByte(0x88de2a), "Selection stores pre-instruction");
        AssertEqual((byte)0xa0, rom.ReadCartridgeByte(0x88de2d), "Fade-in immediately follows selection, without RTL");
        AssertEqual((ushort)16, ReadVerificationWord(rom, 0x88de43), "Native fade-in stop");
        AssertEqual((byte)0xfe, rom.ReadCartridgeByte(0x88de69), "Fade-in increments after table write");
        AssertEqual((ushort)0xde74, ReadVerificationWord(rom, 0x88de6e), "Counter16 selects holding");
        AssertEqual((ushort)0xde96, ReadVerificationWord(rom, 0x88de90), "Holding selects fade-out");
        AssertEqual((byte)0x6b, rom.ReadCartridgeByte(0x88de95), "Fade-out selection returns before writing");
        AssertEqual((byte)0xf0, rom.ReadCartridgeByte(0x88deab), "Zero fade-out counter branches before table write");
        AssertEqual((byte)0xde, rom.ReadCartridgeByte(0x88decf), "Fade-out decrements after table write");
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

    /// <summary>Exercises a retail door transition from Ridley's blue haze to the escape hallway's red haze.</summary>
    internal static void VerifyCeresHazeLifecycle()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SuperMetroid.AssetExtraction.Cartridge.SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres haze oracle revision");
        Suite(nameof(VerifyCeresHazeNativeRamp), () => VerifyCeresHazeNativeRamp(rom));
        Suite(nameof(VerifyCeresHazePhaseControl), () => VerifyCeresHazePhaseControl(rom));

        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.CeresRidley);
        AssertTrue(runtime.CeresHaze.Enabled && !runtime.CeresHaze.IsRed, "Ridley room spawns blue haze");
        runtime.System.SetBossBits(runtime.Room.LoadedRoom.AreaIndex, BossBits.AreaBoss);
        for (int frame = 0; frame < 30; frame++) runtime.CeresHaze.Step();
        AssertTrue(!runtime.CeresHaze.IsRed, "defeat cannot replace the current room's HDMA channel");
        runtime.Room.LoadedLevelData.ResolveDoorCollision(0, runtime.Player.IntroducedSamus.Pose);
        var transition = new DoorTransitionState();
        var audio = new CartridgeAudioState();
        transition.Begin(runtime);
        bool sawDestinationFade = false;
        for (int frame = 0; transition.IsActive && frame < 600; frame++)
        {
            bool fadingIn = transition.Phase == DoorTransitionPhase.FadeInDestinationPalette;
            transition.Step(runtime, audio, 0, SuperMetroid.Core.Rooms.Doors.LagFreeDoorLoaderProgress.Instance);
            if (runtime.Room.LoadedRoom.Definition == RoomHeaderDefinitions.CeresRidley)
                AssertTrue(!runtime.CeresHaze.IsRed, "source room remains blue through actual door fade");
            if (fadingIn)
            {
                sawDestinationFade = true;
                AssertTrue(runtime.CeresHaze.IsRed, "destination fade uses newly selected red channel");
            }
        }
        AssertTrue(!transition.IsActive && sawDestinationFade, "retail escape door completes its haze fade handoff");
        AssertEqual(RoomHeaderDefinitions.Room58Escape, runtime.Room.LoadedRoom.Definition, "retail exit enters final hallway");
        AssertTrue(runtime.CeresHaze.Enabled && runtime.CeresHaze.IsRed, "next room selects escape red on load");
        Console.WriteLine("Ceres haze: native ramps, source fade and retail room-owned blue/red selection agree.");
    }
}

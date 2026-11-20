using SuperMetroid.Core.Fx;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Poses;
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
        // The fade-out's first update shows full strength, not one step past it (#2573).
        int[] leaving = [15, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1];
        foreach (bool dead in new[] { false, true })
        foreach (bool immediate in new[] { false, true })
        {
            var haze = new CeresHazeState(dead, immediate);
            AssertEqual(immediate ? 15 : 0, haze.Strength.FadeSteps, "Load projects initial/completed fade state");
            if (!immediate)
            {
                haze.StepWhileRoomFadesOut();
                haze.Step();
                AssertEqual(0, haze.Strength.FadeSteps, "Waiting ignores fade-out and absent fade-in");
                foreach (int expected in entering)
                {
                    if (expected == 0) haze.StepWhileRoomFadesIn();
                    else haze.StepWhileRoomFadesOut();
                    AssertEqual(expected, haze.Strength.FadeSteps, "Native fade-in ignores further transition signals");
                }
                haze.StepWhileRoomFadesOut();
                AssertEqual(15, haze.Strength.FadeSteps, "Counter16 selects holding without writing or consuming fade-out");
            }
            haze.StepWhileRoomFadesIn();
            AssertEqual(15, haze.Strength.FadeSteps, "Holding ignores fade-in");
            haze.StepWhileRoomFadesOut();
            AssertEqual(15, haze.Strength.FadeSteps, "Fade-out selection retains previous table for one call");
            foreach (int expected in leaving)
            {
                haze.StepWhileRoomFadesIn();
                AssertEqual(expected, haze.Strength.FadeSteps, "the fade-out shows its counter, within the haze's range");
            }
            haze.Step();
            haze.StepWhileRoomFadesIn();
            AssertEqual(1, haze.Strength.FadeSteps, "Zero counter retains final table without restarting");
            AssertEqual(dead ? CeresHazeTint.AfterRidleyDies : CeresHazeTint.WhileRidleyLives, haze.Tint, "Selected channel is room-owned throughout the trace");
            haze = new CeresHazeState(!dead, immediate);
            AssertTrue(haze.Tint == (dead ? CeresHazeTint.WhileRidleyLives : CeresHazeTint.AfterRidleyDies), "Reload replaces the channel");
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
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.CeresRidley);
        AssertTrue(runtime.Room.LoadedScope.CeresHaze is { Tint: CeresHazeTint.WhileRidleyLives }, "Ridley room spawns blue haze");
        runtime.System.RecordBossesDefeated(runtime.Room.LoadedRoom.Area, DefeatedBosses.AreaBoss);
        for (int frame = 0; frame < 30; frame++) runtime.CeresHaze.Step();
        AssertTrue(runtime.CeresHaze.Tint == CeresHazeTint.WhileRidleyLives, "defeat cannot replace the current room's HDMA channel");
        runtime.MeetDoorBlock(runtime.Room.LoadedLevelData.DoorBlockLeadingThrough(RoomDoorListDefinitions.ByRoom[RoomDefinitions.CeresRidley][0]), runtime.Player.IntroducedSamus.Pose.ForRoomCollision());
        var transition = new DoorTransitionState();
        var audio = AudioQueues.Standalone();
        transition.Begin(runtime);
        bool sawDestinationFade = false;
        for (int frame = 0; transition.IsActive && frame < 600; frame++)
        {
            bool fadingIn = transition.Phase is DoorTransitionPhase.Running.FadeInDestinationRoom;
            transition.Step(runtime, audio, 0, SuperMetroid.Core.Compatibility.InstantRoomLoad.Instance);
            if (runtime.Room.LoadedRoom.Definition == RoomDefinitions.CeresRidley)
                AssertTrue(runtime.CeresHaze.Tint == CeresHazeTint.WhileRidleyLives, "source room remains blue through actual door fade");
            if (fadingIn)
            {
                sawDestinationFade = true;
                AssertTrue(runtime.CeresHaze.Tint == CeresHazeTint.AfterRidleyDies, "destination fade uses newly selected red channel");
            }
        }
        AssertTrue(!transition.IsActive && sawDestinationFade, "retail escape door completes its haze fade handoff");
        AssertEqual(RoomDefinitions.CeresRidleyHallway, runtime.Room.LoadedRoom.Definition, "retail exit enters final hallway");
        AssertTrue(runtime.Room.LoadedScope.CeresHaze is { Tint: CeresHazeTint.AfterRidleyDies }, "next room selects escape red on load");
        Console.WriteLine("Ceres haze: native ramps, source fade and retail room-owned blue/red selection agree.");
    }
}

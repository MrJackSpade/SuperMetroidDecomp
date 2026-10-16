using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.VerificationImporters;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the complete Mother Brain glass PLM list and its shatter sequence without runtime program reads.</summary>
internal static class MotherBrainGlassPlmProgramVerification
{
    /// <summary>
    /// Compares the entire bounded cartridge list, then drives its real PLM
    /// dispatcher through every glass-break threshold without permitting a ROM
    /// read of the compiled control bytes.
    /// </summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Mother Brain glass PLM program")]
    internal static void VerifyMotherBrainGlassPlmProgram(SuperMetroidAddressSpace rom)
    {
        BossRoomPlmProgramsVerification.VerifyMotherBrainGlassPrograms(rom);

        var source = new TestAddressSpace();
        var bank84 = new byte[0x8000];
        for (int index = 0; index < bank84.Length; index++)
            bank84[index] = rom.ReadCartridgeByte(0x848000 + index);
        source.WriteBytes(0x848000, bank84);
        source.WriteBytes(0x8f9000,
        [
            unchecked((byte)PlmHeaderId.MotherBrainGlass),
            SnesLittleEndian.HighByte((ushort)PlmHeaderId.MotherBrainGlass),
            9, 5, 0, 0x80, 0, 0,
        ]);
        var guarded = NewMotherBrainGlassProgramReadGuard(source);
        const int width = 32;
        var level = new RoomLevelData(new Extent16<RoomBlocks>(width, 16), new RoomLevelPlanes(new ushort[width * 16], new byte[width * 16], new ushort[width * 16]), new byte[0x400 * 8]);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        bool eventSet = false;
        var plms = new RoomPlmSystem();
        AssertEqual(1, plms.PopulationLoader.LoadRoomPopulation(new PlmRoom(guarded, level, streamer, new SnesVram(), new Bank80SystemState(), AreaId.Tourian), RoomPlmPopulationImporter.Read(guarded, 0x9000), new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState()), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false, IsBossDefeated = () => false, IsMiniBossDefeated = () => false }, Events = new FixtureEventFlags { HasEvent = _ => eventSet, SetEvent = eventNumber =>
            {
                AssertEqual(EventNumber.MotherBrainGlassDestroyed, eventNumber,
                    "glass completion sets the native break event");
                eventSet = true;
            } } }), "glass PLM loads for complete program test");

        // With no head hits, the first one-frame draw must loop at the $0002
        // threshold instead of shattering early. Both passes use the guarded
        // production interpreter, not an independent table reader.
        plms.Step(level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertTrue(plms.MotherBrainGlassAnimation is MotherBrainGlassPlmAnimation { Cursor: { Program: MotherBrainGlassPlmProgram.Stage0, Position: 1 } },
            "intact glass first draw advances to the hit threshold");
        plms.Step(level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertTrue(plms.MotherBrainGlassAnimation is MotherBrainGlassPlmAnimation { Cursor: { Program: MotherBrainGlassPlmProgram.Stage0, Position: 1 } },
            "zero head hits loop at the first glass stage");
        AssertTrue(!eventSet && plms.MotherBrainGlass.MotherBrainGlassProjectileRequests.Count == 0,
            "unhit glass emits neither event nor shards");

        // Head-shot AI can increment the native room argument before a PLM
        // pass. Advance it through the final $12 threshold, then let each
        // one-frame draw and each authored burst execute in production order.
        for (int shot = 0; shot < 18; shot++)
            plms.MotherBrainGlass.IncrementMotherBrainGlassRoomArgument();
        int emitted = 0;
        for (int frame = 0; frame < 128 && plms.BombTorizoHand.HasActiveHeader(PlmHeaderId.MotherBrainGlass); frame++)
        {
            plms.Step(level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
            emitted += plms.MotherBrainGlass.MotherBrainGlassProjectileRequests.Count;
        }
        AssertTrue(eventSet && !plms.BombTorizoHand.HasActiveHeader(PlmHeaderId.MotherBrainGlass),
            $"all glass stages end by setting the event and deleting the PLM; " +
            $"program={plms.MotherBrainGlassAnimation?.RunningProgram}, " +
            $"timer={plms.MotherBrainGlassInstructionTimer}");
        AssertEqual(40, emitted,
            "five authored shatter stages emit four shards twice each");
        AssertEqual(18, plms.MotherBrainGlassRoomArgument,
            "head-shot count survives PLM deletion");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "complete glass shatter never rereads compiled program bytes");
        Console.WriteLine("  Mother Brain glass PLM: all program bytes match ROM; full shatter runs without ROM instruction reads.");
    }

    /// <summary>Guards the compiled bank $84 glass-control span while allowing fixture data reads.</summary>
    private static CartridgeReadGuard NewMotherBrainGlassProgramReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0x84d202 and <= 0x84d2f8,
                address => $"Mother Brain glass reread compiled program byte ${address:X6}.");
}

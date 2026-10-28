using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Rooms.Plms.CompiledDynamicCollectibleGraphicsVerification;
using static SuperMetroid.Verification.Rooms.Plms.CompiledScrollPlmProgramsVerification;
using static SuperMetroid.Verification.Rooms.Plms.LookupRetailPlmHeadersVerification;
using static SuperMetroid.Verification.Rooms.LookupRetailPopulationsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks compiled room PLM populations, header metadata, scroll programs, and collectible uploads against ROM.</summary>
internal static class RoomPlmPopulationDefinitionsVerification
{
    /// <summary>Runs the grouped cartridge comparisons for room PLM definitions and loading behavior.</summary>
    internal static void VerifyCompiledRoomPlmPopulationDefinitions(
        SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyRetailPopulationMappings), () => VerifyRetailPopulationMappings(rom));

        Suite(nameof(VerifyRetailPlmHeaders), () => VerifyRetailPlmHeaders(rom));
        Suite(nameof(VerifyCompiledRoomPlmHeaderLoad), () => VerifyCompiledRoomPlmHeaderLoad(rom));
        Suite(nameof(VerifyCompiledRoomScrollPrograms), () => VerifyCompiledRoomScrollPrograms(rom));
        Suite(nameof(VerifyCompiledDynamicCollectibleGraphics), () => VerifyCompiledDynamicCollectibleGraphics(rom));
        Console.WriteLine("  PLM populations, headers, scroll programs and collectible uploads match ROM.");
    }

    /// <summary>Compares native and compiled population slots and setup results while guarding source population and header reads.</summary>
    internal static void VerifyCompiledRoomPlmHeaderLoad(SuperMetroidAddressSpace rom)
    {
        const ushort populatedPointer = 0x8000;
        int sourceLength = RoomPlmPopulationDefinitions.Get(populatedPointer).Placements.Length * 6 + 2;
        int forbiddenHeaderReadAttempts = 0;
        var guarded = NewRoomPlmPopulationReadGuard(rom, populatedPointer,
            sourceLength, headerRead: () => forbiddenHeaderReadAttempts++);
        (int compiledCount, RoomPlmSlotSnapshot[] compiledSlots) = Load(guarded,
            populatedPointer, useCompiled: true);
        (int nativeCount, RoomPlmSlotSnapshot[] nativeSlots) = Load(rom,
            populatedPointer, useCompiled: false);
        AssertEqual(nativeCount, compiledCount,
            "compiled retail PLM load preserves sequential spawn count");
        // Typed programs are separate objects per load; compare what they run.
        AssertTrue(nativeSlots.Select(slot => slot with { Animation = null })
                .SequenceEqual(compiledSlots.Select(slot => slot with { Animation = null })) &&
            nativeSlots.Select(slot => new VerifyCompiledRoomPlmHeaderLoadCase(slot.Animation?.GetType(), slot.Animation?.RunningProgram))
                .SequenceEqual(compiledSlots.Select(slot => new VerifyCompiledRoomPlmHeaderLoadCase(slot.Animation?.GetType(), slot.Animation?.RunningProgram))),
            "compiled retail PLM load preserves native slots and setup results");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "compiled retail PLM loader never reads bank-$8F population bytes");
        AssertEqual(0, forbiddenHeaderReadAttempts,
            "compiled retail PLM loader never reads bank-$84 header metadata");

        (int emptyCount, RoomPlmSlotSnapshot[] emptySlots) = Load(
            NewRoomPlmPopulationReadGuard(rom, 0x8058, 2),
            0x8058, useCompiled: true);
        AssertEqual(0, emptyCount, "compiled empty PLM population terminates");
        AssertEqual(0, emptySlots.Length, "compiled empty PLM population leaves no slots");
    }

    /// <summary>Loads one PLM population into a fresh room and returns its spawn count and resulting slots.</summary>
    private static RoomPlmPopulationDefinitionsLoadResult Load(
        ISnesAddressSpace bus, ushort pointer, bool useCompiled)
    {
        const int width = 64;
        const int height = 64;
        var level = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(new ushort[width * height], new byte[width * height], new ushort[width * height]), new BackgroundTile[0x400 * 4]);
        var plms = PlmFixtures.Unbound();
        int count = PlmFixtures.LoadPopulation(ref plms, new PlmRoom(level, BgArtworkFixtures.For(new SnesVram()), new SharedGameState(), AreaId.Crateria), (useCompiled ? RoomPlmPopulationDefinitions.Get(pointer) : RoomPlmPopulationImporter.Read(bus, pointer)), new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState()), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false, IsBossDefeated = () => false, IsMiniBossDefeated = () => false }, Events = new FixtureEventFlags { HasEvent = _ => false, SetEvent = _ => { } } });
        return new RoomPlmPopulationDefinitionsLoadResult(count, plms.PopulationSlots.ToArray());
    }

    /// <summary>Rejects reads of the compiled bank-$8F population and, when observed, bank-$84 header metadata.</summary>
    private static CartridgeReadGuard NewRoomPlmPopulationReadGuard(
        ISnesAddressSpace source, ushort pointer, int length, Action? headerRead = null)
    {
        int first = 0x8f0000 | pointer;
        var guard = new CartridgeReadGuard(source).Forbid(address => address >= first && address < first + length,
            address => $"Retail PLM population was reread at ${address:X6}.");
        if (headerRead is null)
            return guard;
        bool IsHeader(int address) => RoomPlmHeaderDefinitionsTooling.All.Any(header =>
            address >= (0x840000 | PlmNativeWords.Word(header.Header)) && address < (0x840000 | PlmNativeWords.Word(header.Header)) + 4);
        return guard.Observe(IsHeader, _ => headerRead())
            .Forbid(IsHeader, address => $"Retail PLM header was reread at ${address:X6}.");
    }
}

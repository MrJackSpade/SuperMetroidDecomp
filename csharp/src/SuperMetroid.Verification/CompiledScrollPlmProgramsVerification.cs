using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Spatial;
using static LookupScrollProgramsVerification;
using static SuiteTimingVerification;

/// <summary>Checks compiled room-scroll PLM programs through both catalog lookup and retail execution.</summary>
internal static class CompiledScrollPlmProgramsVerification
{
    /// <summary>
    /// Confirms the $8F:8230 resident trigger at (8,13) turns storage cell zero green
    /// without rereading its compiled $8F:94FA program from cartridge data.
    /// </summary>
    internal static void VerifyCompiledRoomScrollPrograms(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyScrollProgramIdentities), () => VerifyScrollProgramIdentities(rom));
        Suite(nameof(VerifyScrollProgramIndices), () => VerifyScrollProgramIndices(rom));
        Suite(nameof(VerifyScrollProgramStates), () => VerifyScrollProgramStates(rom));
        Suite(nameof(VerifyScrollProgramTermination), () => VerifyScrollProgramTermination(rom));

        // Retail population $8F:8230 contains one resident $B703 trigger at
        // (8,13). Its $8F:94FA program changes storage cell zero to green.
        // Guard every compiled retail program range during the actual room PLM
        // load/touch/handler path. Constructed-room programs are exercised by
        // VerifyRoomScrollPlms and supply decoded pairs before the handler runs.
        var guarded = NewRetailScrollProgramReadGuard(rom);
        const int width = 64;
        var level = new RoomLevelData(new Extent16<RoomBlocks>(width, 64), new RoomLevelPlanes(new ushort[width * 64], new byte[width * 64], new ushort[width * 64]), new byte[0x400 * 8]);
        var plms = new RoomPlmSystem();
        AssertEqual(1, plms.PopulationLoader.LoadRoomPopulation(new PlmRoom(guarded, level, level.CreateBackgroundStreamer(), new SnesVram(), new Bank80SystemState(), AreaId.Crateria), RoomPlmPopulationDefinition.FromCompiled(0x8230), new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState()), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
            "retail scroll-only population loads from compiled placement");
        RoomScrollGrid scrolls = RoomScrollGrid.CreateImplicit(guarded,
            size: new Extent16<RoomScreens>(4, 4),
            lastRowState: RoomScrollState.RedBoundary);
        scrolls.SetStorage(0, RoomScrollState.RedBoundary);
        AssertTrue(plms.Scrolls.TryNotifyScrollTouch(level.GetBlockIndex(new(8, 13))),
            "retail scroll trigger wakes at its authored coordinate");
        plms.Step(guarded, level, level.CreateBackgroundStreamer(), new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 }, scrolls);
        AssertEqual((byte)RoomScrollState.Green, scrolls.ReadStorage(0),
            "compiled retail scroll program mutates the intended cell");
        AssertTrue(!plms.ScrollPlmTriggered(level.GetBlockIndex(new(8, 13))),
            "retail scroll trigger returns to resident sleep");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "retail scroll PLM did not read any compiled program from the ROM bus");
    }

    /// <summary>Preserves cross-room WRAM storage while rejecting reads of compiled cartridge program ranges.</summary>
    private static CartridgeReadGuard NewRetailScrollProgramReadGuard(ISnesAddressSpace source) =>
        new MutableCartridgeReadGuard(source)
            .Forbid(address => RoomPlmScrollProgramDefinitions.Pointers.Any(pointer =>
                    address >= (0x8f0000 | pointer) &&
                    address < (0x8f0000 | pointer) + RoomPlmScrollProgramDefinitions.Get(pointer).Length),
                address => $"Retail scroll PLM reread compiled program byte ${address:X6}.");
}

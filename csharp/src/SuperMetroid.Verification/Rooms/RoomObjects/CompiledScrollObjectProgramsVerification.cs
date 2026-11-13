using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Rooms.Scrolling.LookupScrollProgramsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Rooms.RoomObjects;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks compiled room-scroll PLM programs through both catalog lookup and retail execution.</summary>
internal static class CompiledScrollObjectProgramsVerification
{
    /// <summary>
    /// Confirms the $8F:8230 resident trigger at (8,13) turns storage cell zero green
    /// without rereading its compiled $8F:94FA program from cartridge data.
    /// </summary>
    internal static void VerifyCompiledRoomScrollPrograms(CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyScrollProgramIdentities), () => VerifyScrollProgramIdentities(rom));
        Suite(nameof(VerifyScrollProgramIndices), () => VerifyScrollProgramIndices(rom));
        Suite(nameof(VerifyScrollProgramStates), () => VerifyScrollProgramStates(rom));
        Suite(nameof(VerifyScrollProgramTermination), () => VerifyScrollProgramTermination(rom));

        // Retail population $8F:8230 contains one resident $B703 trigger at
        // (8,13). Its $8F:94FA program changes storage cell zero to green.
        // Guard every compiled retail program range during the actual room PLM
        // load/touch/handler path. Constructed-room programs are exercised by
        // VerifyRoomScrollObjects and supply decoded pairs before the handler runs.
        var guarded = NewRetailScrollProgramReadGuard(rom);
        const int width = 64;
        var level = new RoomLevelData(new Extent16<RoomBlocks>(width, 64), new RoomLevelPlanes(new RoomTerrainBlock[width * 64], new RoomBlockBehavior[width * 64], new RoomBlockAppearance[width * 64]), RoomLevelData.NothingBeyondGrid());
        var roomObjects = RoomObjectFixtures.Unbound();
        AssertEqual(1, RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(level, BgArtworkFixtures.For(new SnesVram()), new SharedGameState(new SharedRandom()), AreaId.Crateria), RoomObjectPopulationDefinitions.Get(0x8230), new RoomObjectServices { Samus = new FixtureRoomSamus(() => new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
            "retail scroll-only population loads from compiled placement");
        RoomScrollGrid scrolls = RoomScrollGrid.CreateImplicit(RoomScrollGrid.PowerOnCells(),
            size: new Extent16<RoomScreens>(4, 4),
            lastRowState: RoomScrollState.RedBoundary);
        scrolls.SetCell(0, RoomScrollState.RedBoundary);
        AssertTrue(roomObjects.Scrolls.TryNotifyScrollTouch(new(8, 13)),
            "retail scroll trigger wakes at its authored coordinate");
        roomObjects.Step(level, scrolls);
        AssertEqual(RoomScrollState.Green, scrolls.ReadState(0),
            "compiled retail scroll program mutates the intended cell");
        AssertTrue(!roomObjects.ScrollObjectTriggered(new(8, 13)),
            "retail scroll trigger returns to resident sleep");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "retail scroll PLM did not read any compiled program from the ROM bus");
    }

    /// <summary>Preserves cross-room WRAM storage while rejecting reads of compiled cartridge program ranges.</summary>
    private static CartridgeReadGuard NewRetailScrollProgramReadGuard(ISnesAddressSpace source) =>
        new MutableCartridgeReadGuard(source)
            .Forbid(address => RoomObjectScrollChangeDefinitions.Pointers.Any(pointer =>
                    address >= (0x8f0000 | pointer) &&
                    address < (0x8f0000 | pointer) + RoomObjectScrollChangeDefinitions.Get(pointer).Length),
                address => $"Retail scroll PLM reread compiled program byte ${address:X6}.");
}

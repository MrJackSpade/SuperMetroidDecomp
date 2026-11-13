using SuperMetroid.Verification.Rooms.RoomObjects;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Areas.Tourian;
using SuperMetroid.Verification.Harness;

/// <summary>Checks which Tourian access floor each native spawn starts, and the spawned object's place and change.</summary>
internal static class TourianAccessObjectDefinitionsVerification
{
    /// <summary>Compares native spawn operands, then confirms both states allocate the highest free slot at the fixed access block.</summary>
    internal static void VerifyTourianAccessObjectDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyTourianAccessSpawnField), () => VerifyTourianAccessSpawnField(rom));
        Suite(nameof(VerifyDefinition), () => VerifyDefinition(clear: false));
        Suite(nameof(VerifyDefinition), () => VerifyDefinition(clear: true));

        Console.WriteLine(
            "Tourian access PLMs: both native spawns start their floor's list, and real first-place spawns make their floor's change.");

        void VerifyDefinition(bool clear)
        {
            TerrainChange expected = TourianAccessFloors.ForState(clear ? TourianAccessFloor.Clear : TourianAccessFloor.Crumble);
            const int width = 16;
            const int height = 16;
            int blockCount = width * height;
            var level = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(new RoomTerrainBlock[blockCount], new RoomBlockBehavior[blockCount], new RoomBlockAppearance[blockCount]), RoomLevelData.NothingBeyondGrid());
            var roomObjects = RoomObjectFixtures.Unbound();

            AssertTrue(
                roomObjects.Tourian.TrySpawnTourianAccess(level, clear ? TourianAccessFloor.Clear : TourianAccessFloor.Crumble),
                $"Tourian access {(clear ? "clear" : "crumble")} actor allocates");
            AssertEqual(1, roomObjects.SlotTable.ActiveCount, "Tourian access spawn occupies one PLM slot");
            RoomObjectSlotSnapshot slot = roomObjects.PopulationSlots.Single();
            AssertEqual(0, slot.ProcessingPosition, "Tourian access takes the first free slot in processing order");
            AssertEqual(new Position16<RoomBlocks>(6, 12), slot.Block, "Tourian access fixed block coordinate");
            AssertTrue(slot.Behaviour is TerrainChangeObject { Change: var change } && change == expected,
                "Tourian access starts its terrain change");
            AssertEqual(1, expected.FirstFrameDelay, "Tourian access draws on its first update");
        }
    }

    /// <summary>
    /// Reads the native crumble and already-unlocked spawn operands, including their fixed block coordinates, and checks that
    /// each starts the list of the floor state the port spawns for it. The port gives the access objects no identity (#1622),
    /// so the native header is identified by the list it starts rather than compared with a Core identity.
    /// </summary>
    private static void VerifyTourianAccessSpawnField(SuperMetroidAddressSpace rom)
    {
        // Native spawn operands: completed descent crumbles; already-unlocked room clears.
        TourianAccessObjectDefinitionsCase[] cases = [new TourianAccessObjectDefinitionsCase(false,0x88dc9f), new TourianAccessObjectDefinitionsCase(true,0x88db99)];
        foreach (var item in cases)
        {
            ushort header = CartridgeImportSource.Require(rom).ReadCartridgeWord(item.SpawnOperand);
            ushort list = CartridgeImportSource.Require(rom).ReadCartridgeWord(0x840000 | (header + 2));
            AssertEqual(item.Clear ? TourianAccessFloor.Clear : TourianAccessFloor.Crumble,
                RoomObjectNativeListAddresses.TourianAccessEntries.Single(entry => entry.Entry == list).Program,
                $"Tourian native spawn clear={item.Clear} starts its floor's list");
            AssertEqual((byte)6, rom.ReadCartridgeByte(item.SpawnOperand - 2), "Tourian native spawn column");
            AssertEqual((byte)12, rom.ReadCartridgeByte(item.SpawnOperand - 1), "Tourian native spawn row");
        }
    }

    /// <summary>Runs the Tourian access spawn definition comparison with a retail cartridge oracle.</summary>
    [VerificationCommand("--tourian-access-definitions")]
    internal static int TourianAccessDefinitionsCommand()
    {
        Suite(nameof(VerifyTourianAccessObjectDefinitions), () => VerifyTourianAccessObjectDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}

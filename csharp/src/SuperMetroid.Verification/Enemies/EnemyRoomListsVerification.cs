using SuperMetroid.AssetExtraction.Enemies;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.AssetExtraction.World.RoomStates;

/// <summary>Checks compiled enemy populations and graphics sets against retail room-state references.</summary>
internal static class EnemyRoomListsVerification
{
    /// <summary>The Reflec's bank-$A0 header, whose artwork two graphics sets load though no picture shows it (#1686).</summary>
    private const ushort ReflecHeader = 0xdbff;

    /// <summary>Compares every ordered enemy placement, death quota, and graphics-set member to the cartridge.</summary>
    internal static void VerifyCompiledEnemyRoomLists()
    {
        ISnesAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        ushort[] populationPointers = RoomStateDefinitions.All
            .Select(state => RoomEnemyPopulationDefinitions.PointerOf(state.EnemyPopulation)).Distinct().Order().ToArray();
        ushort[] graphicsPointers = RoomStateDefinitions.All
            .Select(state => RoomEnemyGraphicsSetDefinitions.PointerOf(RoomEnemyGraphicsSetLists.Of(state))).Distinct().Order().ToArray();
        // A population the cartridge reads from another's stored list (the Mother Brain room's once her area's boss is dead
        // reads the Draygon room's empty one, #2574) is compiled as its own definition: the definitions address exactly the
        // stored lists the room states name, and each holds what the list it reads holds.
        AssertEqual(RoomEnemyPopulationDefinitions.Definitions.Select(RoomEnemyPopulationDefinitions.PointerOf).Distinct().Count(), populationPointers.Length,
            "all retail population identities are compiled");
        foreach (RoomEnemyPopulationDefinition definition in RoomEnemyPopulationDefinitions.Definitions)
        {
            RoomEnemyPopulationDefinition stored = RoomEnemyPopulationDefinitions.Get(RoomEnemyPopulationDefinitions.PointerOf(definition));
            AssertTrue(definition.GetType() == stored.GetType() && definition.Placements.Span.SequenceEqual(stored.Placements.Span) &&
                    (definition is not PlacedRoomEnemyPopulationDefinition placed || placed.DeathQuota == ((PlacedRoomEnemyPopulationDefinition)stored).DeathQuota),
                $"population {definition.Name} holds what its stored list holds");
        }
        AssertEqual(RoomEnemyPopulationDefinitions.Definitions.Length,
            RoomEnemyPopulationDefinitions.Definitions.Select(list => list.Name).Distinct().Count(),
            "every compiled population has its own name");
        AssertEqual(RoomEnemyGraphicsSetLists.ListCount, graphicsPointers.Length,
            "all retail graphics-set identities are compiled");
        AssertTrue(populationPointers.SequenceEqual(RoomEnemyPopulationDefinitions.Pointers),
            "compiled population identities exactly match room states");
        AssertTrue(graphicsPointers.SequenceEqual(RoomEnemyGraphicsSetDefinitions.Pointers),
            "compiled graphics-set identities exactly match room states");

        int populationRecords = 0;
        foreach (ushort pointer in populationPointers)
        {
            RoomEnemyPopulationDefinition compiled = RoomEnemyPopulationDefinitions.Get(pointer);
            int address = RoomEnemyRomLayout.PopulationBank | pointer;
            ReadOnlySpan<RoomEnemyPlacement> records = compiled.Placements.Span;
            for (int index = 0; index < records.Length; index++, address += 16)
            {
                ushort definitionWord = ReadVerificationWord(bus, address);
                AssertTrue(definitionWord != 0xffff,
                    $"population $A1:{pointer:X4} has no early terminator");
                // A header several actors share places the one its first parameter selects (#1989).
                EnemyDefinitionId nativeEnemy = EnemySpawnSettingsCodec.PlacedActor(definitionWord, ReadVerificationWord(bus, address + 12));
                // The native entry is compared as the typed spawn the codec decodes for the species: its position, its
                // engine properties, and the settings its species reads from the remaining words.
                RoomEnemyPlacement native = RoomEnemyPopulationRecordCodec.Decode(nativeEnemy,
                    ReadVerificationWord(bus, address + 2), ReadVerificationWord(bus, address + 4),
                    ReadVerificationWord(bus, address + 6), ReadVerificationWord(bus, address + 8),
                    ReadVerificationWord(bus, address + 10), ReadVerificationWord(bus, address + 12),
                    ReadVerificationWord(bus, address + 14));
                AssertEqual(native, records[index],
                    $"population $A1:{pointer:X4} ordered slot {index}");
            }
            AssertEqual(0xffff, ReadVerificationWord(bus, address),
                $"population $A1:{pointer:X4} length reaches its native terminator");
            // The room load reads the quota after the terminator only of a list that places enemies; an empty list has
            // none, and the byte after the first list's terminator is the next list's first (#2584).
            if (compiled is PlacedRoomEnemyPopulationDefinition placed)
            {
                AssertEqual(bus.ReadCartridgeByte(address + 2), placed.DeathQuota,
                    $"population $A1:{pointer:X4} death quota follows its terminator");
            }
            populationRecords += records.Length;
        }
        AssertEqual(RoomEnemyPopulationDefinitions.Definitions.Sum(list => list.Placements.Length), populationRecords,
            "every ordered retail enemy placement is compiled");

        int graphicsRecords = 0;
        foreach (ushort pointer in graphicsPointers)
        {
            RoomEnemyGraphicsSetDefinition compiled = RoomEnemyGraphicsSetDefinitions.Get(pointer);
            RoomEnemyGraphicsSetMembers members = RoomEnemyGraphicsSetLists.MembersOf(compiled);
            int address = RoomEnemyRomLayout.TilesetBank | pointer;
            ReadOnlySpan<RoomEnemyGraphicsSetMember> records = members.Records.Span;
            int index = 0;
            for (ushort definitionWord = ReadVerificationWord(bus, address); definitionWord != 0xffff;
                 address += 4, definitionWord = ReadVerificationWord(bus, address))
            {
                // The Reflec's artwork, which two sets load and no picture shows, is no record of the import's (#1686).
                if (definitionWord == ReflecHeader)
                    continue;
                AssertTrue(index < records.Length, $"graphics set $B4:{pointer:X4} compiles its native record {index}");
                ushort placementWord = ReadVerificationWord(bus, address + 2);
                // The member's palette row is the import's knowledge, which colours its sheets (#1512), so the record the
                // import holds names the row the native record's placement word names.
                int nativeRow = RoomEnemyGraphicsSetMember.NativePaletteRow(placementWord);
                RoomEnemyGraphicsSetMember native = EnemyDefinitionIdsConstants2.TryFromHeaderPointer(definitionWord, out EnemyDefinitionId enemy)
                    ? new RoomEnemyGraphicsSetMember(enemy, nativeRow)
                    : new RoomEnemyGraphicsSetMember(SuperMetroid.AssetExtraction.CartridgeTables.EnemyHeaderWords.UnplacedArtworkHeaders
                        .Decode(definitionWord, "header of unplaced enemy artwork"), nativeRow);
                AssertEqual(native, records[index],
                    $"graphics set $B4:{pointer:X4} ordered member {index} and its palette row");
                // So is the fixed block its tiles stage at, which places them among the sprite characters (#1513).
                AssertEqual(RoomEnemyGraphicsSetMember.NativeStagingBlock(placementWord), native.FixedBlock ?? 0,
                    $"graphics set $B4:{pointer:X4} member {index}'s staging block in the import's catalog");
                index++;
            }
            AssertEqual(records.Length, index,
                $"graphics set $B4:{pointer:X4} length reaches its native terminator");
            graphicsRecords += records.Length;
        }
        AssertEqual(RoomEnemyGraphicsSetLists.RecordCount, graphicsRecords,
            "every ordered retail enemy graphics member is compiled");
        AssertThrows<ArgumentOutOfRangeException>(
            () => RoomEnemyPopulationDefinitions.Get(0),
            "unknown population pointer fails loudly");
        AssertThrows<ArgumentOutOfRangeException>(
            () => RoomEnemyGraphicsSetDefinitions.Get(0),
            "unknown graphics-set pointer fails loudly");

        Console.WriteLine(
            $"Enemy room lists: {populationPointers.Length} populations/{populationRecords} " +
            $"placements and {graphicsPointers.Length} graphics sets/{graphicsRecords} " +
            "members match the independent import oracle, including order and quotas.");
    }

    /// <summary>Runs the complete enemy room-list comparison against the retail ROM.</summary>
    [VerificationCommand("--enemy-room-lists")]
    internal static int EnemyRoomListsCommand()
    {
        Suite(nameof(VerifyCompiledEnemyRoomLists), () => VerifyCompiledEnemyRoomLists());
        return 0;
    }
}

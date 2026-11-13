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
        AssertEqual(RoomEnemyPopulationDefinitions.ListCount, populationPointers.Length,
            "all retail population identities are compiled");
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
            ReadOnlySpan<RoomEnemyPopulationRecord> records = compiled.Records.Span;
            for (int index = 0; index < records.Length; index++, address += 16)
            {
                ushort definitionWord = ReadVerificationWord(bus, address);
                AssertTrue(definitionWord != 0xffff,
                    $"population $A1:{pointer:X4} has no early terminator");
                EnemyDefinitionId nativeEnemy = EnemyDefinitionIds.FromHeaderPointer(definitionWord);
                // The native entry is compared as the typed spawn the codec decodes for the species: its position, its
                // engine properties, and the settings its species reads from the remaining words.
                RoomEnemyPopulationRecord native = RoomEnemyPopulationRecordCodec.Decode(nativeEnemy,
                    ReadVerificationWord(bus, address + 2), ReadVerificationWord(bus, address + 4),
                    ReadVerificationWord(bus, address + 6), ReadVerificationWord(bus, address + 8),
                    ReadVerificationWord(bus, address + 10), ReadVerificationWord(bus, address + 12),
                    ReadVerificationWord(bus, address + 14));
                AssertEqual(native, records[index],
                    $"population $A1:{pointer:X4} ordered slot {index}");
            }
            AssertEqual(0xffff, ReadVerificationWord(bus, address),
                $"population $A1:{pointer:X4} length reaches its native terminator");
            AssertEqual(bus.ReadCartridgeByte(address + 2), compiled.DeathQuota,
                $"population $A1:{pointer:X4} death quota follows its terminator");
            populationRecords += records.Length;
        }
        AssertEqual(RoomEnemyPopulationDefinitions.RecordCount, populationRecords,
            "every ordered retail enemy placement is compiled");

        int graphicsRecords = 0;
        foreach (ushort pointer in graphicsPointers)
        {
            RoomEnemyGraphicsSetDefinition compiled = RoomEnemyGraphicsSetDefinitions.Get(pointer);
            RoomEnemyGraphicsSetMembers members = RoomEnemyGraphicsSetLists.MembersOf(compiled);
            var importRows = new GraphicsSetRows(members, EnemyGraphicsSetPaletteRowDefinitions.Rows);
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
                RoomEnemyGraphicsSetMember native = EnemyDefinitionIds.TryFromHeaderPointer(definitionWord, out EnemyDefinitionId enemy)
                    ? new RoomEnemyGraphicsSetMember(enemy)
                    : new RoomEnemyGraphicsSetMember(SuperMetroid.AssetExtraction.CartridgeTables.EnemyHeaderWords.UnplacedArtworkHeaders
                        .Decode(definitionWord, "header of unplaced enemy artwork"));
                AssertEqual(native, records[index],
                    $"graphics set $B4:{pointer:X4} ordered member {index}");
                // The member's palette row is the import's knowledge, which colours its sheets (#1512).
                AssertEqual(RoomEnemyGraphicsSetMember.NativePaletteRow(placementWord), importRows.RecordRow(index),
                    $"graphics set $B4:{pointer:X4} member {index}'s palette row in the import's catalog");
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

using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyCompiledEnemyRoomLists()
    {
        ISnesAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        ushort[] populationPointers = RoomStateDefinitions.All
            .Select(state => state.EnemyPopulationPointer).Distinct().Order().ToArray();
        ushort[] graphicsPointers = RoomStateDefinitions.All
            .Select(state => state.EnemyTilesetPointer).Distinct().Order().ToArray();
        AssertEqual(RoomEnemyPopulationDefinitions.ListCount, populationPointers.Length,
            "all retail population identities are compiled");
        AssertEqual(RoomEnemyGraphicsSetDefinitions.ListCount, graphicsPointers.Length,
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
                RoomEnemyPopulationRecord native = new(
                    ReadVerificationWord(bus, address),
                    ReadVerificationWord(bus, address + 2),
                    ReadVerificationWord(bus, address + 4),
                    ReadVerificationWord(bus, address + 6),
                    ReadVerificationWord(bus, address + 8),
                    ReadVerificationWord(bus, address + 10),
                    ReadVerificationWord(bus, address + 12),
                    ReadVerificationWord(bus, address + 14));
                AssertEqual(native, records[index],
                    $"population $A1:{pointer:X4} ordered slot {index}");
                AssertTrue(native.DefinitionPointer != 0xffff,
                    $"population $A1:{pointer:X4} has no early terminator");
            }
            AssertEqual(0xffff, ReadVerificationWord(bus, address),
                $"population $A1:{pointer:X4} length reaches its native terminator");
            AssertEqual(bus.ReadByte(address + 2), compiled.DeathQuota,
                $"population $A1:{pointer:X4} death quota follows its terminator");
            populationRecords += records.Length;
        }
        AssertEqual(RoomEnemyPopulationDefinitions.RecordCount, populationRecords,
            "every ordered retail enemy placement is compiled");

        int graphicsRecords = 0;
        foreach (ushort pointer in graphicsPointers)
        {
            RoomEnemyGraphicsSetDefinition compiled = RoomEnemyGraphicsSetDefinitions.Get(pointer);
            int address = RoomEnemyRomLayout.TilesetBank | pointer;
            ReadOnlySpan<RoomEnemyGraphicsSetHeader> records = compiled.Records.Span;
            for (int index = 0; index < records.Length; index++, address += 4)
            {
                RoomEnemyGraphicsSetHeader native = new(
                    ReadVerificationWord(bus, address),
                    ReadVerificationWord(bus, address + 2));
                AssertEqual(native, records[index],
                    $"graphics set $B4:{pointer:X4} ordered member {index}");
                AssertTrue(native.DefinitionPointer != 0xffff,
                    $"graphics set $B4:{pointer:X4} has no early terminator");
            }
            AssertEqual(0xffff, ReadVerificationWord(bus, address),
                $"graphics set $B4:{pointer:X4} length reaches its native terminator");
            graphicsRecords += records.Length;
        }
        AssertEqual(RoomEnemyGraphicsSetDefinitions.RecordCount, graphicsRecords,
            "every ordered retail enemy graphics member is compiled");
        AssertThrows<ArgumentOutOfRangeException>(
            () => RoomEnemyPopulationDefinitions.Get(0),
            "unknown population pointer fails loudly");
        AssertThrows<ArgumentOutOfRangeException>(
            () => RoomEnemyGraphicsSetDefinitions.Get(0),
            "unknown graphics-set pointer fails loudly");

        var guardedRuntime = new SuperMetroidRuntime(new EnemyRoomListReadGuard(bus));
        guardedRuntime.InitializeStartingCeresRoom();
        AssertTrue(guardedRuntime.Enemies.IsLoaded,
            "production Ceres entry loads ordered enemies without room-list ROM reads");
        Console.WriteLine(
            $"Enemy room lists: {populationPointers.Length} populations/{populationRecords} " +
            $"placements and {graphicsPointers.Length} graphics sets/{graphicsRecords} " +
            "members match ROM; guarded production entry passes.");
    }

    private sealed class EnemyRoomListReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        private static readonly HashSet<int> SourceBytes = BuildSourceBytes();

        public byte ReadByte(int address)
        {
            if (SourceBytes.Contains(address))
                throw new InvalidOperationException(
                    $"Production room entry read enemy room-list source ${address:X6}.");
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);

        private static HashSet<int> BuildSourceBytes()
        {
            var bytes = new HashSet<int>();
            foreach (ushort pointer in RoomEnemyPopulationDefinitions.Pointers)
            {
                RoomEnemyPopulationDefinition list = RoomEnemyPopulationDefinitions.Get(pointer);
                int address = RoomEnemyRomLayout.PopulationBank | pointer;
                foreach (int item in Enumerable.Range(address, list.Records.Length * 16 + 3))
                    bytes.Add(item);
            }
            foreach (ushort pointer in RoomEnemyGraphicsSetDefinitions.Pointers)
            {
                RoomEnemyGraphicsSetDefinition list = RoomEnemyGraphicsSetDefinitions.Get(pointer);
                int address = RoomEnemyRomLayout.TilesetBank | pointer;
                foreach (int item in Enumerable.Range(address, list.Records.Length * 4 + 2))
                    bytes.Add(item);
            }
            return bytes;
        }
    }
}

using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyCompiledEnemyDefinitions()
    {
        ISnesAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var referencedPointers = new HashSet<ushort>();
        foreach (CartridgeRoomState state in RoomStateDefinitions.All)
        {
            int populationAddress = RoomEnemyRomLayout.PopulationBank |
                state.EnemyPopulationPointer;
            for (int slot = 0; slot < RoomEnemySystem.MaximumEnemyCount; slot++)
            {
                ushort pointer = ReadVerificationWord(bus, populationAddress);
                if (pointer == 0xffff) break;
                referencedPointers.Add(pointer);
                populationAddress += 16;
            }

            int graphicsAddress = RoomEnemyRomLayout.TilesetBank |
                state.EnemyTilesetPointer;
            for (int slot = 0; slot < 4; slot++)
            {
                ushort pointer = ReadVerificationWord(bus, graphicsAddress);
                if (pointer == 0xffff) break;
                referencedPointers.Add(pointer);
                graphicsAddress += 4;
            }
        }

        AssertEqual(RoomEnemyDefinitionCatalog.Count, referencedPointers.Count,
            "compiled enemy-header catalog covers every retail room population and graphics set");
        AssertTrue(referencedPointers.Order().SequenceEqual(RoomEnemyDefinitionCatalog.Pointers),
            "compiled enemy-header identities exactly match the independent room-state ROM walk");
        foreach (ushort pointer in referencedPointers)
        {
            AssertEqual(ReadNativeEnemyDefinition(bus, pointer),
                RoomEnemyDefinitionCatalog.Get(pointer),
                $"compiled enemy header $A0:{pointer:X4} retains all 33 native fields");
        }
        AssertThrows<ArgumentOutOfRangeException>(
            () => RoomEnemyDefinitionCatalog.Get(0),
            "compiled enemy headers reject an unrecognized pointer");

        // This is the actual room-entry path, not merely a catalog lookup. The guard
        // rejects every byte of every retail header while leaving immutable artwork
        // and all mutable SNES memory available for this intermediate migration.
        var guardedRuntime = new SuperMetroidRuntime(new EnemyHeaderReadGuard(bus));
        guardedRuntime.InitializeStartingCeresRoom();
        AssertTrue(guardedRuntime.Enemies.IsLoaded,
            "Ceres production room entry loads enemies without reading native headers");

        Console.WriteLine(
            $"Enemy definitions: {referencedPointers.Count} retail headers match all fields; " +
            "production room entry rejects bank-$A0 header reads.");
    }

    private static RoomEnemyDefinition ReadNativeEnemyDefinition(
        ISnesAddressSpace bus, ushort pointer)
    {
        int address = RoomEnemyRomLayout.DefinitionBank | pointer;
        ushort Word(int offset) => ReadVerificationWord(bus, address + offset);
        byte Byte(int offset) => bus.ReadByte(address + offset);
        return new RoomEnemyDefinition(
            Word(0), Word(2), Word(4), Word(6), Word(8), Word(10),
            Byte(12), Byte(13), Word(14), Word(16), Word(18), Word(20),
            Word(22), Word(24), Word(26), Word(28), Word(30), Word(32),
            Word(34), Word(36), Word(38), Word(40), Word(42), Word(44),
            Word(46), Word(48), Word(50), Word(52),
            Word(54) | Byte(56) << 16, Byte(57), Word(58), Word(60), Word(62));
    }

    private sealed class EnemyHeaderReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        private static readonly HashSet<int> HeaderBytes =
            RoomEnemyDefinitionCatalog.Pointers
                .SelectMany(pointer => Enumerable.Range(
                    RoomEnemyRomLayout.DefinitionBank | pointer, 64))
                .ToHashSet();

        public byte ReadByte(int address)
        {
            if (HeaderBytes.Contains(address))
                throw new InvalidOperationException(
                    $"Production room entry read enemy header ${address:X6}.");
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}

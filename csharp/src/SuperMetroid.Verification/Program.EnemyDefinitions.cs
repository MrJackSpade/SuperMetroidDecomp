using System.Reflection;
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
        ushort[] auxiliaryPointers =
            RoomEnemyAuxiliaryDefinitionCatalog.Pointers.Order().ToArray();
        AssertTrue(auxiliaryPointers.SequenceEqual(new ushort[]
            {
                EnemyLifecycleDefinitions.RespawnPlaceholder,
                MotherBrainBabyMetroidDefinitions.EnemyDefinition,
                EnemyDefinitionPointers.MotherBrainFallingTube,
                TorizoChozoOrbInstructionProgramDefinitions.BombOrbEnemyHeader,
                TorizoChozoOrbInstructionProgramDefinitions.GoldenOrbEnemyHeader,
            }),
            "all runtime-created enemy headers outside retail room lists are catalogued");
        AssertTrue(auxiliaryPointers.All(pointer => !referencedPointers.Contains(pointer)),
            "auxiliary headers do not duplicate a room-selected definition");
        foreach (ushort pointer in auxiliaryPointers)
        {
            AssertTrue(RoomEnemyAuxiliaryDefinitionCatalog.TryGet(pointer,
                    out RoomEnemyDefinition compiled),
                $"auxiliary enemy header $A0:{pointer:X4} resolves");
            AssertEqual(ReadNativeEnemyDefinition(bus, pointer), compiled,
                $"auxiliary enemy header $A0:{pointer:X4} retains all 33 native fields");
        }
        var namePointers = referencedPointers
            .Select(pointer => RoomEnemyDefinitionCatalog.Get(pointer).NamePointer)
            .Where(pointer => pointer != 0)
            .ToHashSet();
        AssertTrue(namePointers.SetEquals(RoomEnemySpawnNameDefinitions.Pointers),
            "compiled enemy spawn-name identities exactly match all retail headers");
        foreach (ushort pointer in namePointers)
        {
            int address = RoomEnemyRomLayout.TilesetBank | pointer;
            var native = new RoomEnemySpawnNameWords(
                ReadVerificationWord(bus, address),
                ReadVerificationWord(bus, address + 2),
                ReadVerificationWord(bus, address + 4),
                ReadVerificationWord(bus, address + 6),
                ReadVerificationWord(bus, address + 8),
                ReadVerificationWord(bus, address + 12));
            AssertEqual(native, RoomEnemySpawnNameDefinitions.Get(pointer),
                $"compiled enemy name $B4:{pointer:X4} retains exactly the six copied words");
        }
        AssertThrows<ArgumentOutOfRangeException>(
            () => RoomEnemyDefinitionCatalog.Get(0),
            "compiled enemy headers reject an unrecognized pointer");
        AssertTrue(!RoomEnemyAuxiliaryDefinitionCatalog.TryGet(0, out _),
            "auxiliary enemy headers reject an unrecognized pointer");
        AssertThrows<InvalidDataException>(
            () => RoomEnemySpawnNameDefinitions.Get(0),
            "compiled enemy names reject an unrecognized pointer");

        // This is the actual room-entry path, not merely a catalog lookup. The guard
        // rejects every byte of every retail header while leaving immutable artwork
        // and all mutable SNES memory available for this intermediate migration.
        var guardedRuntime = new SuperMetroidRuntime(new EnemyHeaderReadGuard(bus));
        guardedRuntime.InitializeStartingCeresRoom();
        AssertTrue(guardedRuntime.Enemies.IsLoaded,
            "Ceres production room entry loads enemies without reading native headers");

        VerifyMotherBrainFallingTubePopulationDefinitions(bus);

        Console.WriteLine(
            $"Enemy definitions: {referencedPointers.Count} retail + " +
            $"{auxiliaryPointers.Length} auxiliary headers and " +
            $"{namePointers.Count} spawn-name records match all retained fields; " +
            "production room entry rejects native header and name reads.");
    }

    private static void VerifyMotherBrainFallingTubePopulationDefinitions(
        ISnesAddressSpace rom)
    {
        ushort[] pointers = MotherBrainFallingTubePopulationDefinitions.Pointers.ToArray();
        AssertEqual(5, pointers.Length, "all five Mother Brain falling-tube placements are compiled");
        var enemies = new RoomEnemySystem();
        FieldInfo busField = typeof(RoomEnemySystem).GetField("_bus",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        MethodInfo spawn = typeof(RoomEnemySystem).GetMethod("SpawnMotherBrainFallingTube",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        busField.SetValue(enemies, new EnemyHeaderReadGuard(rom));

        for (int index = 0; index < pointers.Length; index++)
        {
            ushort pointer = pointers[index];
            int address = 0xa90000 | pointer;
            ushort[] words = Enumerable.Range(0, 8)
                .Select(wordIndex => ReadVerificationWord(rom, address + wordIndex * 2))
                .ToArray();
            var native = new RoomEnemyPopulationRecord(
                words[0], words[1], words[2], words[3],
                words[4], words[5], words[6], words[7]);
            AssertEqual(native, MotherBrainFallingTubePopulationDefinitions.Get(pointer),
                $"Mother Brain falling-tube record $A9:{pointer:X4} matches all eight native words");

            spawn.Invoke(enemies, [pointer]);
            RoomEnemySlot tube = enemies.Slots[index];
            AssertEqual(native, tube.Spawn.Population,
                $"production tube spawn {index} retains its authored placement and parameters");
            AssertEqual(native.XPosition, tube.XPosition,
                $"production tube spawn {index} uses its compiled X coordinate");
            AssertEqual(native.YPosition, tube.YPosition,
                $"production tube spawn {index} uses its compiled Y coordinate");
        }
        AssertEqual(5, enemies.EnemyCount,
            "all five compiled falling-tube placements allocated physical enemy slots");
        AssertThrows<ArgumentOutOfRangeException>(
            () => MotherBrainFallingTubePopulationDefinitions.Get(0),
            "an unknown falling-tube placement fails explicitly");

        // A constructed bus must continue to supply its own placement rather than
        // silently borrowing the retail coordinates from the compiled catalog.
        var fixture = new TestAddressSpace();
        byte[] header = Enumerable.Range(0, 64)
            .Select(offset => rom.ReadByte(0xa00000 |
                (EnemyDefinitionPointers.MotherBrainFallingTube + offset)))
            .ToArray();
        fixture.WriteBytes(0xa00000 | EnemyDefinitionPointers.MotherBrainFallingTube, header);
        int fixtureAddress = 0xa90000 | MotherBrainFallingTubePopulationDefinitions.BottomLeft;
        byte[] authoredRecord = Enumerable.Range(0, 16)
            .Select(offset => rom.ReadByte(fixtureAddress + offset))
            .ToArray();
        authoredRecord[2] = 0x34;
        authoredRecord[3] = 0x12;
        fixture.WriteBytes(fixtureAddress, authoredRecord);
        var fixtureEnemies = new RoomEnemySystem();
        busField.SetValue(fixtureEnemies, fixture);
        spawn.Invoke(fixtureEnemies, [MotherBrainFallingTubePopulationDefinitions.BottomLeft]);
        AssertEqual(0x1234, fixtureEnemies.Slots[0].XPosition,
            "constructed room fixture retains its authored falling-tube X position");

        Console.WriteLine(
            "Mother Brain falling tubes: five complete ROM records, five guarded production " +
            "spawns and one independent constructed placement pass.");
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
        private static readonly HashSet<int> DefinitionBytes =
            RoomEnemyDefinitionCatalog.Pointers
                .Concat(RoomEnemyAuxiliaryDefinitionCatalog.Pointers)
                .SelectMany(pointer => Enumerable.Range(
                    RoomEnemyRomLayout.DefinitionBank | pointer, 64))
                .Concat(RoomEnemySpawnNameDefinitions.Pointers
                    .SelectMany(pointer => Enumerable.Range(
                        RoomEnemyRomLayout.TilesetBank | pointer, 14)))
                .Concat(MotherBrainFallingTubePopulationDefinitions.Pointers.ToArray()
                    .SelectMany(pointer => Enumerable.Range(0xa90000 | pointer, 16)))
                .ToHashSet();

        public byte ReadByte(int address)
        {
            if (DefinitionBytes.Contains(address))
                throw new InvalidOperationException(
                    $"Production room entry read enemy header/name definition ${address:X6}.");
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}

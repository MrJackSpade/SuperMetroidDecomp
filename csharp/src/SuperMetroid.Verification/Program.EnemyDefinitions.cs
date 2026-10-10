using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyCompiledEnemyDefinitions()
    {
        ISnesAddressSpace bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var referencedPointers = new HashSet<EnemyDefinitionId>();
        foreach (CartridgeRoomState state in RoomStateDefinitions.All)
        {
            int populationAddress = RoomEnemyRomLayout.PopulationBank |
                state.EnemyPopulationPointer;
            for (int slot = 0; slot < RoomEnemySystem.MaximumEnemyCount; slot++)
            {
                ushort pointerWord = ReadVerificationWord(bus, populationAddress);
                if (pointerWord == 0xffff) break;
                EnemyDefinitionId pointer = EnemyDefinitionIds.FromHeaderPointer(pointerWord);
                referencedPointers.Add(pointer);
                populationAddress += 16;
            }

            int graphicsAddress = RoomEnemyRomLayout.TilesetBank |
                state.EnemyTilesetPointer;
            for (int slot = 0; slot < 4; slot++)
            {
                ushort pointerWord = ReadVerificationWord(bus, graphicsAddress);
                if (pointerWord == 0xffff) break;
                EnemyDefinitionId pointer = EnemyDefinitionIds.FromHeaderPointer(pointerWord);
                referencedPointers.Add(pointer);
                graphicsAddress += 4;
            }
        }

        AssertEqual(RoomEnemyDefinitionCatalog.Count, referencedPointers.Count,
            "compiled enemy-header catalog covers every retail room population and graphics set");
        AssertTrue(referencedPointers.Order().SequenceEqual(RoomEnemyDefinitionCatalog.Pointers),
            "compiled enemy-header identities exactly match the independent room-state ROM walk");
        foreach (EnemyDefinitionId pointer in referencedPointers)
        {
            AssertEqual(ReadNativeEnemyDefinition(bus, pointer),
                RoomEnemyDefinitionCatalog.Get(pointer),
                $"compiled enemy header $A0:{(int)pointer:X4} retains all 33 native fields");
        }
        EnemyDefinitionId[] auxiliaryPointers =
            RoomEnemyAuxiliaryDefinitionCatalog.Pointers.Order().ToArray();
        AssertTrue(auxiliaryPointers.SequenceEqual(new EnemyDefinitionId[]
            {
                EnemyDefinitionId.Respawn,
                EnemyDefinitionId.SporeSpawnStalk,
                EnemyDefinitionId.BabyMetroidCutscene,
                EnemyDefinitionId.MotherBrainTubes,
                EnemyDefinitionId.BombTorizoOrb,
                EnemyDefinitionId.GoldenTorizoOrb,
            }),
            "all runtime-created enemy headers outside retail room lists are catalogued");
        AssertTrue(auxiliaryPointers.All(pointer => !referencedPointers.Contains(pointer)),
            "auxiliary headers do not duplicate a room-selected definition");
        foreach (EnemyDefinitionId pointer in auxiliaryPointers)
        {
            AssertTrue(RoomEnemyAuxiliaryDefinitionCatalog.TryGet(pointer,
                    out RoomEnemyDefinition compiled),
                $"auxiliary enemy header $A0:{(int)pointer:X4} resolves");
            AssertEqual(ReadNativeEnemyDefinition(bus, pointer), compiled,
                $"auxiliary enemy header $A0:{(int)pointer:X4} retains all 33 native fields");
        }
        AssertThrows<ArgumentOutOfRangeException>(
            () => RoomEnemyDefinitionCatalog.Get(0),
            "compiled enemy headers reject an unrecognized pointer");
        AssertTrue(!RoomEnemyAuxiliaryDefinitionCatalog.TryGet(0, out _),
            "auxiliary enemy headers reject an unrecognized pointer");

        Suite(nameof(VerifyMotherBrainFallingTubePopulationDefinitions), () => VerifyMotherBrainFallingTubePopulationDefinitions(bus));

        Console.WriteLine(
            $"Enemy definitions: {referencedPointers.Count} retail + " +
            $"{auxiliaryPointers.Length} auxiliary headers match all retained fields; " +
            "cartridge parsing is isolated in the importer.");
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
        busField.SetValue(enemies, SuperMetroidAddressSpace.CreateWithoutCartridge());

        for (int index = 0; index < pointers.Length; index++)
        {
            ushort pointer = pointers[index];
            int address = 0xa90000 | pointer;
            ushort[] words = Enumerable.Range(0, 8)
                .Select(wordIndex => ReadVerificationWord(rom, address + wordIndex * 2))
                .ToArray();
            var native = new RoomEnemyPopulationRecord(
                EnemyDefinitionIds.FromHeaderPointer(words[0]), words[1], words[2], words[3],
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
                ((ushort)EnemyDefinitionId.MotherBrainTubes + offset)))
            .ToArray();
        fixture.WriteBytes(0xa00000 | (ushort)EnemyDefinitionId.MotherBrainTubes, header);
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
            "Mother Brain falling tubes: five complete ROM records, five RAM-only production " +
            "spawns and one independent constructed placement pass.");
    }

    private static RoomEnemyDefinition ReadNativeEnemyDefinition(
        ISnesAddressSpace bus, EnemyDefinitionId pointer)
    {
        int address = RoomEnemyRomLayout.DefinitionBank | (ushort)pointer;
        ushort Word(int offset) => ReadVerificationWord(bus, address + offset);
        byte Byte(int offset) => bus.ReadByte(address + offset);
        return new RoomEnemyDefinition(
            Word(0), Word(2), Word(4), Word(6), Word(8), Word(10),
            Byte(12), Byte(13), Word(14), Word(16), Word(18), Word(20),
            Word(22), Word(24), ClosedNativeWords.Decode<GrappleAiRoutine>(Word(26), "grapple AI routine"), Word(28), Word(30), Word(32),
            Word(34), Word(36), Word(38), Word(40), Word(42), Word(44),
            Word(46), Word(48), Word(50), Word(52),
            Word(54) | Byte(56) << 16, Byte(57), Word(58), Word(60), Word(62));
    }

}

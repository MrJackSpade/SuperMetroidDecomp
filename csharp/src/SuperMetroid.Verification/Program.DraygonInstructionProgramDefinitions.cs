using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Proves that Draygon's four physical enemy records share one complete compiled
    /// mechanics owner and retain the cartridge's atomic four-list installation command.
    /// </summary>
    private static void VerifyDraygonInstructionProgramDefinitions()
    {
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        const BindingFlags staticFlags = BindingFlags.Static | BindingFlags.NonPublic;
        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));

        for (int index = 0;
             index < DraygonInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            DraygonInstructionMechanicsWord definition =
                DraygonInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                ReadDraygonWord(rom, 0xa50000 | definition.Address),
                definition.Value,
                $"Draygon mechanics word $A5:{definition.Address:X4}");
        }

        ushort[] definitions =
        [
            ReadDraygonDefinition("DraygonBodyDefinition", staticFlags),
            ReadDraygonDefinition("DraygonEyeDefinition", staticFlags),
            ReadDraygonDefinition("DraygonTailDefinition", staticFlags),
            ReadDraygonDefinition("DraygonArmsDefinition", staticFlags),
        ];
        var enemies = new RoomEnemySystem();
        var guard = new DraygonInstructionReadGuard();
        typeof(RoomEnemySystem).GetField("_bus", instanceFlags)!.SetValue(enemies, guard);
        MethodInfo readMechanics = typeof(RoomEnemySystem).GetMethod(
            "ReadEnemyInstructionMechanicsWord", instanceFlags)!;
        foreach (ushort enemyDefinition in definitions)
        {
            var slot = new RoomEnemySlot(0)
            {
                EnemyDefinitionPointer = enemyDefinition,
                Definition = default(RoomEnemyDefinition) with { Bank = 0xa5 },
            };
            for (int index = 0;
                 index < DraygonInstructionProgramDefinitions.MechanicsWordCount;
                 index++)
            {
                DraygonInstructionMechanicsWord word =
                    DraygonInstructionProgramDefinitions.MechanicsWord(index);
                AssertEqual(
                    word.Value,
                    (ushort)readMechanics.Invoke(enemies, [slot, word.Address])!,
                    $"Draygon definition ${enemyDefinition:X4} owns $A5:{word.Address:X4}");
            }
        }
        AssertEqual(0, guard.ReadAttempts,
            "all four Draygon definitions avoid cartridge mechanics reads");

        RoomEnemySlot body = enemies.Slots[0];
        RoomEnemySlot eye = enemies.Slots[1];
        RoomEnemySlot tail = enemies.Slots[2];
        RoomEnemySlot arms = enemies.Slots[3];
        body.EnemyDefinitionPointer = definitions[0];
        eye.EnemyDefinitionPointer = definitions[1];
        tail.EnemyDefinitionPointer = definitions[2];
        arms.EnemyDefinitionPointer = definitions[3];
        var state = new DraygonEnemyState(body)
        {
            Eye = eye,
            Tail = tail,
            Arms = arms,
            RoomLoadingIrqCommand = 0,
        };
        typeof(RoomEnemySystem).GetField("_draygon", instanceFlags)!.SetValue(enemies, state);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "TryProcessDraygonInstruction", instanceFlags)!;

        object?[] resetArguments =
        [
            body,
            null,
            DraygonCodePointers.Instruction_Draygon_SetInstList_Body_Eye_Tail_Arms,
            DraygonInstructionProgramDefinitions.BodyFacingLeftReset,
        ];
        AssertTrue((bool)process.Invoke(enemies, resetArguments)!,
            "Draygon four-list reset command is translated");
        AssertEqual(DraygonInstructionProgramDefinitions.BodyFacingLeftIdle,
            body.CurrentInstruction, "Draygon reset installs body list");
        AssertEqual(DraygonInstructionProgramDefinitions.EyeFacingLeftIdle,
            eye.CurrentInstruction, "Draygon reset installs eye list");
        AssertEqual(0x99c6, tail.CurrentInstruction,
            "Draygon reset installs internal tail idle list");
        AssertEqual(DraygonInstructionProgramDefinitions.ArmsFacingLeftIdle,
            arms.CurrentInstruction, "Draygon reset installs arms list");
        AssertEqual(0x97c5, (ushort)resetArguments[3]!,
            "Draygon reset consumes all four list operands atomically");

        object?[] duplicateIrqArguments =
        [
            body,
            null,
            DraygonCodePointers.Instruction_Draygon_RoomLoadingInterruptCmd_BeginHUDDrawDuplicate,
            (ushort)0x9c8a,
        ];
        AssertTrue((bool)process.Invoke(enemies, duplicateIrqArguments)!,
            "right-facing duplicate HUD IRQ command is translated");
        AssertEqual(0x000c, state.RoomLoadingIrqCommand,
            "duplicate HUD IRQ command publishes the native room-loading command");
        AssertEqual(0x9c8c, (ushort)duplicateIrqArguments[3]!,
            "duplicate HUD IRQ command advances one instruction word");

        AssertThrows<InvalidDataException>(
            () => DraygonInstructionProgramDefinitions.ReadMechanicsWord(
                DraygonInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Draygon extended-spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => DraygonInstructionProgramDefinitions.ReadMechanicsWord(0x8000),
            "foreign bank-A5 data is outside the compiled Draygon family");

        _ = ProbeDraygonInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeDraygonInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Draygon allocation probe consumes mechanics data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Draygon mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            $"Draygon instruction mechanics: " +
            $"{DraygonInstructionProgramDefinitions.MechanicsWordCount} compiled words, " +
            $"{DraygonInstructionProgramDefinitions.PresentationWordCount} live presentation " +
            "operands, four physical owners, atomic list handoff, and both HUD IRQ opcodes pass.");
    }

    private static ushort ReadDraygonDefinition(string name, BindingFlags flags) =>
        (ushort)typeof(RoomEnemySystem).GetField(name, flags)!.GetRawConstantValue()!;

    private static ushort ReadDraygonWord(SuperMetroidAddressSpace source, int address) =>
        unchecked((ushort)(source.ReadByte(address) | source.ReadByte(address + 1) << 8));

    private static int ProbeDraygonInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += DraygonInstructionProgramDefinitions.ReadMechanicsWord(
                DraygonInstructionProgramDefinitions.BodyFacingLeftIdle);
        }
        return checksum;
    }

    private sealed class DraygonInstructionReadGuard : ISnesAddressSpace
    {
        internal int ReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            ReadAttempts++;
            throw new InvalidOperationException(
                $"Draygon mechanics attempted cartridge read ${address:X6}.");
        }

        public void WriteByte(int address, byte value) =>
            throw new InvalidOperationException(
                $"Draygon mechanics attempted cartridge write ${address:X6}.");
    }
}

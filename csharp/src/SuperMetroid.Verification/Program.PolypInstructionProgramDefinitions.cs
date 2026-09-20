using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyPolypInstructionProgramDefinitions()
    {
        VerifyPolypInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyPolypInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < PolypInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            PolypInstructionMechanicsWord definition =
                PolypInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadPolypInstructionWord(rom, definition.Address),
                $"Polyp mechanics word $A2:{definition.Address:X4}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = new PolypInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        Type type = typeof(RoomEnemySystem);
        type.GetField("_bus", flags)!.SetValue(enemies, guard);
        type.GetField("_setRandomNumber", flags)!.SetValue(
            enemies,
            (Action<ushort>)(_ => { }));
        var initialize = type.GetMethod("InitializePolyp", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        MethodInfo process = type.GetMethod("ProcessInstructions", flags)!;

        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = RoomEnemySystem.PolypDefinition;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        initialize(slot);
        AssertEqual(PolypInstructionProgramDefinitions.Stationary,
            slot.CurrentInstruction,
            "real Polyp initializer installs compiled stationary program");

        object?[] processArguments =
            [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        slot.InstructionTimer = 1;
        process.Invoke(enemies, processArguments);
        slot.InstructionTimer = 1;
        process.Invoke(enemies, processArguments);
        AssertEqual(unchecked((ushort)(PolypInstructionProgramDefinitions.Stationary + 4)),
            slot.CurrentInstruction,
            "Polyp program reaches terminal sleep");
        AssertTrue(guard.SawPresentationWord,
            "Polyp spritemap operand remains a cartridge read");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids both compiled Polyp mechanics words");
        AssertThrows<InvalidDataException>(
            () => PolypInstructionProgramDefinitions.ReadMechanicsWord(
                PolypInstructionProgramDefinitions.PresentationWord),
            "Polyp spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => PolypInstructionProgramDefinitions.ReadMechanicsWord(0xb520),
            "adjacent cooldown table is rejected as instruction mechanics");

        _ = ProbePolypInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbePolypInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Polyp allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Polyp mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Polyp instruction mechanics: two compiled words, the real initializer, " +
            "terminal sleep, and one live spritemap read pass with mechanics bytes forbidden.");
    }

    private static int ProbePolypInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += PolypInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? PolypInstructionProgramDefinitions.Stationary
                    : (ushort)0xb51e);
        }
        return checksum;
    }

    private static ushort ReadPolypInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa20000 | address) |
            source.ReadByte(0xa20000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class PolypInstructionReadGuard(
        ISnesAddressSpace source) : ISnesAddressSpace
    {
        internal bool SawPresentationWord { get; private set; }
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (PolypInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Polyp mechanics byte ${address:X6}.");
            }

            int presentation = 0xa20000 | PolypInstructionProgramDefinitions.PresentationWord;
            if (address == presentation || address == presentation + 1)
                SawPresentationWord = true;
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}

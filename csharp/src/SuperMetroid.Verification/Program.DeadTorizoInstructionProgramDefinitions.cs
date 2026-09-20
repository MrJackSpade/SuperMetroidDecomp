using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyDeadTorizoInstructionProgramDefinitions()
    {
        VerifyDeadTorizoInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyDeadTorizoInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < DeadTorizoInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            DeadTorizoInstructionMechanicsWord definition =
                DeadTorizoInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadDeadTorizoInstructionWord(rom, definition.Address),
                $"Dead Torizo mechanics word $A9:{definition.Address:X4}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = new DeadTorizoInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        Type type = typeof(RoomEnemySystem);
        type.GetField("_bus", flags)!.SetValue(enemies, guard);
        var initialize = type.GetMethod("InitializeDeadTorizo", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        MethodInfo process = type.GetMethod("ProcessInstructions", flags)!;

        RoomEnemySlot corpse = enemies.Slots[0];
        corpse.EnemyDefinitionPointer = RoomEnemySystem.DeadTorizoDefinition;
        corpse.Definition = default(RoomEnemyDefinition) with { Bank = 0xa9 };
        initialize(corpse);
        AssertEqual(DeadTorizoInstructionProgramDefinitions.Stationary,
            corpse.CurrentInstruction,
            "real Dead Torizo initializer installs compiled stationary program");

        object?[] processArguments =
            [corpse, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        corpse.InstructionTimer = 1;
        process.Invoke(enemies, processArguments);
        corpse.InstructionTimer = 1;
        process.Invoke(enemies, processArguments);
        AssertEqual(DeadTorizoInstructionProgramDefinitions.SleepOpcode,
            corpse.CurrentInstruction,
            "Dead Torizo program reaches terminal sleep");
        AssertTrue(guard.SawPresentationWord,
            "Dead Torizo spritemap operand remains a cartridge read");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids both compiled Dead Torizo mechanics words");
        AssertThrows<InvalidDataException>(
            () => DeadTorizoInstructionProgramDefinitions.ReadMechanicsWord(
                DeadTorizoInstructionProgramDefinitions.PresentationWord),
            "Dead Torizo spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => DeadTorizoInstructionProgramDefinitions.ReadMechanicsWord(
                DeadTorizoInstructionProgramDefinitions.FirstAdjacentPresentationData),
            "adjacent Dead Torizo spritemap data is rejected as instruction mechanics");

        _ = ProbeDeadTorizoInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeDeadTorizoInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Dead Torizo allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Dead Torizo mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Dead Torizo instruction mechanics: two compiled words, the real initializer, " +
            "terminal sleep, and one live spritemap read pass with mechanics bytes forbidden.");
    }

    private static int ProbeDeadTorizoInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += DeadTorizoInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? DeadTorizoInstructionProgramDefinitions.Stationary
                    : DeadTorizoInstructionProgramDefinitions.SleepOpcode);
        }
        return checksum;
    }

    private static ushort ReadDeadTorizoInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa90000 | address) |
            source.ReadByte(0xa90000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class DeadTorizoInstructionReadGuard(
        ISnesAddressSpace source) : ISnesAddressSpace
    {
        internal bool SawPresentationWord { get; private set; }
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (DeadTorizoInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Dead Torizo mechanics byte ${address:X6}.");
            }

            int presentation =
                0xa90000 | DeadTorizoInstructionProgramDefinitions.PresentationWord;
            if (address == presentation || address == presentation + 1)
                SawPresentationWord = true;
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}

using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyKzanInstructionProgramDefinitions()
    {
        VerifyKzanInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyKzanInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < KzanInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            KzanInstructionMechanicsWord definition =
                KzanInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadKzanInstructionWord(rom, 0xa60000 | definition.Address),
                $"Kzan instruction mechanics word $A6:{definition.Address:X4}");
        }

        var guard = new KzanInstructionProgramReadGuard(rom);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        var initialize = typeof(RoomEnemySystem).GetMethod("InitializeKzanTop", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = RoomEnemySystem.KzanTopDefinition;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa6 };
        slot.YPosition = 0x0080;
        initialize(slot);
        AssertEqual(KzanInstructionProgramDefinitions.Idle, slot.CurrentInstruction,
            "Kzan initializer installs compiled program identity");

        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        object?[] arguments =
            [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        process.Invoke(enemies, arguments);
        process.Invoke(enemies, arguments);
        AssertEqual(unchecked((ushort)(KzanInstructionProgramDefinitions.Idle + 4)),
            slot.CurrentInstruction,
            "Kzan reaches terminal native sleep");
        AssertEqual(KzanInstructionProgramDefinitions.PresentationWord,
            guard.ObservedPresentationWord,
            "Kzan spritemap operand remains a cartridge read");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled Kzan mechanics bytes");

        AssertThrows<InvalidDataException>(
            () => KzanInstructionProgramDefinitions.ReadMechanicsWord(
                KzanInstructionProgramDefinitions.PresentationWord),
            "Kzan spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => KzanInstructionProgramDefinitions.ReadMechanicsWord(0x8b2f),
            "adjacent Kzan initializer code is rejected as mechanics");

        _ = ProbeKzanInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeKzanInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Kzan allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Kzan mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Kzan instruction mechanics: two compiled words, the complete production " +
            "program and one live spritemap read pass with mechanics bytes forbidden.");
    }

    private static int ProbeKzanInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += KzanInstructionProgramDefinitions.ReadMechanicsWord(
                KzanInstructionProgramDefinitions.Idle);
        }
        return checksum;
    }

    private static ushort ReadKzanInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class KzanInstructionProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal ushort? ObservedPresentationWord { get; private set; }
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (KzanInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Kzan mechanics byte ${address:X6}.");
            }

            int presentation = 0xa60000 | KzanInstructionProgramDefinitions.PresentationWord;
            if (address == presentation || address == presentation + 1)
            {
                ObservedPresentationWord =
                    KzanInstructionProgramDefinitions.PresentationWord;
            }

            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}

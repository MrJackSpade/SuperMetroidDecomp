using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyKzanInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyKzanInstructionProgramDefinitions), () => VerifyKzanInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    private static void VerifyKzanInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < KzanInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                KzanInstructionProgramDefinitionsTooling.MechanicsWord(index);
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
        slot.EnemyDefinitionPointer = EnemyDefinitionId.KzanTop;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa6 };
        slot.YPosition = 0x0080;
        initialize(slot);
        AssertEqual(KzanInstructionProgramDefinitions.Idle, slot.CurrentInstruction,
            "Kzan initializer installs compiled program identity");

        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        object?[] arguments =
            [slot, null, null, (ushort)0, (ushort)0, (ushort)0];
        process.Invoke(enemies, arguments);
        AssertEqual(ReadKzanInstructionWord(rom, 0xa60000 |
                KzanInstructionProgramDefinitionsTooling.PresentationWord), slot.SpritemapPointer,
            "Kzan selects its exact native frame without cartridge reads");
        process.Invoke(enemies, arguments);
        AssertEqual(unchecked((ushort)(KzanInstructionProgramDefinitions.Idle + 4)),
            slot.CurrentInstruction,
            "Kzan reaches terminal native sleep");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled Kzan mechanics bytes");

        AssertThrows<InvalidDataException>(
            () => KzanInstructionProgramDefinitions.ReadMechanicsWord(
                KzanInstructionProgramDefinitionsTooling.PresentationWord),
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
            "program and exact frame selection pass with instruction bytes forbidden.");
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
        ISnesAddressSpace, IImportCartridgeSource
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (KzanInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Kzan mechanics byte ${address:X6}.");
            }

            int presentation = 0xa60000 | KzanInstructionProgramDefinitionsTooling.PresentationWord;
            if (address == presentation || address == presentation + 1)
            {
                throw new InvalidOperationException("Production read the compiled Kzan visual operand.");
            }

            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}

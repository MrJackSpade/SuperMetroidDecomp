using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyElevatorInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyElevatorInstructionProgramDefinitions), () => VerifyElevatorInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    private static void VerifyElevatorInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyElevatorMechanicsMapping), () => VerifyElevatorMechanicsMapping(rom));
        Suite(nameof(VerifyElevatorPresentationMapping), () => VerifyElevatorPresentationMapping());
        Suite(nameof(VerifyElevatorVisualPointers), () => VerifyElevatorVisualPointers(rom));

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = new ElevatorInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        Type type = typeof(RoomEnemySystem);
        type.GetField("_bus", flags)!.SetValue(enemies, guard);
        MethodInfo initialize = type.GetMethod("InitializeElevator", flags)!;
        MethodInfo process = type.GetMethod("ProcessInstructions", flags)!;
        RoomEnemySlot elevator = enemies.Slots[0];
        elevator.EnemyDefinitionPointer = RoomEnemySystem.ElevatorDefinition;
        elevator.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
        initialize.Invoke(enemies, [elevator, null]);
        AssertEqual(ElevatorInstructionProgramDefinitions.Loop,
            elevator.CurrentInstruction,
            "real elevator initializer installs compiled animation loop");

        object?[] arguments =
            [elevator, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        for (int call = 0; call < 3; call++)
        {
            elevator.InstructionTimer = 1;
            process.Invoke(enemies, arguments);
        }
        AssertEqual((ushort)0x94da, elevator.CurrentInstruction,
            "elevator two-frame program loops to its second mechanics word");
        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "both elevator visual selectors are compiled, not reread from cartridge");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled elevator mechanics byte");
        AssertThrows<InvalidDataException>(
            () => ElevatorInstructionProgramDefinitions.ReadMechanicsWord(
                ElevatorInstructionProgramDefinitionsTooling.PresentationWordAddress(0)),
            "elevator spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => ElevatorInstructionProgramDefinitions.ReadMechanicsWord(
                ElevatorInstructionProgramDefinitions.FirstAdjacentMechanicsData),
            "adjacent elevator input table is rejected as instruction mechanics");

        _ = ProbeElevatorInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeElevatorInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "elevator allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed elevator mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Elevator instruction mechanics: four compiled words, the real initializer, " +
            "the complete two-frame loop, and two compiled visual selectors pass with " +
            "mechanics and selector reads forbidden.");
    }

    private static void VerifyElevatorMechanicsMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses = [0x94d6, 0x94da, 0x94de, 0x94e0];
        AssertEqual(addresses.Length, ElevatorInstructionProgramDefinitionsTooling.MechanicsWordCount, "elevator native mechanics count");
        var bytes = new HashSet<int>();
        for (int index = 0; index < addresses.Length; index++)
        {
            ushort native = ReadElevatorInstructionWord(rom, addresses[index]);
            var word = ElevatorInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(addresses[index], word.Address, "elevator native word position");
            AssertEqual(native, word.Value, "elevator enumerated native word");
            AssertEqual(native, ElevatorInstructionProgramDefinitions.ReadMechanicsWord(addresses[index]), "elevator direct native word");
            bytes.Add(addresses[index]); bytes.Add(addresses[index] + 1);
        }
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            AssertEqual(bytes.Contains(address), ElevatorInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0xa30000 | address), "elevator full byte ownership");
            AssertEqual(bytes.Contains(address), ElevatorInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0x1a30000 | address), "elevator bank mask aliases");
            AssertTrue(!ElevatorInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0xa20000 | address), "elevator other bank rejected");
        }
        var words = addresses.ToHashSet();
        for (int address = 0x94d4; address <= 0x94e4; address++)
            if (!words.Contains((ushort)address))
                AssertThrows<InvalidDataException>(() => ElevatorInstructionProgramDefinitions.ReadMechanicsWord((ushort)address), "elevator invalid mechanics word");
        foreach (int index in new[] { int.MinValue, -1, 4, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => ElevatorInstructionProgramDefinitionsTooling.MechanicsWord(index), "elevator mechanics bounds");
    }
    private static void VerifyElevatorPresentationMapping()
    {
        ushort[] expected = [0x94d8, 0x94dc];
        AssertEqual(expected.Length, ElevatorInstructionProgramDefinitionsTooling.PresentationWordCount, "elevator native visual count");
        for (int index = 0; index < expected.Length; index++)
            AssertEqual(expected[index], ElevatorInstructionProgramDefinitionsTooling.PresentationWordAddress(index), "elevator native visual position");
        for (int address = 0; address <= ushort.MaxValue; address++)
            AssertEqual(address is 0x94d8 or 0x94dc, ElevatorInstructionProgramDefinitions.IsPresentationWord((ushort)address), "elevator full visual membership");
        foreach (int index in new[] { int.MinValue, -1, 2, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => ElevatorInstructionProgramDefinitionsTooling.PresentationWordAddress(index), "elevator visual ordinal bounds");
    }
    private static void VerifyElevatorVisualPointers(SuperMetroidAddressSpace rom)
    {
        foreach (ushort operand in new ushort[] { 0x94d8, 0x94dc })
        {
            ushort native = ReadElevatorInstructionWord(rom, operand);
            AssertEqual(native, EnemySpritemapDefinitions.ElevatorFrameAt(operand), "elevator native visual pointer");
            AssertEqual((ushort)4, ReadElevatorInstructionWord(rom, native), "elevator native four-entry map");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0xa3, operand, out ushort shared), "elevator shared selector exists");
            AssertEqual(native, shared, "elevator shared selector value");
            AssertTrue(CompiledEnemyVisualSelectors.IsCalculatedSelector(0xa30000 | operand), "elevator excluded from literal regeneration");
        }
        for (int address = 0x94d4; address <= 0x94e4; address++)
            if (address is not (0x94d8 or 0x94dc))
            {
                AssertThrows<InvalidDataException>(() => EnemySpritemapDefinitions.ElevatorFrameAt((ushort)address), "elevator invalid visual operand");
                AssertTrue(!CompiledEnemyVisualSelectors.TryGet(0xa3, (ushort)address, out ushort missing), "elevator shared holes rejected");
                AssertEqual((ushort)0, missing, "elevator missing output cleared");
            }
    }
    private static int ProbeElevatorInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += ElevatorInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0 ? ElevatorInstructionProgramDefinitions.Loop : (ushort)0x94de);
        }
        return checksum;
    }

    private static ushort ReadElevatorInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa30000 | address) |
            source.ReadByte(0xa30000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class ElevatorInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (ElevatorInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled elevator mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa30000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < ElevatorInstructionProgramDefinitionsTooling.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        ElevatorInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        ObservedPresentationWords.Add(presentation);
                        break;
                    }
                }
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}

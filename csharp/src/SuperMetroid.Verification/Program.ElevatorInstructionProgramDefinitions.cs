using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;

internal static class ElevatorInstructionProgramDefinitionsVerification
{
    internal static void VerifyElevatorInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyElevatorInstructionProgramDefinitions), () => VerifyElevatorInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    internal static void VerifyElevatorInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyElevatorMechanicsMapping), () => VerifyElevatorMechanicsMapping(rom));
        Suite(nameof(VerifyElevatorPresentationMapping), () => VerifyElevatorPresentationMapping());
        Suite(nameof(VerifyElevatorVisualPointers), () => VerifyElevatorVisualPointers(rom));

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = NewElevatorInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        Type type = typeof(RoomEnemySystem);
        type.GetField("_bus", flags)!.SetValue(enemies, guard);
        MethodInfo initialize = type.GetMethod("InitializeElevator", flags)!;
        MethodInfo process = type.GetMethod("ProcessInstructions", flags)!;
        RoomEnemySlot elevator = enemies.Slots[0];
        elevator.EnemyDefinitionPointer = EnemyDefinitionId.Elevator;
        elevator.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
        initialize.Invoke(enemies, [elevator, null]);
        AssertEqual(ElevatorInstructionProgramDefinitions.Loop,
            elevator.CurrentInstruction,
            "real elevator initializer installs compiled animation loop");

        object?[] arguments =
            [elevator, null, null, Position16<RoomPixels>.Zero, (ushort)0];
        for (int call = 0; call < 3; call++)
        {
            elevator.InstructionTimer = 1;
            process.Invoke(enemies, arguments);
        }
        AssertEqual((ushort)0x94da, elevator.CurrentInstruction,
            "elevator two-frame program loops to its second mechanics word");
        AssertEqual(0, guard.ObservedWords.Count,
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
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa30000 | addresses[index]);
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
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa30000 | operand);
            AssertEqual(native, EnemySpritemapDefinitions.ElevatorFrameAt(operand), "elevator native visual pointer");
            AssertEqual((ushort)4, CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa30000 | native), "elevator native four-entry map");
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

    private static CartridgeReadGuard NewElevatorInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(ElevatorInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled elevator mechanics byte ${address:X6}.")
            .ObserveWords(0xa30000, ElevatorInstructionProgramDefinitionsTooling.PresentationWordCount,
                ElevatorInstructionProgramDefinitionsTooling.PresentationWordAddress);

    [VerificationCommand("--elevator-instruction-program-definitions")]
    internal static int ElevatorInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyElevatorInstructionProgramDefinitions), () => VerifyElevatorInstructionProgramDefinitions());
        return 0;
    }
}

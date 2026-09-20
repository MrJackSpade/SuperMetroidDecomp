using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    private static void VerifyTourianEntranceStatueInstructionProgramDefinitions()
    {
        VerifyTourianEntranceStatueInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyTourianEntranceStatueInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        AssertEqual(3,
            TourianEntranceStatueInstructionProgramDefinitions.MechanicsWordCount,
            "Tourian entrance-statue mechanics word count");
        for (int index = 0;
             index < TourianEntranceStatueInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            TourianEntranceStatueInstructionMechanicsWord definition =
                TourianEntranceStatueInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadTourianEntranceStatueInstructionWord(rom, definition.Address),
                $"Tourian entrance-statue mechanics word $AA:{definition.Address:X4}");
        }

        var guard = new TourianEntranceStatueInstructionReadGuard(rom);
        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod(
            "InitializeTourianEntranceStatue",
            flags)!;
        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        foreach (ushort parameter in new ushort[] { 0, 2, 4 })
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemies, new SnesCgram());
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.TourianEntranceStatueDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xaa };
            slot.Parameter1 = parameter;
            initialize.Invoke(enemies, [slot]);
            AssertEqual(
                TourianEntranceStatueInstructionProgramDefinitions.GetInitialInstruction(
                    parameter),
                slot.CurrentInstruction,
                $"Tourian entrance-statue initializer parameter {parameter}");

            process.Invoke(
                enemies,
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0]);
            AssertTrue(slot.Properties.HasAny(EnemyProperties.Deleted),
                $"Tourian entrance-statue parameter {parameter} executes native delete");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Tourian entrance-statue production execution avoids compiled mechanics bytes");

        AssertThrows<InvalidDataException>(
            () => TourianEntranceStatueInstructionProgramDefinitions.ReadMechanicsWord(
                TourianEntranceStatueInstructionProgramDefinitions.AdjacentUnusedProgram),
            "unused Tourian entrance-statue presentation program is rejected as mechanics");

        _ = ProbeTourianEntranceStatueInstructionAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeTourianEntranceStatueInstructionAllocation();
        AssertTrue(checksum != 0,
            "Tourian entrance-statue allocation probe consumes compiled data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Tourian entrance-statue mechanics lookups allocate no storage");

        Console.WriteLine(
            "Tourian entrance-statue instruction mechanics: three compiled delete " +
            "programs and all three production initializer selections pass.");
    }

    private static int ProbeTourianEntranceStatueInstructionAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += TourianEntranceStatueInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? TourianEntranceStatueInstructionProgramDefinitions.Ridley
                    : TourianEntranceStatueInstructionProgramDefinitions.BaseDecoration);
        }
        return checksum;
    }

    private static ushort ReadTourianEntranceStatueInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xaa0000 | address) |
            source.ReadByte(0xaa0000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class TourianEntranceStatueInstructionReadGuard(
        ISnesAddressSpace source) : ISnesAddressSpace
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (TourianEntranceStatueInstructionProgramDefinitions
                .IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Tourian entrance-statue mechanics byte " +
                    $"${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}

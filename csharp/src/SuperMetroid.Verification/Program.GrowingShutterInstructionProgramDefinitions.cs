using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyGrowingShutterInstructionProgramDefinitions()
    {
        VerifyGrowingShutterInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyGrowingShutterInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static |
            BindingFlags.NonPublic;
        for (int index = 0;
             index < GrowingShutterInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            GrowingShutterInstructionMechanicsWord definition =
                GrowingShutterInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadGrowingShutterInstructionWord(rom, 0xa20000 | definition.Address),
                $"growing-shutter instruction mechanics word $A2:{definition.Address:X4}");
        }

        var guard = new GrowingShutterInstructionProgramReadGuard(rom);
        var enemies = new RoomEnemySystem();
        Type enemySystemType = typeof(RoomEnemySystem);
        enemySystemType.GetField("_bus", flags)!.SetValue(enemies, guard);
        var initialize = enemySystemType.GetMethod("InitializeGrowingShutter", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        var finishSection = enemySystemType.GetMethod("FinishGrowingShutterSection", flags)!
            .CreateDelegate<Action<RoomEnemySlot, GrowingShutterEnemyState, ushort, bool>>();
        MethodInfo process = enemySystemType.GetMethod("ProcessInstructions", flags)!;

        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = RoomEnemySystem.GrowingShutterDefinition;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        slot.CurrentInstruction = 0;
        slot.ExtraProperties = 0;
        slot.Parameter2 = 0;
        slot.YPosition = 0x0100;
        initialize(slot);
        GrowingShutterEnemyState state = enemies.GrowingShutterStates[0]!;

        object?[] processArguments =
            [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        for (int programIndex = 0;
             programIndex < GrowingShutterInstructionProgramDefinitions.ProgramCount;
             programIndex++)
        {
            ushort entry =
                GrowingShutterInstructionProgramDefinitions.ProgramEntryPoint(programIndex);
            AssertEqual(entry, slot.CurrentInstruction,
                $"growing-shutter stage {programIndex} installs compiled program identity");
            process.Invoke(enemies, processArguments);
            AssertEqual(unchecked((ushort)(entry + 4)), slot.CurrentInstruction,
                $"growing-shutter stage {programIndex} advances to terminal sleep");
            AssertEqual((ushort)1, slot.InstructionTimer,
                $"growing-shutter stage {programIndex} frame duration");
            process.Invoke(enemies, processArguments);
            AssertEqual(unchecked((ushort)(entry + 4)), slot.CurrentInstruction,
                $"growing-shutter stage {programIndex} terminal sleep remains installed");

            finishSection(
                slot,
                state,
                unchecked((ushort)(0x0110 + programIndex * 0x10)),
                false);
        }

        AssertEqual((ushort)4, state.GrowthLevel,
            "growing-shutter real section transitions complete all four stages");
        AssertEqual(
            GrowingShutterInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "growing-shutter spritemap words remain cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled growing-shutter mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => GrowingShutterInstructionProgramDefinitions.ReadMechanicsWord(0xe99a),
            "growing-shutter spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => GrowingShutterInstructionProgramDefinitions.ReadMechanicsWord(0xe9b0),
            "adjacent vertical-shutter program is rejected as growing-shutter mechanics");

        _ = ProbeGrowingShutterInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeGrowingShutterInstructionMechanicsAllocation();
        AssertTrue(checksum != 0,
            "growing-shutter allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed growing-shutter mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Growing-shutter instruction mechanics: eight compiled words, four real " +
            "stage programs, and four live spritemap reads pass with mechanics bytes " +
            "forbidden.");
    }

    private static int ProbeGrowingShutterInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += GrowingShutterInstructionProgramDefinitions.ReadMechanicsWord(
                GrowingShutterInstructionProgramDefinitions.ProgramEntryPoint(index & 3));
        }
        return checksum;
    }

    private static ushort ReadGrowingShutterInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class GrowingShutterInstructionProgramReadGuard(
        ISnesAddressSpace source) : ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (GrowingShutterInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled growing-shutter mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa20000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < GrowingShutterInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        GrowingShutterInstructionProgramDefinitions.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        ObservedPresentationWords.Add(presentation);
                    }
                }
            }

            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}

using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyDeadSidehopperInstructionProgramDefinitions()
    {
        VerifyDeadSidehopperInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyDeadSidehopperInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < DeadSidehopperInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            DeadSidehopperInstructionMechanicsWord definition =
                DeadSidehopperInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadDeadSidehopperInstructionWord(rom, definition.Address),
                $"Dead sidehopper mechanics word $A9:{definition.Address:X4}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = new DeadSidehopperInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        Type type = typeof(RoomEnemySystem);
        type.GetField("_bus", flags)!.SetValue(enemies, guard);
        var initialize = type.GetMethod("InitializeDeadSidehopper", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        MethodInfo process = type.GetMethod("ProcessInstructions", flags)!;

        RoomEnemySlot corpse = enemies.Slots[0];
        corpse.EnemyDefinitionPointer = RoomEnemySystem.DeadSidehopperDefinition;
        corpse.Definition = default(RoomEnemyDefinition) with { Bank = 0xa9 };
        corpse.Parameter1 = 0;
        initialize(corpse);
        AssertEqual(DeadSidehopperInstructionProgramDefinitions.AliveIdle,
            corpse.CurrentInstruction,
            "real Dead sidehopper initializer installs compiled alive-idle program");

        object?[] processArguments =
            [corpse, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        ExecuteDeadSidehopperProgram(
            enemies,
            process,
            processArguments,
            corpse,
            DeadSidehopperInstructionProgramDefinitions.AliveIdle,
            frameCount: 1);
        ExecuteDeadSidehopperProgram(
            enemies,
            process,
            processArguments,
            corpse,
            DeadSidehopperInstructionProgramDefinitions.AliveCorpse,
            frameCount: 1);
        ExecuteDeadSidehopperProgram(
            enemies,
            process,
            processArguments,
            corpse,
            DeadSidehopperInstructionProgramDefinitions.InitiallyDead,
            frameCount: 1);
        ExecuteDeadSidehopperProgram(
            enemies,
            process,
            processArguments,
            corpse,
            DeadSidehopperInstructionProgramDefinitions.AliveHopping,
            frameCount: 8);

        AssertEqual(DeadSidehopperInstructionProgramDefinitions.HoppingSleepOpcode,
            corpse.CurrentInstruction,
            "Dead sidehopper hopping program executes end-hop callback and reaches sleep");
        AssertEqual(DeadSidehopperAiFunction.BeginPostLandingDelay,
            enemies.DeadSidehoppers[0]!.Function,
            "Dead sidehopper end-hop callback returns ownership to main AI");
        AssertEqual(DeadSidehopperInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all Dead sidehopper spritemap operands remain cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Dead sidehopper mechanics byte");
        AssertThrows<InvalidDataException>(
            () => DeadSidehopperInstructionProgramDefinitions.ReadMechanicsWord(
                DeadSidehopperInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Dead sidehopper spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => DeadSidehopperInstructionProgramDefinitions.ReadMechanicsWord(
                DeadSidehopperInstructionProgramDefinitions.FirstAdjacentProgram),
            "adjacent corpse program is rejected as Dead sidehopper mechanics");

        _ = ProbeDeadSidehopperInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeDeadSidehopperInstructionMechanicsAllocation();
        AssertTrue(checksum != 0,
            "Dead sidehopper allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Dead sidehopper mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Dead sidehopper instruction mechanics: sixteen compiled words, the real " +
            "initializer, all four programs, end-hop handoff, and eleven live spritemap " +
            "reads pass with mechanics bytes forbidden.");
    }

    private static void ExecuteDeadSidehopperProgram(
        RoomEnemySystem enemies,
        MethodInfo process,
        object?[] processArguments,
        RoomEnemySlot corpse,
        ushort program,
        int frameCount)
    {
        corpse.CurrentInstruction = program;
        for (int frame = 0; frame <= frameCount; frame++)
        {
            corpse.InstructionTimer = 1;
            process.Invoke(enemies, processArguments);
        }
    }

    private static int ProbeDeadSidehopperInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += DeadSidehopperInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? DeadSidehopperInstructionProgramDefinitions.AliveHopping
                    : DeadSidehopperInstructionProgramDefinitions.HoppingSleepOpcode);
        }
        return checksum;
    }

    private static ushort ReadDeadSidehopperInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa90000 | address) |
            source.ReadByte(0xa90000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class DeadSidehopperInstructionReadGuard(
        ISnesAddressSpace source) : ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (DeadSidehopperInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Dead sidehopper mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa90000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < DeadSidehopperInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        DeadSidehopperInstructionProgramDefinitions.PresentationWordAddress(index);
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

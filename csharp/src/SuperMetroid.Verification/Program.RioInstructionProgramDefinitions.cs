using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyRioInstructionProgramDefinitions()
    {
        VerifyRioInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyRioInstructionProgramDefinitions(SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < RioInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            RioInstructionMechanicsWord definition =
                RioInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadRioInstructionWord(rom, definition.Address),
                $"Rio mechanics word $A2:{definition.Address:X4}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = new RioInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        Type type = typeof(RoomEnemySystem);
        type.GetField("_bus", flags)!.SetValue(enemies, guard);
        var initialize = type.GetMethod("InitializeRio", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        MethodInfo process = type.GetMethod("ProcessInstructions", flags)!;

        RoomEnemySlot rio = enemies.Slots[0];
        rio.EnemyDefinitionPointer = RoomEnemySystem.RioDefinition;
        rio.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        initialize(rio);
        AssertEqual(RioInstructionProgramDefinitions.Idle,
            rio.CurrentInstruction,
            "real Rio initializer installs compiled idle program");

        object?[] processArguments =
            [rio, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        ExecuteRioProgram(
            enemies,
            process,
            processArguments,
            rio,
            RioInstructionProgramDefinitions.Idle,
            callCount: 13);
        AssertEqual(unchecked((ushort)(RioInstructionProgramDefinitions.Idle + 4)),
            rio.CurrentInstruction,
            "idle fallthrough and post-swoop program return to first repeated idle frame");

        RioEnemyState state = enemies.RioStates[0]!;
        state.AnimationFinished = false;
        ExecuteRioProgram(
            enemies,
            process,
            processArguments,
            rio,
            RioInstructionProgramDefinitions.SwoopingPart1,
            callCount: 6);
        AssertEqual(unchecked((ushort)(RioInstructionProgramDefinitions.SwoopingPart1 + 22)),
            rio.CurrentInstruction,
            "first swoop program reaches terminal sleep");
        AssertTrue(state.AnimationFinished,
            "first swoop program publishes animation completion");

        ExecuteRioProgram(
            enemies,
            process,
            processArguments,
            rio,
            RioInstructionProgramDefinitions.SwoopingPart2,
            callCount: 3);
        AssertEqual(unchecked((ushort)(RioInstructionProgramDefinitions.SwoopingPart2 + 4)),
            rio.CurrentInstruction,
            "second swoop program loops and installs its first repeated frame");

        state.AnimationFinished = false;
        ExecuteRioProgram(
            enemies,
            process,
            processArguments,
            rio,
            RioInstructionProgramDefinitions.SwoopCooldown,
            callCount: 6);
        AssertEqual(unchecked((ushort)(RioInstructionProgramDefinitions.SwoopCooldown + 22)),
            rio.CurrentInstruction,
            "swoop cooldown reaches terminal sleep");
        AssertTrue(state.AnimationFinished,
            "swoop cooldown publishes animation completion");

        AssertEqual(RioInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all Rio spritemap operands remain cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Rio mechanics byte");
        AssertThrows<InvalidDataException>(
            () => RioInstructionProgramDefinitions.ReadMechanicsWord(
                RioInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Rio spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => RioInstructionProgramDefinitions.ReadMechanicsWord(
                RioInstructionProgramDefinitions.FirstAdjacentMechanicsData),
            "adjacent Rio launch data is rejected as instruction mechanics");

        _ = ProbeRioInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeRioInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Rio allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Rio mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Rio instruction mechanics: thirty-two compiled words, the real initializer, " +
            "idle fallthrough/return, both swoop phases, cooldown completion, and twenty-four " +
            "live spritemap reads pass with mechanics bytes forbidden.");
    }

    private static void ExecuteRioProgram(
        RoomEnemySystem enemies,
        MethodInfo process,
        object?[] processArguments,
        RoomEnemySlot rio,
        ushort program,
        int callCount)
    {
        rio.CurrentInstruction = program;
        for (int call = 0; call < callCount; call++)
        {
            rio.InstructionTimer = 1;
            process.Invoke(enemies, processArguments);
        }
    }

    private static int ProbeRioInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += RioInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? RioInstructionProgramDefinitions.Idle
                    : unchecked((ushort)(RioInstructionProgramDefinitions.SwoopCooldown + 22)));
        }
        return checksum;
    }

    private static ushort ReadRioInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa20000 | address) |
            source.ReadByte(0xa20000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class RioInstructionReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (RioInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Rio mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa20000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < RioInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        RioInstructionProgramDefinitions.PresentationWordAddress(index);
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

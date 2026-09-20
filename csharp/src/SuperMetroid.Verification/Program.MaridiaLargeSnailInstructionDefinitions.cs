using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyMaridiaLargeSnailInstructionDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags instance = BindingFlags.Instance | BindingFlags.NonPublic;
        MethodInfo install = typeof(RoomEnemySystem).GetMethod(
            "InstallMaridiaLargeSnailInstructionList",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        FieldInfo busField = typeof(RoomEnemySystem).GetField("_bus", instance)!;
        var guarded = new MaridiaLargeSnailInstructionReadGuard(rom);

        for (ushort animationIndex = 0; animationIndex < 8; animationIndex++)
        {
            ushort expected = ReadMaridiaLargeSnailInstructionWord(
                rom, 0xa2cb77 + animationIndex * 2);
            AssertEqual(expected,
                MaridiaLargeSnailInstructionDefinitions.InstructionPointer(animationIndex),
                $"Maridia Large Snail instruction selector {animationIndex}");

            var enemies = new RoomEnemySystem();
            busField.SetValue(enemies, guarded);
            RoomEnemySlot slot = enemies.Slots[0];
            var state = new MaridiaLargeSnailEnemyState(slot)
            {
                RequestedInstructionListIndex = animationIndex,
                InstalledInstructionListIndex = ushort.MaxValue,
            };
            slot.InstructionTimer = 0x1234;
            slot.Timer = 0x5678;

            install.Invoke(null, [slot, state]);
            AssertEqual(animationIndex, state.InstalledInstructionListIndex,
                $"production Maridia Large Snail installed selector {animationIndex}");
            AssertEqual(expected, slot.CurrentInstruction,
                $"production Maridia Large Snail instruction {animationIndex}");
            AssertEqual((ushort)1, slot.InstructionTimer,
                $"production Maridia Large Snail instruction timer {animationIndex}");
            AssertEqual((ushort)0, slot.Timer,
                $"production Maridia Large Snail general timer {animationIndex}");
        }

        for (int index = 0;
             index < MaridiaLargeSnailInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            MaridiaLargeSnailInstructionMechanicsWord definition =
                MaridiaLargeSnailInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadMaridiaLargeSnailInstructionWord(
                    rom, 0xa20000 | definition.Address),
                $"Maridia Large Snail mechanics word $A2:{definition.Address:X4}");
        }

        (ushort Program, int Calls, ushort Terminal, bool FinishesAttack, bool PlaysSplash,
            string Name)[] programs =
        [
            (MaridiaLargeSnailInstructionProgramDefinitions.FacingLeftIdle, 2, 0xca4f,
                false, false, "left idle"),
            (MaridiaLargeSnailInstructionProgramDefinitions.FacingLeftAttacking, 14, 0xca89,
                true, true, "left attack"),
            (MaridiaLargeSnailInstructionProgramDefinitions.FacingLeftRollingForwards, 9,
                0xca8f, false, false, "left forward roll"),
            (MaridiaLargeSnailInstructionProgramDefinitions.FacingLeftRollingBackwards, 9,
                0xcab7, false, false, "left backward roll"),
            (MaridiaLargeSnailInstructionProgramDefinitions.FacingRightIdle, 2, 0xcadf,
                false, false, "right idle"),
            (MaridiaLargeSnailInstructionProgramDefinitions.FacingRightAttacking, 14, 0xcb19,
                true, true, "right attack"),
            (MaridiaLargeSnailInstructionProgramDefinitions.FacingRightRollingForwards, 9,
                0xcb1f, false, false, "right forward roll"),
            (MaridiaLargeSnailInstructionProgramDefinitions.FacingRightRollingBackwards, 9,
                0xcb47, false, false, "right backward roll"),
        ];

        foreach (var program in programs)
        {
            var enemies = new RoomEnemySystem();
            busField.SetValue(enemies, guarded);
            var initialize = typeof(RoomEnemySystem).GetMethod(
                "InitializeMaridiaLargeSnail", instance)!
                .CreateDelegate<Action<RoomEnemySlot>>(enemies);
            MethodInfo process = typeof(RoomEnemySystem).GetMethod(
                "ProcessInstructions", instance)!;
            RoomEnemySlot snail = enemies.Slots[0];
            snail.EnemyDefinitionPointer = RoomEnemySystem.MaridiaLargeSnailDefinition;
            snail.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
            initialize(snail);

            AssertEqual(MaridiaLargeSnailInstructionProgramDefinitions.FacingLeftIdle,
                snail.CurrentInstruction,
                $"real Maridia Large Snail initializer before {program.Name}");
            ExecuteMaridiaLargeSnailProgram(
                enemies, process, snail, program.Program, program.Calls);
            AssertEqual(program.Terminal, snail.CurrentInstruction,
                $"Maridia Large Snail {program.Name} terminal instruction");

            MaridiaLargeSnailEnemyState state = enemies.MaridiaLargeSnailStates[0]!;
            AssertEqual(program.FinishesAttack, state.AttackAnimationFinished,
                $"Maridia Large Snail {program.Name} completion flag");
            AssertEqual(program.PlaysSplash ? (ushort?)0x000e : null,
                enemies.LastMaridiaLargeSnailSoundEffect,
                $"Maridia Large Snail {program.Name} splash sound");
            AssertEqual(false, state.AttackAllowsRotation,
                $"Maridia Large Snail {program.Name} leaves rotation disallowed");
        }

        {
            var enemies = new RoomEnemySystem();
            busField.SetValue(enemies, guarded);
            var initialize = typeof(RoomEnemySystem).GetMethod(
                "InitializeMaridiaLargeSnail", instance)!
                .CreateDelegate<Action<RoomEnemySlot>>(enemies);
            MethodInfo process = typeof(RoomEnemySystem).GetMethod(
                "ProcessInstructions", instance)!;
            RoomEnemySlot snail = enemies.Slots[0];
            snail.EnemyDefinitionPointer = RoomEnemySystem.MaridiaLargeSnailDefinition;
            snail.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
            initialize(snail);
            ExecuteMaridiaLargeSnailProgram(
                enemies,
                process,
                snail,
                MaridiaLargeSnailInstructionProgramDefinitions.FacingLeftRollingForwards,
                2);
            AssertTrue(enemies.MaridiaLargeSnailStates[0]!.AttackAllowsRotation,
                "Maridia Large Snail forward roll enables its attack window");
            ExecuteMaridiaLargeSnailProgram(
                enemies,
                process,
                snail,
                snail.CurrentInstruction,
                1);
            AssertEqual(false, enemies.MaridiaLargeSnailStates[0]!.AttackAllowsRotation,
                "Maridia Large Snail forward roll closes its attack window");
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => MaridiaLargeSnailInstructionDefinitions.InstructionPointer(8),
            "Maridia Large Snail selector past table");
        AssertEqual(MaridiaLargeSnailInstructionProgramDefinitions.PresentationWordCount,
            guarded.ObservedPresentationWords.Count,
            "all Maridia Large Snail extended-spritemap operands remain cartridge reads");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "production execution avoids compiled Maridia Large Snail mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => MaridiaLargeSnailInstructionProgramDefinitions.ReadMechanicsWord(
                MaridiaLargeSnailInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Maridia Large Snail presentation pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => MaridiaLargeSnailInstructionProgramDefinitions.ReadMechanicsWord(
                MaridiaLargeSnailInstructionProgramDefinitions.FirstAdjacentMechanicsData),
            "adjacent Maridia Large Snail selector data is rejected as mechanics");

        _ = ProbeMaridiaLargeSnailInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeMaridiaLargeSnailInstructionMechanicsAllocation();
        AssertTrue(checksum != 0,
            "Maridia Large Snail allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Maridia Large Snail mechanics lookups allocate no per-frame storage");
        Console.WriteLine(
            "Maridia Large Snail instruction mechanics: all eight native selectors, " +
            "eighty-four compiled mechanics words, all eight programs and callback side " +
            "effects, and sixty live presentation reads pass with mechanics bytes forbidden.");
    }

    private static void ExecuteMaridiaLargeSnailProgram(
        RoomEnemySystem enemies,
        MethodInfo process,
        RoomEnemySlot snail,
        ushort program,
        int callCount)
    {
        snail.CurrentInstruction = program;
        object?[] arguments =
            [snail, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        for (int call = 0; call < callCount; call++)
        {
            snail.InstructionTimer = 1;
            process.Invoke(enemies, arguments);
        }
    }

    private static int ProbeMaridiaLargeSnailInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += MaridiaLargeSnailInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? MaridiaLargeSnailInstructionProgramDefinitions.FacingLeftIdle
                    : unchecked((ushort)(
                        MaridiaLargeSnailInstructionProgramDefinitions.FacingRightAttacking +
                        0x38)));
        }
        return checksum;
    }

    private static ushort ReadMaridiaLargeSnailInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class MaridiaLargeSnailInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (address is >= 0xa2cb77 and < 0xa2cb87 ||
                MaridiaLargeSnailInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Maridia Large Snail attempted migrated mechanics read ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa20000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < MaridiaLargeSnailInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        MaridiaLargeSnailInstructionProgramDefinitions.PresentationWordAddress(index);
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

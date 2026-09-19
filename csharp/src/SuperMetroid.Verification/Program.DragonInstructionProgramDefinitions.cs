using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyDragonInstructionProgramDefinitions()
    {
        VerifyDragonInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyDragonInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        for (int index = 0;
             index < DragonInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            DragonInstructionMechanicsWord definition =
                DragonInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadDragonInstructionWord(rom, 0xa20000 | definition.Address),
                $"Dragon instruction mechanics word $A2:{definition.Address:X4}");
        }

        var guard = new DragonInstructionProgramReadGuard(rom);
        (DragonAnimationSelector Selector, ushort Terminal, int Frames)[] programs =
        [
            (DragonAnimationSelector.IdleFacingLeft, 0xe59f, 2),
            (DragonAnimationSelector.IdleFacingRight, 0xe5b1, 2),
            (DragonAnimationSelector.WingsFacingLeft, 0xe5a5, 11),
            (DragonAnimationSelector.WingsFacingRight, 0xe5b7, 11),
            (DragonAnimationSelector.AttackingFacingLeft, 0xe5d5, 47),
            (DragonAnimationSelector.AttackingFacingRight, 0xe5ed, 47),
        ];

        foreach ((DragonAnimationSelector selector, ushort terminal, int frames) in programs)
        {
            RoomEnemySystem enemies =
                CreateDragonProgramSystem(guard, selector, out RoomEnemySlot slot);
            DragonEnemyState state = enemies.DragonStates[0]!;
            AssertEqual(DragonAnimationDefinitions.InstructionList(selector),
                slot.CurrentInstruction, $"Dragon installs {selector} program");
            RunDragonProgram(enemies, slot, frames);
            AssertEqual(terminal, slot.CurrentInstruction,
                $"Dragon {selector} program reaches its native loop/sleep cursor");
            bool attacks = selector is DragonAnimationSelector.AttackingFacingLeft or
                DragonAnimationSelector.AttackingFacingRight;
            AssertEqual(attacks, state.AnimationFinished,
                $"Dragon {selector} attack-completion callback state");
        }

        AssertEqual(DragonInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all live Dragon spritemap words remain cartridge reads");
        for (int index = 0;
             index < DragonInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                DragonInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Dragon presentation word $A2:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Dragon mechanics byte");

        AssertThrows<InvalidDataException>(
            () => DragonInstructionProgramDefinitions.ReadMechanicsWord(0xe59d),
            "interleaved Dragon spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => DragonInstructionProgramDefinitions.ReadMechanicsWord(0xe5ef),
            "adjacent Dragon selector table is rejected as mechanics");

        _ = ProbeDragonInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeDragonInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Dragon allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Dragon mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Dragon instruction mechanics: twenty-six compiled words, all six body, " +
            "wing, and attack programs, both completion callbacks, and sixteen live " +
            "spritemap reads pass with mechanics bytes forbidden.");

        static RoomEnemySystem CreateDragonProgramSystem(
            DragonInstructionProgramReadGuard guard,
            DragonAnimationSelector selector,
            out RoomEnemySlot slot)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            var initialize = typeof(RoomEnemySystem).GetMethod("InitializeDragon", flags)!
                .CreateDelegate<Action<RoomEnemySlot>>(enemies);
            MethodInfo install = typeof(RoomEnemySystem).GetMethod(
                "InstallDragonInstructionList", flags)!;

            slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.DragonDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
            slot.Parameter1 = selector is DragonAnimationSelector.WingsFacingLeft or
                DragonAnimationSelector.WingsFacingRight ? (ushort)1 : (ushort)0;
            initialize(slot);
            DragonEnemyState state = enemies.DragonStates[0]!;
            state.InstalledInstructionListIndex = DragonAnimationSelector.ForceReinstall;
            state.RequestedInstructionListIndex = selector;
            state.AnimationFinished = false;
            install.Invoke(null, [slot, state]);
            return enemies;
        }

        static void RunDragonProgram(RoomEnemySystem enemies, RoomEnemySlot slot, int frames)
        {
            MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            for (int frame = 0; frame < frames; frame++)
                process.Invoke(enemies, arguments);
        }
    }

    private static int ProbeDragonInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += DragonInstructionProgramDefinitions.ReadMechanicsWord(
                DragonInstructionProgramDefinitions.IdleFacingLeft);
        }
        return checksum;
    }

    private static ushort ReadDragonInstructionWord(SuperMetroidAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class DragonInstructionProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (DragonInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Dragon mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa20000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < DragonInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        DragonInstructionProgramDefinitions.PresentationWordAddress(index);
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

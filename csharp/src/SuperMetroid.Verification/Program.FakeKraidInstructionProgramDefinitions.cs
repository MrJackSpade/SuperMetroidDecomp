using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyFakeKraidInstructionProgramDefinitions()
    {
        VerifyFakeKraidInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyFakeKraidInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        int population = EnemyRomTablePointers.Kraid.FakeKraidPopulationRecord;
        ushort populationDefinition = (ushort)(rom.ReadByte(population) |
            rom.ReadByte(population + 1) << 8);
        ushort extraProperties = (ushort)(rom.ReadByte(population + 10) |
            rom.ReadByte(population + 11) << 8);
        AssertEqual(RoomEnemySystem.FakeKraidDefinition, populationDefinition,
            "retail Fake Kraid population matches the installed visual family");
        AssertTrue((extraProperties &
                (ushort)EnemyExtraProperties.UsesExtendedSpritemap) == 0,
            "retail Fake Kraid uses ordinary OAM composition");

        for (int index = 0;
             index < FakeKraidInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            FakeKraidInstructionMechanicsWord definition =
                FakeKraidInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadFakeKraidInstructionWord(rom, 0xa60000 | definition.Address),
                $"Fake Kraid instruction mechanics word $A6:{definition.Address:X4}");
        }

        var guard = new FakeKraidInstructionProgramReadGuard(rom);
        var level = new RoomLevelData(
            32,
            32,
            new ushort[1024],
            new byte[1024],
            new ushort[1024],
            new byte[8]);

        VerifyInitializer(movingRight: false);
        VerifyInitializer(movingRight: true);

        ExerciseWalk(
            FakeKraidInstructionProgramDefinitions.StepForwardsFacingLeft,
            movingRight: false,
            walkDelta: -4);
        ExerciseWalk(
            FakeKraidInstructionProgramDefinitions.StepBackwardsFacingLeft,
            movingRight: false,
            walkDelta: 4);
        ExerciseWalk(
            FakeKraidInstructionProgramDefinitions.StepForwardsFacingRight,
            movingRight: true,
            walkDelta: 4);
        ExerciseWalk(
            FakeKraidInstructionProgramDefinitions.StepBackwardsFacingRight,
            movingRight: true,
            walkDelta: -4);
        ExerciseSpit(
            FakeKraidInstructionProgramDefinitions.FireSpitFacingLeft,
            movingRight: false);
        ExerciseSpit(
            FakeKraidInstructionProgramDefinitions.FireSpitFacingRight,
            movingRight: true);

        AssertEqual(0, guard.ForbiddenPresentationReadAttempts,
            "Fake Kraid programs never read installed visual selectors");
        for (int index = 0;
             index < FakeKraidInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                FakeKraidInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertEqual(ReadFakeKraidInstructionWord(rom, 0xa60000 | address),
                KraidVisualDefinitions.FrameAt(
                    RoomEnemySystem.FakeKraidDefinition, address),
                $"compiled Fake Kraid frame $A6:{address:X4}");
        }
        AssertThrows<InvalidDataException>(
            () => KraidVisualDefinitions.FrameAt(
                RoomEnemySystem.FakeKraidDefinition, 0x9a42),
            "unlisted Fake Kraid visual operand fails loudly");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Fake Kraid mechanics byte");

        AssertThrows<InvalidDataException>(
            () => FakeKraidInstructionProgramDefinitions.ReadMechanicsWord(0x99b0),
            "interleaved Fake Kraid spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => FakeKraidInstructionProgramDefinitions.ReadMechanicsWord(0x99f4),
            "retail-unused Fake Kraid standing program is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => FakeKraidInstructionProgramDefinitions.ReadMechanicsWord(0x9a42),
            "adjacent Fake Kraid standing program is rejected as mechanics");

        _ = ProbeFakeKraidInstructionMechanicsAllocation();
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeFakeKraidInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Fake Kraid allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - allocatedBefore,
            "warmed Fake Kraid mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Fake Kraid instruction mechanics: forty-eight compiled words, all six live " +
            "programs and four action selectors pass with twenty-four visual selectors " +
            "and mechanics source words forbidden.");

        void VerifyInitializer(bool movingRight)
        {
            RoomEnemySystem enemies = CreateSystem(
                movingRight,
                out RoomEnemySlot slot,
                out _,
                out _);
            AssertEqual(
                movingRight
                    ? FakeKraidInstructionProgramDefinitions.StepForwardsFacingRight
                    : FakeKraidInstructionProgramDefinitions.StepForwardsFacingLeft,
                slot.CurrentInstruction,
                $"Fake Kraid initializer selects {(movingRight ? "right" : "left")} program");
        }

        void ExerciseWalk(ushort program, bool movingRight, short walkDelta)
        {
            RoomEnemySystem enemies = CreateSystem(
                movingRight,
                out RoomEnemySlot slot,
                out SamusState samus,
                out FakeKraidEnemyState state);
            slot.CurrentInstruction = program;
            slot.InstructionTimer = 1;
            state.WalkDelta = walkDelta;
            state.FacingDelta = movingRight ? (short)4 : (short)-4;
            state.WalkStepTimer = 0x0100;
            state.SpitDecisionTimer = 0x0100;

            RunFrames(enemies, slot, samus, level, 60);

            AssertEqual((ushort)0x00ff, state.WalkStepTimer,
                $"Fake Kraid $A6:{program:X4} executes its walk callback");
            AssertEqual((ushort)0x00ff, state.SpitDecisionTimer,
                $"Fake Kraid $A6:{program:X4} executes its action callback");
        }

        void ExerciseSpit(ushort program, bool movingRight)
        {
            RoomEnemySystem enemies = CreateSystem(
                movingRight,
                out RoomEnemySlot slot,
                out SamusState samus,
                out FakeKraidEnemyState state);
            slot.CurrentInstruction = program;
            slot.InstructionTimer = 1;
            state.WalkStepTimer = 0x0100;
            state.SpitDecisionTimer = 0x0100;

            RunFrames(enemies, slot, samus, level, 60);

            AssertEqual(2, state.SpawnedSpitCount,
                $"Fake Kraid $A6:{program:X4} spawns its projectile pair");
            AssertEqual((ushort?)0x0016, enemies.LastFakeKraidSoundEffect,
                $"Fake Kraid $A6:{program:X4} publishes its cry");
        }

        RoomEnemySystem CreateSystem(
            bool movingRight,
            out RoomEnemySlot slot,
            out SamusState samus,
            out FakeKraidEnemyState state)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            typeof(RoomEnemySystem).GetField("_readRandomNumber", flags)!
                .SetValue(enemies, (Func<ushort>)(() => 0));
            var initialize = typeof(RoomEnemySystem).GetMethod("InitializeFakeKraid", flags)!
                .CreateDelegate<Action<RoomEnemySlot, SamusState?>>(enemies);

            slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.FakeKraidDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa6 };
            slot.XPosition = 0x0100;
            slot.YPosition = 0x0100;
            samus = new SamusState
            {
                XPosition = movingRight ? (ushort)0x0140 : (ushort)0x00c0,
                YPosition = slot.YPosition,
            };
            initialize(slot, samus);
            state = enemies.FakeKraidStates[0]!;
            return enemies;
        }

        static void RunFrames(
            RoomEnemySystem enemies,
            RoomEnemySlot slot,
            SamusState samus,
            RoomLevelData level,
            int frames)
        {
            MethodInfo process = typeof(RoomEnemySystem).GetMethod(
                "ProcessInstructions", flags)!;
            object?[] arguments =
                [slot, samus, level, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            for (int frame = 0; frame < frames; frame++)
                process.Invoke(enemies, arguments);
        }
    }

    private static int ProbeFakeKraidInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += FakeKraidInstructionProgramDefinitions.ReadMechanicsWord(
                FakeKraidInstructionProgramDefinitions.StepForwardsFacingLeft);
        }
        return checksum;
    }

    private static ushort ReadFakeKraidInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class FakeKraidInstructionProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal int ForbiddenPresentationReadAttempts { get; private set; }
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (FakeKraidInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Fake Kraid mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa60000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < FakeKraidInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        FakeKraidInstructionProgramDefinitions.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        ForbiddenPresentationReadAttempts++;
                        throw new InvalidOperationException(
                            $"Production read installed Fake Kraid selector ${address:X6}.");
                    }
                }
            }

            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}

using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCeresDoorInstructionProgramDefinitions()
    {
        VerifyCeresDoorInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyCeresDoorInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        for (int index = 0;
             index < CeresDoorInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            CeresDoorInstructionMechanicsWord definition =
                CeresDoorInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadCeresDoorInstructionWord(rom, 0xa60000 | definition.Address),
                $"Ceres door instruction mechanics word $A6:{definition.Address:X4}");
        }
        for (int index = 0;
             index < CeresDoorInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                CeresDoorInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertEqual(CeresDoorInstructionProgramDefinitions.PresentationWordFrame(index),
                ReadCeresDoorInstructionWord(rom, 0xa60000 | address),
                $"Ceres door visual selector $A6:{address:X4}");
            AssertEqual(CeresDoorInstructionProgramDefinitions.PresentationWordFrame(index),
                CeresDoorInstructionProgramDefinitions.ReadPresentationFrame(address),
                $"compiled Ceres door visual selector $A6:{address:X4}");
        }

        var guard = new CeresDoorInstructionProgramReadGuard(rom);
        bool bossDefeated = false;

        RoomEnemySystem ridleyRoom = CreateSystem(
            CeresDoorInstructionProgramDefinitions.RidleyRoomFacingRight,
            ceresStatus: 0,
            () => bossDefeated,
            out RoomEnemySlot ridleyDoor,
            out SamusState ridleySamus);
        SetNear(ridleyDoor, ridleySamus);
        RunFrames(ridleyRoom, ridleyDoor, ridleySamus, 20);
        AssertEqual(CeresDoorInstructionProgramDefinitions.RidleyRoomFacingRightWait + 4,
            ridleyDoor.CurrentInstruction,
            "Ridley-room Ceres door loops while the area boss is alive");
        AssertEqual((ushort)1, ridleyDoor.VariableB,
            "Ridley-room Ceres door publishes its Ridley-drawn flag");
        bossDefeated = true;
        RunFrames(ridleyRoom, ridleyDoor, ridleySamus, 4);
        AssertEqual((ushort)0, ridleyDoor.VariableB,
            "Ridley-room Ceres door clears its Ridley-drawn flag after defeat");

        ExerciseOrdinaryDoor(
            CeresDoorInstructionProgramDefinitions.NormalFacingRight,
            facingRight: true);
        ExerciseOrdinaryDoor(
            CeresDoorInstructionProgramDefinitions.NormalFacingLeft,
            facingRight: false);

        ExerciseLoop(
            CeresDoorInstructionProgramDefinitions.RotatingElevatorPreExplosionOverlay,
            ceresStatus: 0,
            expectedLoop: CeresDoorInstructionProgramDefinitions
                .RotatingElevatorPreExplosionOverlayLoop);
        ExerciseLoop(
            CeresDoorInstructionProgramDefinitions.RotatingElevatorInvisibleWall,
            ceresStatus: 1,
            expectedLoop: CeresDoorInstructionProgramDefinitions
                .RotatingElevatorInvisibleWallLoop);
        ExerciseLoop(
            CeresDoorInstructionProgramDefinitions.RidleyEscapeMode7LeftWall,
            ceresStatus: 0,
            expectedLoop: CeresDoorInstructionProgramDefinitions
                .RidleyEscapeMode7LeftWallLoop);
        ExerciseLoop(
            CeresDoorInstructionProgramDefinitions.RidleyEscapeMode7RightWall,
            ceresStatus: 0,
            expectedLoop: CeresDoorInstructionProgramDefinitions
                .RidleyEscapeMode7RightWallLoop);

        RoomEnemySystem preEscapeWall = CreateSystem(
            CeresDoorInstructionProgramDefinitions.RotatingElevatorInvisibleWall,
            ceresStatus: 0,
            () => false,
            out RoomEnemySlot preEscapeSlot,
            out SamusState preEscapeSamus);
        SetFar(preEscapeSlot, preEscapeSamus);
        RunFrames(preEscapeWall, preEscapeSlot, preEscapeSamus, 3);
        AssertEqual(CeresDoorInstructionProgramDefinitions.ClosedFacingLeft + 8,
            preEscapeSlot.CurrentInstruction,
            "pre-escape invisible wall branches into the normal left-door program");

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Ceres door mechanics and selector byte");

        AssertThrows<InvalidDataException>(
            () => CeresDoorInstructionProgramDefinitions.ReadMechanicsWord(0xf540),
            "interleaved Ceres door spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => CeresDoorInstructionProgramDefinitions.ReadMechanicsWord(0xf63e),
            "adjacent Ceres door callback code is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => CeresDoorInstructionProgramDefinitions.ReadPresentationFrame(0xf53e),
            "interleaved Ceres door duration is rejected as a visual selector");
        AssertThrows<InvalidDataException>(
            () => CeresDoorInstructionProgramDefinitions.ReadPresentationFrame(0xf63e),
            "adjacent Ceres door callback code is rejected as a visual selector");

        _ = ProbeCeresDoorInstructionMechanicsAllocation();
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeCeresDoorInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Ceres door allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - allocatedBefore,
            "warmed Ceres door mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Ceres door instruction mechanics: ninety-seven compiled words, all seven " +
            "variants and thirty-three compiled visual selectors pass with ROM reads forbidden.");

        void ExerciseOrdinaryDoor(ushort program, bool facingRight)
        {
            RoomEnemySystem enemies = CreateSystem(
                program,
                ceresStatus: 0,
                () => false,
                out RoomEnemySlot slot,
                out SamusState samus);
            SetNear(slot, samus);
            RunFrames(enemies, slot, samus, 5);

            SetFar(slot, samus);
            RunFrames(enemies, slot, samus, 30);
            AssertTrue(!slot.Properties.HasAny(EnemyProperties.Invisible),
                $"ordinary {(facingRight ? "right" : "left")} door closes visibly");

            SetNear(slot, samus);
            RunFrames(enemies, slot, samus, 30);
            AssertTrue(slot.Properties.HasAny(
                    EnemyProperties.Invisible | EnemyProperties.IgnoreSamusCollision),
                $"ordinary {(facingRight ? "right" : "left")} door opens and clears collision");
        }

        void ExerciseLoop(ushort program, ushort ceresStatus, ushort expectedLoop)
        {
            RoomEnemySystem enemies = CreateSystem(
                program,
                ceresStatus,
                () => false,
                out RoomEnemySlot slot,
                out SamusState samus);
            RunFrames(enemies, slot, samus, 3);
            AssertEqual(unchecked((ushort)(expectedLoop + 4)), slot.CurrentInstruction,
                $"Ceres door program $A6:{program:X4} loops at its authored frame");
        }

        RoomEnemySystem CreateSystem(
            ushort program,
            ushort ceresStatus,
            Func<bool> isBossDefeated,
            out RoomEnemySlot slot,
            out SamusState samus)
        {
            var enemies = new RoomEnemySystem { CeresStatus = ceresStatus };
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            typeof(RoomEnemySystem).GetField("_isAreaBossDefeated", flags)!
                .SetValue(enemies, isBossDefeated);
            slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer =
                CeresDoorInstructionProgramDefinitions.EnemyDefinitionPointer;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa6 };
            slot.XPosition = 0x0080;
            slot.YPosition = 0x0080;
            slot.CurrentInstruction = program;
            slot.InstructionTimer = 1;
            samus = new SamusState();
            return enemies;
        }

        static void SetNear(RoomEnemySlot slot, SamusState samus)
        {
            samus.XPosition = slot.XPosition;
            samus.YPosition = slot.YPosition;
        }

        static void SetFar(RoomEnemySlot slot, SamusState samus)
        {
            samus.XPosition = unchecked((ushort)(slot.XPosition + 0x0080));
            samus.YPosition = slot.YPosition;
        }

        static void RunFrames(
            RoomEnemySystem enemies,
            RoomEnemySlot slot,
            SamusState samus,
            int frames)
        {
            MethodInfo process = typeof(RoomEnemySystem).GetMethod(
                "ProcessInstructions", flags)!;
            object?[] arguments =
                [slot, samus, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            for (int frame = 0; frame < frames; frame++)
                process.Invoke(enemies, arguments);
        }
    }

    private static int ProbeCeresDoorInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += CeresDoorInstructionProgramDefinitions.ReadMechanicsWord(
                CeresDoorInstructionProgramDefinitions.NormalFacingRight);
        }
        return checksum;
    }

    private static ushort ReadCeresDoorInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class CeresDoorInstructionProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (CeresDoorInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Ceres door mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa60000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < CeresDoorInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        CeresDoorInstructionProgramDefinitions.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        ForbiddenReadAttempts++;
                        throw new InvalidOperationException(
                            $"Production read compiled Ceres door visual selector byte ${address:X6}.");
                    }
                }
            }

            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}

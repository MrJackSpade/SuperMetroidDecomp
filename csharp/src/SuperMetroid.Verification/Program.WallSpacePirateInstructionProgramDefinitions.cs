using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyWallSpacePirateInstructionProgramDefinitions()
    {
        VerifyWallSpacePirateInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyWallSpacePirateInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < WallSpacePirateInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            WallSpacePirateInstructionMechanicsWord definition =
                WallSpacePirateInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadWallPirateWord(rom, 0xb20000 | definition.Address),
                $"wall Pirate mechanics word $B2:{definition.Address:X4}");
        }

        var guard = new WallSpacePirateInstructionReadGuard(rom);
        RoomLevelData level = CreateOpenWallPirateRoom();
        VerifyAttack(movingRight: false);
        VerifyAttack(movingRight: true);
        VerifyClimb(
            WallSpacePirateInstructionProgramDefinitions.LandedOnLeftWall,
            expectedFunction: WallSpacePirateFunction.ClimbingLeftWall,
            frames: 12);
        VerifyClimb(
            WallSpacePirateInstructionProgramDefinitions.MovingUpLeftWall,
            expectedFunction: WallSpacePirateFunction.ClimbingLeftWall,
            frames: 260);
        VerifyClimb(
            WallSpacePirateInstructionProgramDefinitions.MovingDownLeftWall,
            expectedFunction: WallSpacePirateFunction.ClimbingLeftWall,
            frames: 260);
        VerifyClimb(
            WallSpacePirateInstructionProgramDefinitions.LandedOnRightWall,
            expectedFunction: WallSpacePirateFunction.ClimbingRightWall,
            frames: 12);
        VerifyClimb(
            WallSpacePirateInstructionProgramDefinitions.MovingDownRightWall,
            expectedFunction: WallSpacePirateFunction.ClimbingRightWall,
            frames: 260);
        VerifyClimb(
            WallSpacePirateInstructionProgramDefinitions.MovingUpRightWall,
            expectedFunction: WallSpacePirateFunction.ClimbingRightWall,
            frames: 260);
        VerifyCollisionReversal(onRightWall: false);
        VerifyCollisionReversal(onRightWall: true);

        AssertEqual(
            WallSpacePirateInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all wall Pirate spritemap words remain live cartridge reads");
        for (int index = 0;
             index < WallSpacePirateInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                WallSpacePirateInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads wall Pirate presentation $B2:{address:X4}");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled wall Pirate mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => WallSpacePirateInstructionProgramDefinitions.ReadMechanicsWord(0xecc6),
            "wall Pirate spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => WallSpacePirateInstructionProgramDefinitions.ReadMechanicsWord(0xee40),
            "adjacent wall Pirate callback code is rejected as mechanics");

        _ = ProbeWallSpacePirateInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeWallSpacePirateInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "wall Pirate allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed wall Pirate mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Wall Space Pirate instruction mechanics: 150 compiled words, all eight " +
            "production programs, both attack/jump handoffs, four climb directions, and " +
            "42 live spritemap reads pass with mechanics bytes forbidden.");

        void VerifyAttack(bool movingRight)
        {
            ushort program = movingRight
                ? WallSpacePirateInstructionProgramDefinitions.FireAndJumpRight
                : WallSpacePirateInstructionProgramDefinitions.FireAndJumpLeft;
            ushort expectedCursor = movingRight ? (ushort)0xeda2 : (ushort)0xece2;
            WallSpacePirateFunction expectedFunction = movingRight
                ? WallSpacePirateFunction.WallJumpingRight
                : WallSpacePirateFunction.WallJumpingLeft;
            (RoomEnemySystem enemies, RoomEnemySlot slot,
                WallSpacePirateEnemyState state, SamusState samus) = CreateSystem();
            slot.CurrentInstruction = program;
            slot.InstructionTimer = 1;
            RunFrames(enemies, slot, samus, level, 70);
            AssertEqual(expectedCursor, slot.CurrentInstruction,
                $"wall Pirate {(movingRight ? "right" : "left")} attack sleeps");
            AssertEqual(expectedFunction, state.Function,
                $"wall Pirate {(movingRight ? "right" : "left")} attack jump handoff");
            AssertEqual(1, state.SpawnedLaserCount,
                $"wall Pirate {(movingRight ? "right" : "left")} laser count");
            AssertEqual((ushort)0x0066, enemies.LastSpacePirateSoundEffect!.Value,
                $"wall Pirate {(movingRight ? "right" : "left")} jump sound");
        }

        void VerifyClimb(
            ushort program,
            WallSpacePirateFunction expectedFunction,
            int frames)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot,
                WallSpacePirateEnemyState state, SamusState samus) = CreateSystem();
            ushort initialY = slot.YPosition;
            slot.CurrentInstruction = program;
            slot.InstructionTimer = 1;
            RunFrames(enemies, slot, samus, level, frames);
            AssertEqual(expectedFunction, state.Function,
                $"wall Pirate program $B2:{program:X4} climb function");
            if (frames > 12)
            {
                AssertTrue(slot.YPosition != initialY,
                    $"wall Pirate program $B2:{program:X4} applies climb displacement");
            }
        }

        void VerifyCollisionReversal(bool onRightWall)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot,
                WallSpacePirateEnemyState state, SamusState samus) = CreateSystem();
            slot.CurrentInstruction = onRightWall
                ? WallSpacePirateInstructionProgramDefinitions.MovingDownRightWall
                : WallSpacePirateInstructionProgramDefinitions.MovingDownLeftWall;
            slot.InstructionTimer = 1;
            RunFrames(
                enemies,
                slot,
                samus,
                CreateWallPirateFloorRoom(),
                11);
            AssertEqual(WallSpacePirateClimbDirection.Up, state.ClimbDirection,
                $"wall Pirate {(onRightWall ? "right" : "left")} collision reverses direction");
            AssertEqual(
                onRightWall ? (ushort)0xee02 : (ushort)0xecf8,
                slot.CurrentInstruction,
                $"wall Pirate {(onRightWall ? "right" : "left")} collision installs upward list");
        }

        (RoomEnemySystem Enemies, RoomEnemySlot Slot,
            WallSpacePirateEnemyState State, SamusState Samus) CreateSystem()
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(
                enemies,
                (Func<ushort>)(() => 0));
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.GreyWallSpacePirateDefinition;
            slot.Definition = default(RoomEnemyDefinition) with
            {
                Bank = 0xb2,
                Damage = 20,
            };
            slot.XPosition = 0x0100;
            slot.YPosition = 0x0100;
            slot.XRadius = 8;
            slot.YRadius = 16;
            slot.Parameter2 = 0x0080;
            typeof(RoomEnemySystem).GetMethod("InitializeWallSpacePirate", flags)!
                .Invoke(enemies, [slot]);
            var samus = new SamusState
            {
                XPosition = 0x0080,
                YPosition = 0x0100,
            };
            return (enemies, slot, enemies.WallSpacePirateStates[0]!, samus);
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

    private static RoomLevelData CreateOpenWallPirateRoom()
    {
        const int width = 32;
        const int height = 32;
        int blockCount = width * height;
        return new RoomLevelData(
            width,
            height,
            new ushort[blockCount],
            new byte[blockCount],
            new ushort[blockCount],
            new byte[8]);
    }

    private static RoomLevelData CreateWallPirateFloorRoom()
    {
        const int width = 32;
        const int height = 32;
        var foreground = new ushort[width * height];
        for (int column = 0; column < width; column++)
            foreground[17 * width + column] = 0x8000;
        return new RoomLevelData(
            width,
            height,
            foreground,
            new byte[foreground.Length],
            new ushort[foreground.Length],
            new byte[8]);
    }

    private static int ProbeWallSpacePirateInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += WallSpacePirateInstructionProgramDefinitions.ReadMechanicsWord(
                WallSpacePirateInstructionProgramDefinitions.MovingDownLeftWall);
        }
        return checksum;
    }

    private static ushort ReadWallPirateWord(SuperMetroidAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class WallSpacePirateInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (WallSpacePirateInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled wall Pirate mechanics ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xb20000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < WallSpacePirateInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        WallSpacePirateInstructionProgramDefinitions.PresentationWordAddress(index);
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

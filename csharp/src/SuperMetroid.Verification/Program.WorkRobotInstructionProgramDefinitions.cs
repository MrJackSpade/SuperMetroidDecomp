using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static readonly ushort[] WorkRobotInstructionEntries =
    [
        WorkRobotInstructionProgramDefinitions.NoPowerNeutral,
        WorkRobotInstructionProgramDefinitions.NoPowerLeaningLeft,
        WorkRobotInstructionProgramDefinitions.NoPowerLeaningRight,
        WorkRobotInstructionProgramDefinitions.Initial,
        WorkRobotInstructionProgramDefinitions.FacingLeftWalkingForwards,
        WorkRobotInstructionProgramDefinitions.FacingLeftHitWallMovingForwards,
        WorkRobotInstructionProgramDefinitions.FacingLeftShotSamusAhead,
        WorkRobotInstructionProgramDefinitions.FacingLeftShotSamusBehind,
        WorkRobotInstructionProgramDefinitions.FacingLeftShotLaserDownLeft,
        WorkRobotInstructionProgramDefinitions.FacingLeftShotLaserLeft,
        WorkRobotInstructionProgramDefinitions.FacingLeftShotLaserUpLeft,
        WorkRobotInstructionProgramDefinitions.FacingLeftLaserShotRecoil,
        WorkRobotInstructionProgramDefinitions.ApproachingFallRight,
        WorkRobotInstructionProgramDefinitions.FacingRightWalkingForwards,
        WorkRobotInstructionProgramDefinitions.FacingRightHitWallMovingForwards,
        WorkRobotInstructionProgramDefinitions.FacingRightShotSamusAhead,
        WorkRobotInstructionProgramDefinitions.FacingRightShotSamusBehind,
        WorkRobotInstructionProgramDefinitions.FacingRightShotLaserDownRight,
        WorkRobotInstructionProgramDefinitions.FacingRightShotLaserRight,
        WorkRobotInstructionProgramDefinitions.FacingRightShotLaserUpRight,
        WorkRobotInstructionProgramDefinitions.FacingRightLaserShotRecoil,
        WorkRobotInstructionProgramDefinitions.ApproachingFallLeft,
    ];

    private static void VerifyWorkRobotInstructionProgramDefinitions()
    {
        VerifyWorkRobotInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyWorkRobotInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        int mechanics = 0;
        int presentation = 0;
        for (int address = 0xc6d3; address < 0xcb77; address += 2)
        {
            if (WorkRobotInstructionProgramDefinitions.IsCompiledMechanicsByte(
                    0xa80000 | address))
            {
                mechanics++;
                AssertEqual(ReadWorkRobotInstructionWord(rom, unchecked((ushort)address)),
                    WorkRobotInstructionProgramDefinitions.ReadMechanicsWord(
                        unchecked((ushort)address)),
                    $"Work Robot mechanics word $A8:{address:X4}");
            }
            else
            {
                presentation++;
                AssertThrows<InvalidDataException>(
                    () => WorkRobotInstructionProgramDefinitions.ReadMechanicsWord(
                        unchecked((ushort)address)),
                    $"Work Robot presentation word $A8:{address:X4} is rejected as mechanics");
            }
        }
        AssertEqual(WorkRobotInstructionProgramDefinitions.MechanicsWordCount, mechanics,
            "Work Robot compiled mechanics word count");
        AssertEqual(WorkRobotInstructionProgramDefinitions.PresentationWordCount, presentation,
            "Work Robot live presentation word count");

        var guard = new WorkRobotInstructionReadGuard(rom);
        RoomEnemySystem enemies = CreateWorkRobotInstructionSystem(guard, out RoomEnemySlot robot);
        RoomLevelData level = CreateWorkRobotInstructionRoom();
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessInstructions",
            BindingFlags.Instance | BindingFlags.NonPublic)!;

        foreach (ushort entry in WorkRobotInstructionEntries)
        {
            robot.CurrentInstruction = entry;
            for (int call = 0; call < 400; call++)
            {
                robot.InstructionTimer = 1;
                robot.XPosition = 512;
                robot.YPosition = unchecked((ushort)(320 - robot.YRadius));
                enemies.WorkRobotStates[0]!.LaserCooldown = ushort.MaxValue;
                process.Invoke(
                    enemies,
                    [robot, null, level, (ushort)384, (ushort)192, (ushort)0, (byte)0]);
            }
        }

        AssertEqual(WorkRobotInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all reachable Work Robot spritemap operands remain cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Work Robot production execution avoids compiled mechanics bytes");
        for (int index = 0;
             index < WorkRobotInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = WorkRobotInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Work Robot presentation $A8:{address:X4}");
        }
        AssertThrows<InvalidDataException>(
            () => WorkRobotInstructionProgramDefinitions.ReadMechanicsWord(0xc6d4),
            "odd Work Robot instruction pointer is rejected");
        AssertThrows<InvalidDataException>(
            () => WorkRobotInstructionProgramDefinitions.ReadMechanicsWord(0xcb77),
            "adjacent Work Robot initialization AI is rejected as instruction mechanics");

        _ = ProbeWorkRobotInstructionAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeWorkRobotInstructionAllocation();
        AssertTrue(checksum != 0, "Work Robot instruction allocation probe consumes data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Work Robot instruction mechanics lookups allocate no storage");

        Console.WriteLine(
            "Work Robot instruction mechanics: 367 compiled words, all 22 authored " +
            "entries, 18 callbacks, and 227 live presentation reads pass.");
    }

    private static RoomEnemySystem CreateWorkRobotInstructionSystem(
        ISnesAddressSpace bus,
        out RoomEnemySlot robot)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        typeof(RoomEnemySystem).GetField("_readRandomNumber", flags)!.SetValue(
            enemies,
            (Func<ushort>)(() => 0));

        robot = enemies.Slots[0];
        robot.EnemyDefinitionPointer = RoomEnemySystem.WorkRobotDefinition;
        robot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
        robot.XRadius = 8;
        robot.YRadius = 16;
        var state = new WorkRobotEnemyState(robot) { Powered = true };
        var states = (WorkRobotEnemyState?[])typeof(RoomEnemySystem)
            .GetField("_workRobotStates", flags)!.GetValue(enemies)!;
        states[0] = state;
        return enemies;
    }

    private static RoomLevelData CreateWorkRobotInstructionRoom()
    {
        const int width = 64;
        const int height = 32;
        var words = new ushort[width * height];
        for (int x = 0; x < width; x++)
            words[20 * width + x] = 0x8000;
        return new RoomLevelData(
            width, height, words, new byte[words.Length], new ushort[words.Length], new byte[8]);
    }

    private static int ProbeWorkRobotInstructionAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += WorkRobotInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? WorkRobotInstructionProgramDefinitions.Initial
                    : WorkRobotInstructionProgramDefinitions.FacingRightLaserShotRecoil);
        }
        return checksum;
    }

    private static ushort ReadWorkRobotInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa80000 | address) |
            source.ReadByte(0xa80000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class WorkRobotInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (WorkRobotInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Work Robot mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa80000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < WorkRobotInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = WorkRobotInstructionProgramDefinitions
                        .PresentationWordAddress(index);
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

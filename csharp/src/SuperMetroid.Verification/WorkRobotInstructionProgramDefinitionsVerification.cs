using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using static CompiledEnemyVisualSelectorAssertionsVerification;
using static SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.Core.Rooms.LevelData;

/// <summary>Checks Work Robot's powered and unpowered instruction lists against bank A8.</summary>
internal static class WorkRobotInstructionProgramDefinitionsVerification
{
    /// <summary>The 22 authored robot entries spanning idle, walk, wall, shot, laser, recoil, and fall behavior.</summary>
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

    /// <summary>Loads the retail cartridge as the oracle for Work Robot control and visual selectors.</summary>
    internal static void VerifyWorkRobotInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyWorkRobotInstructionProgramDefinitions), () => VerifyWorkRobotInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Compares compiled words, executes all 22 entries, and checks sprite selectors without runtime reads.</summary>
    internal static void VerifyWorkRobotInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        int mechanics = 0;
        int presentation = 0;
        for (int address = 0xc6d3; address < 0xcb77; address += 2)
        {
            if (WorkRobotInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(
                    0xa80000 | address))
            {
                mechanics++;
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa80000 | (unchecked((ushort)address))),
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
        AssertEqual(WorkRobotInstructionProgramDefinitionsTooling.MechanicsWordCount, mechanics,
            "Work Robot compiled mechanics word count");
        AssertEqual(WorkRobotInstructionProgramDefinitionsTooling.PresentationWordCount, presentation,
            "Work Robot live presentation word count");

        var guard = NewWorkRobotInstructionReadGuard(rom);
        RoomEnemySystem enemies = CreateWorkRobotInstructionSystem(guard, out RoomEnemySlot robot);
        RoomLevelData level = CreateWorkRobotInstructionRoom();

        foreach (ushort entry in WorkRobotInstructionEntries)
        {
            robot.CurrentInstruction = entry;
            for (int call = 0; call < 400; call++)
            {
                robot.InstructionTimer = NativeCountdown.Start(1);
                robot.Position = robot.Position.WithWhole(new(512, unchecked((ushort)(320 - robot.Radius.Y))));
                (enemies.WorkRobotStates[0] ?? throw new InvalidOperationException("Verification requires enemies.WorkRobotStates[0].")).LaserCooldown = NativeCountdown.Start(ushort.MaxValue);
                enemies.Instructions.ProcessInstructions(robot, null, level, new Position16<RoomPixels>((ushort)384, (ushort)192), (ushort)0);
            }
        }

        AssertEqual(0, guard.ObservedWords.Count,
            "Work Robot execution uses compiled spritemap selectors");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Work Robot production execution avoids compiled mechanics bytes");
        for (int index = 0;
             index < WorkRobotInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort address = WorkRobotInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            AssertCompiledEnemyVisualSelector(rom, EnemyDefinitionId.Robot,
                0xa8, address, $"Work Robot $A8:{address:X4}");
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
            "entries, 18 callbacks, and 227 compiled visual selectors pass.");
    }

    /// <summary>Installs a powered bank A8 robot with deterministic random input for list execution.</summary>
    internal static RoomEnemySystem CreateWorkRobotInstructionSystem(
        ISnesAddressSpace bus,
        out RoomEnemySlot robot)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(bus);
        enemies.VerificationServices().ReadRandom = (Func<ushort>)(() => 0);

        robot = enemies.SlotTable.Slots[0];
        robot.EnemyDefinitionPointer = EnemyDefinitionId.Robot;
        robot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
        robot.Radius = new(8, 16);
        var state = new WorkRobotEnemyState(robot) { Powered = true };
        var states = (WorkRobotEnemyState?[])enemies.WorkRobotBehavior._workRobotStates;
        states[0] = state;
        return enemies;
    }

    /// <summary>Builds a wide room with a solid floor for Work Robot movement and fall callbacks.</summary>
    private static RoomLevelData CreateWorkRobotInstructionRoom()
    {
        const int width = 64;
        const int height = 32;
        var words = new ushort[width * height];
        for (int x = 0; x < width; x++)
            words[20 * width + x] = 0x8000;
        return new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(words, new byte[words.Length], new ushort[words.Length]), new byte[8]);
    }

    /// <summary>Alternates initial and recoil words repeatedly to expose compiled lookup allocations.</summary>
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

    /// <summary>Rejects compiled mechanics reads and optionally rejects visual selector reads.</summary>
    internal static CartridgeReadGuard NewWorkRobotInstructionReadGuard(ISnesAddressSpace source, bool forbidPresentation = false)
    {
        var guard = new CartridgeReadGuard(source).Forbid(WorkRobotInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
            address => $"Production read compiled Work Robot mechanics byte ${address:X6}.");
        return forbidPresentation
            ? guard.ForbidWords(0xa80000, WorkRobotInstructionProgramDefinitionsTooling.PresentationWordCount, WorkRobotInstructionProgramDefinitionsTooling.PresentationWordAddress,
                address => $"Installed Work Robot read presentation byte ${address:X6}.")
            : guard.ObserveWords(0xa80000, WorkRobotInstructionProgramDefinitionsTooling.PresentationWordCount, WorkRobotInstructionProgramDefinitionsTooling.PresentationWordAddress);
    }

    /// <summary>Runs Work Robot instruction checks from the verification command dispatcher.</summary>
    [VerificationCommand("--work-robot-instruction-program-definitions")]
    internal static int WorkRobotInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyWorkRobotInstructionProgramDefinitions), () => VerifyWorkRobotInstructionProgramDefinitions());
        return 0;
    }
}

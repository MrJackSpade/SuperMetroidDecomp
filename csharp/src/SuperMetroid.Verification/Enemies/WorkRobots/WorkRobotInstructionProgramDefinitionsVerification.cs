using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.Core.Rooms.LevelData;
namespace SuperMetroid.Verification.Enemies.WorkRobots;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Work Robot's powered and unpowered instruction lists against bank A8.</summary>
internal static class WorkRobotInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge for the Work Robot animation checks.</summary>
    internal static void VerifyWorkRobotInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyWorkRobotInstructionProgramDefinitions), () => VerifyWorkRobotInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>
    /// Every program runs for 400 forced updates on a floored room with the laser held back, exercising each walk, wall,
    /// ledge, recoil and footstep cue; the program oracle matches bank $A8.
    /// </summary>
    internal static void VerifyWorkRobotInstructionProgramDefinitions(SuperMetroidAddressSpace rom)
    {
        RoomEnemySystem enemies = CreateWorkRobotInstructionSystem(out RoomEnemySlot robot);
        RoomLevelData level = CreateWorkRobotInstructionRoom();
        WorkRobotEnemyState state = enemies.WorkRobotStates[0] ?? throw new InvalidOperationException("Verification requires enemies.WorkRobotStates[0].");

        foreach (WorkRobotProgram program in Enum.GetValues<WorkRobotProgram>())
        {
            state.Program = AnimationProgramCursor<WorkRobotProgram>.Start(program);
            for (int call = 0; call < 400; call++)
            {
                robot.InstructionTimer = NativeCountdown.Start(1);
                robot.Position = robot.Position.WithWhole(new(512, unchecked((ushort)(320 - robot.Radius.Y))));
                state.LaserCooldown = NativeCountdown.Start(ushort.MaxValue);
                EnemyInstructionInterpreter.ProcessInstructions(robot, null, level, new Position16<RoomPixels>(384, 192), 0);
            }
        }

        Suite(nameof(WorkRobotAnimationsVerification.VerifyWorkRobotAnimations), () => WorkRobotAnimationsVerification.VerifyWorkRobotAnimations(rom));
        Console.WriteLine("Work Robot: all 24 typed programs run their walk, wall, ledge, recoil and footstep cues.");
    }

    /// <summary>Installs a powered bank A8 robot with deterministic random input for list execution.</summary>
    internal static RoomEnemySystem CreateWorkRobotInstructionSystem(
        out RoomEnemySlot robot)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().ReadRandom = () => 0;

        robot = enemies.SlotTable.Slots[0];
        robot.Bind(EnemyDefinitionId.Robot, robot.Definition);
        robot.Bind(robot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = 0xa8 });
        robot.Radius = new(8, 16);
        var state = new WorkRobotEnemyState() { Powered = true, Program = AnimationProgramCursor<WorkRobotProgram>.Start(WorkRobotProgram.Initial) };
        robot.SetSpeciesState(state);
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

    /// <summary>Runs Work Robot instruction checks from the verification command dispatcher.</summary>
    [VerificationCommand("--work-robot-instruction-program-definitions")]
    internal static int WorkRobotInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyWorkRobotInstructionProgramDefinitions), () => VerifyWorkRobotInstructionProgramDefinitions());
        return 0;
    }
}

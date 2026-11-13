using SuperMetroid.Core.Animation;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.WorkRobots;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

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
    internal static void VerifyWorkRobotInstructionProgramDefinitions(CartridgeImportAddressSpace rom)
    {
        RoomEnemySystem enemies = CreateWorkRobotInstructionSystem(out RoomEnemySlot robot);
        RoomLevelData level = CreateWorkRobotInstructionRoom();
        WorkRobotEnemyState state = enemies.WorkRobotStates[0] ?? throw new InvalidOperationException("Verification requires enemies.WorkRobotStates[0].");

        foreach (WorkRobotSegment program in Enum.GetValues<WorkRobotSegment>())
        {
            state.Playback = new(WorkRobotAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
            for (int call = 0; call < 400; call++)
            {
                robot.AnimationClock.StepOnNextUpdate();
                robot.Position = robot.Position.WithWhole(new(512, unchecked((ushort)(320 - robot.Radius.Y))));
                state.LaserCooldown = WrappingCountdown.Start(ushort.MaxValue);
                EnemyAnimationPlayer.AdvanceAnimation(robot, null, level, new Position16<RoomPixels>(384, 192), 0);
            }
        }

        Suite(nameof(WorkRobotAnimationsVerification.VerifyWorkRobotAnimations), () => WorkRobotAnimationsVerification.VerifyWorkRobotAnimations(rom));
        Console.WriteLine("Work Robot: all 24 typed programs run their walk, wall, ledge, recoil and footstep cues.");
    }

    /// <summary>Installs a powered bank A8 robot with deterministic random input for list execution.</summary>
    internal static RoomEnemySystem CreateWorkRobotInstructionSystem(
        out RoomEnemySlot robot)
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().ReadRandom = () => 0;

        robot = enemies.SlotTable.Slots[0];
        robot.Bind(EnemyDefinitionId.Robot, robot.Definition);
        robot.Bind(robot.DefinitionId, default(RoomEnemyDefinition));
        robot.Radius = new(8, 16);
        var state = new WorkRobotEnemyState() { Powered = true, Playback = new(WorkRobotAnimations.StartingWith(WorkRobotSegment.Initial), AnimationPlace.StartOfAnimation) };
        robot.SetSpeciesState(state);
        return enemies;
    }

    /// <summary>Builds a wide room with a solid floor for Work Robot movement and fall callbacks.</summary>
    private static RoomLevelData CreateWorkRobotInstructionRoom() =>
        VerificationHelpersVerification.CreateSolidRowsRoom(new(64, 32), firstSolidRow: 20, solidRowCount: 1);

    /// <summary>Runs Work Robot instruction checks from the verification command dispatcher.</summary>
    [VerificationCommand("--work-robot-instruction-program-definitions")]
    internal static int WorkRobotInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyWorkRobotInstructionProgramDefinitions), () => VerifyWorkRobotInstructionProgramDefinitions());
        return 0;
    }
}

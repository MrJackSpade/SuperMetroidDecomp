using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.WorkRobots;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>An airborne Work Robot's typed falling speed keeps the cartridge's gravity carry and wraparound.</summary>
internal static class WorkRobotFallVerification
{
    /// <summary>
    /// Runs the production gravity AI for one airborne update in open air. Gravity adds half a pixel to the falling
    /// speed: one and a half pixels becomes two, its fraction carrying into the whole word, and minus half a pixel
    /// becomes zero, the carry wrapping the whole word from 65535. The slot variables hold the same words.
    /// </summary>
    internal static void VerifyWorkRobotFall()
    {
        RoomLevelData air = VerificationHelpersVerification.CreateEmptyRoom(new(8, 8));
        var frame = new ClockedEnemyAiFrame(
            RoomEnemyFrameFixtures.Standalone(air, 0) with { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false },
            new FrameCounters(SuperMetroid.Core.Compatibility.ShortFrameCount.Zero));
        foreach ((short whole, ushort fraction, ushort expectedWhole, ushort expectedFraction) in new VerifyWorkRobotFallCase[]
                 {
                     new VerifyWorkRobotFallCase(1, 32768, 2, 0),
                     new VerifyWorkRobotFallCase(-1, 32768, 0, 0),
                 })
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            // The powered robot, whose main AI is its fall, is set up in the air as its room's population sets it up (#2554).
            RoomEnemyPlace place = enemies.LoadPopulation(RoomEnemyFrameFixtures.StandaloneSamus(), air,
                PopulationPlacementAccess.Entry(EnemyDefinitionId.WorkRobot,
                    SuperMetroid.AssetExtraction.Enemies.EnemySpawnSettingsCodec.Decode(EnemyDefinitionId.WorkRobot,
                        SuperMetroid.AssetExtraction.Enemies.EnemySpawnSettingsCodec.Blank(EnemyDefinitionId.WorkRobot)),
                    new(64, 32)))[0];
            place.Radius = new(8, 8);
            WorkRobotEnemyState state = place.RequireSpeciesState<WorkRobotEnemyState>();
            state.FallVelocity = SignedSixteenSixteen.FromParts(whole, fraction);

            place.RunMainAi(frame);

            string context = $"Work Robot falling at {whole} and {fraction}/65536 pixels";
            AssertEqual(expectedWhole, state.FallVelocity.UnsignedWholePixels, $"{context}: whole speed word");
            AssertEqual(expectedFraction, state.FallVelocity.Fraction, $"{context}: speed fraction word");
            AssertEqual(SignedSixteenSixteen.FromParts(unchecked((short)expectedWhole), expectedFraction), state.FallVelocity, context);
        }
        Console.WriteLine("  Work Robot fall: gravity carries the fraction into the whole word and wraps it.");
    }

    /// <summary>Runs the Work Robot fall check.</summary>
    [VerificationCommand("--work-robot-fall")]
    internal static int WorkRobotFallCommand()
    {
        Suite(nameof(VerifyWorkRobotFall), () => VerifyWorkRobotFall());
        return 0;
    }
}

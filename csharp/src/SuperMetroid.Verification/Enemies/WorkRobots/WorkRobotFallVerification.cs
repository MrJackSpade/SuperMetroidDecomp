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
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
            place.Bind(EnemyDefinitionId.UnpoweredWorkRobot, place.KeptDefinition);
            place.Radius = new(8, 8);
            place.Position = FixedPosition<RoomPixels>.FromWhole(new(64, 32));
            var state = new WorkRobotEnemyState() { FallVelocity = SignedSixteenSixteen.FromParts(whole, fraction) };
            place.SetSpeciesState(state);

            enemies.WorkRobotBehavior.RunWorkRobotMainAi(place, place.RequireSpeciesState<WorkRobotEnemyState>(), frame);

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

using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.Core.Rooms.LevelData;
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
        var air = new RoomLevelData(new Extent16<RoomBlocks>(8, 8), new RoomLevelPlanes(new ushort[64], new byte[64], new ushort[64]), new byte[8]);
        var frame = new ClockedEnemyAiFrame(
            new RoomEnemyFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, Level = air },
            new FrameCounters(0));
        foreach ((short whole, ushort fraction, ushort expectedWhole, ushort expectedFraction) in new VerifyWorkRobotFallCase[]
                 {
                     new VerifyWorkRobotFallCase(1, 32768, 2, 0),
                     new VerifyWorkRobotFallCase(-1, 32768, 0, 0),
                 })
        {
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(EnemyDefinitionId.RobotNoPower, slot.Definition);
            slot.Radius = new(8, 8);
            slot.Position = FixedPosition<RoomPixels>.FromWhole(new(64, 32));
            var state = new WorkRobotEnemyState() { FallVelocity = SnesSignedSixteenSixteen.FromParts(whole, fraction) };
            slot.SetSpeciesState(state);

            enemies.WorkRobotBehavior.RunWorkRobotMainAi(slot, frame);

            string context = $"Work Robot falling at {whole} and {fraction}/65536 pixels";
            AssertEqual(expectedWhole, state.FallVelocity.WholeWord, $"{context}: whole speed word");
            AssertEqual(expectedFraction, state.FallVelocity.Fraction, $"{context}: speed fraction word");
            AssertEqual(SnesSignedSixteenSixteen.FromParts(unchecked((short)expectedWhole), expectedFraction), state.FallVelocity, context);
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

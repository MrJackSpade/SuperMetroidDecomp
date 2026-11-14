using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the falling Mother Brain tube's trajectory, hiding, head follow, and landing effects.</summary>
internal static class MotherBrainTubeDescentVerification
{
    /// <summary>Compares fractional acceleration and head position every frame through tube deletion and room rebuild.</summary>
    internal static int VerifyMotherBrainTubeDescent()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        var state = new MotherBrainEnemyState(enemies.SlotTable.Slots[0]) { Head = enemies.SlotTable.Slots[1] };
        enemies.MotherBrainEncounter.MotherBrain = state;
        var tube = enemies.SlotTable.Slots[2];
        tube.SetSpawnParameter1(EnemyDefinitionId.MotherBrainTubes, 8);
        tube.SetSpawnParameter2(EnemyDefinitionId.MotherBrainTubes, 0);
        tube.Position = tube.Position.WithWhole(new(128, 128));
        state.Head.Position = state.Head.Position.WithWholeY(72);
        tube.Bind(EnemyDefinitionId.MotherBrainTubes, tube.Definition);
        MotherBrainFakeDeath.InitializeMotherBrainFallingTube(tube);
        Action<RoomEnemySlot> step = enemies.MotherBrainBehavior.FakeDeath.RunMotherBrainFallingTubeMain;
        uint expectedPosition = SixteenSixteenWords.FromWhole(tube.Position.Y.Whole);
        int speed = 0, movingFramesBeforeHide = 0, hiddenFramesBeforeLanding = 0;
        ushort previousHeadY = state.Head.Position.Y.Whole;
        int frame;
        for (frame = 0; frame < 200; frame++)
        {
            speed += 6;
            expectedPosition += (uint)SixteenSixteen.FromEightEight(speed);
            ushort expectedY = (ushort)SixteenSixteenWords.WholeOf(expectedPosition);
            ushort expectedHead = (ushort)Math.Min(expectedY - 56, 196);
            step(tube);
            AssertEqual(expectedY, tube.Position.Y.Whole, $"Tube native acceleration at frame {frame}");
            AssertEqual((ushort)expectedPosition, tube.Position.Y.Fraction, "Tube retains fractional motion");
            AssertEqual(expectedHead, state.Head.Position.Y.Whole, $"Brain follows tube every frame ({frame})");
            AssertEqual(expectedY >= 244, tube.Presence.HasAny(EnemyPresence.Hidden),
                "Tube hiding threshold does not gate head descent");
            if (expectedY < 244 && state.Head.Position.Y.Whole > previousHeadY) movingFramesBeforeHide++;
            if (expectedY >= 244 && expectedHead < 196) hiddenFramesBeforeLanding++;
            previousHeadY = state.Head.Position.Y.Whole;
            if (expectedHead == 196) break;
            AssertTrue(!tube.Presence.HasAny(EnemyPresence.Deleted), "Tube remains alive before landing");
        }
        AssertTrue(frame < 200 && movingFramesBeforeHide > 20 && hiddenFramesBeforeLanding > 0,
            "Trajectory includes continuous descent both before hiding and before landing");
        AssertTrue(tube.Presence.HasAny(EnemyPresence.Deleted), "Landing deletes the main tube");
        AssertEqual(MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows1And2, state.Phase, "Landing starts room rebuild");
        AssertEqual(0x20, enemies.Earthquake.RemainingFrames, "Landing retains native earthquake duration");
        Console.WriteLine($"Mother Brain tube descent: {frame + 1} exact trajectory frames, hide threshold and landing passed.");
        return 0;
    }

    /// <summary>Runs the focused Mother Brain tube descent verification command.</summary>
    [VerificationCommand("--mother-brain-tube-descent")]
    internal static int MotherBrainTubeDescentCommand()
    {
        return VerifyMotherBrainTubeDescent();
    }
}

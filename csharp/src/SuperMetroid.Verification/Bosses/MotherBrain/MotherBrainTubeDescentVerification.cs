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
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        enemies.Places.InUpdateOrder[1].Bind(EnemyDefinitionId.MotherBrainHead, enemies.Places.InUpdateOrder[1].KeptDefinition);
        var state = new MotherBrainEnemyState(enemies.Places.InUpdateOrder[0].BoundFor(EnemyDefinitionId.MotherBrainBody), enemies.Places.InUpdateOrder[1], new MotherBrainWalkCounter(), new MotherBrainBurstCycle(), new MotherBrainBrainAnimation(), RoomWarningFixtures.Zebes());
        enemies.MotherBrainEncounter.BeginAsSetUp(state, state.Head);
        var tube = enemies.Places.InUpdateOrder[2];
        // The main tube is placed with its piece's spawn settings, as its population entry places it.
        tube.BindPlaced(EnemyDefinitionId.MotherBrainTubes, new MotherBrainTubeSpawnSettings(MotherBrainTubePiece.Main));
        tube.Position = tube.Position.WithWhole(new(128, 128));
        state.Head.Position = state.Head.Position.WithWholeY(72);
        // The tube holds the encounter that spawned it, as its spawn gives it (#2218).
        tube.SetSpeciesState(state);
        tube.SetSpeciesState(MotherBrainFakeDeath.InitializeMotherBrainFallingTube(tube.HeldOccupant().SpawnSettings<MotherBrainTubeSpawnSettings>(), tube));
        // The descent is traced from the update the main tube starts to fall: its wait runs out on its first update.
        tube.RequireSpeciesState<MotherBrainFallingTubeState>().FallDelay.LastFor(1);
        Action<RoomEnemyPlace> step = place => enemies.MotherBrainBehavior.FakeDeath.RunMotherBrainFallingTubeMain(place, place.RequireSpeciesState<MotherBrainEnemyState>(), place.RequireSpeciesState<MotherBrainFallingTubeState>());
        uint expectedPosition = SixteenSixteenWords.FromWhole(tube.Position.Y.Whole);
        int speed = 0, movingFramesBeforeHide = 0, hiddenFramesBeforeLanding = 0;
        ushort previousHeadY = state.Head.Position.Y.Whole;
        int frame;
        for (frame = 0; frame < 200; frame++)
        {
            speed += 6;
            expectedPosition += (uint)EightEightOracle.ToSixteenSixteen(speed);
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
        AssertEqual(MotherBrainAscentPhase.DrawMural, state.AscentPhase, "Landing starts room rebuild");
        AssertEqual(0, state.NextMuralRow?.Place, "the rebuild starts at the mural's top row");
        AssertEqual(0x20, enemies.Earthquake.RemainingFrames, "Landing retains native earthquake duration");

        // #2211: the rebuild draws the mural two rows an update, top to bottom from block row two of column two, then
        // hands off to the phase-two graphics setup.
        enemies.PassEvents.Begin();
        for (int update = 0; update < 6; update++)
        {
            AssertEqual(MotherBrainAscentPhase.DrawMural, state.AscentPhase, $"the mural is still drawing at update {update}");
            enemies.MotherBrainBehavior.FakeDeath.DrawNextMuralRows(state);
        }
<<<<<<< HEAD
        AssertEqual(string.Join(",", Enumerable.Range(0, 12)),
            string.Join(",", enemies.LastEnemyFrame.All<MotherBrainRoomChangeRequest>().Select(request =>
                request is MotherBrainMuralRowReveal reveal ? reveal.Row.Place.ToString(System.Globalization.CultureInfo.InvariantCulture) : request.ToString())),
||||||| 418ca79935
        AssertTrue(enemies.LastEnemyFrame.All<MotherBrainRoomChangeRequest>().SequenceEqual(Enumerable.Range(0, 12).Select(row =>
                new MotherBrainMuralRowReveal(global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Bosses.MotherBrain.MotherBrainMuralRow>([typeof(int)], new object?[] { row })))),
=======
        AssertTrue(enemies.LastEnemyFrame.All<MotherBrainRoomChangeRequest>().SequenceEqual(Enumerable.Range(0, 12).Select(row =>
                new MotherBrainMuralRowReveal(new global::SuperMetroid.Core.Bosses.MotherBrain.MotherBrainMuralRow(row)))),
>>>>>>> origin/main
            "the mural's twelve rows are revealed in order");
        AssertTrue(enemies.LastEnemyFrame.All<MotherBrainRoomChangeRequest>().Select(request => request.Block).SequenceEqual(Enumerable.Range(0, 12).Select(row =>
                new Position16<RoomBlocks>(2, 2 + row))),
            "each of the mural's rows is revealed a block below the last, from block row two of column two");
        AssertEqual(MotherBrainAscentPhase.SetupPhase2Graphics, state.AscentPhase, "the drawn mural hands off to the phase-two graphics");
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

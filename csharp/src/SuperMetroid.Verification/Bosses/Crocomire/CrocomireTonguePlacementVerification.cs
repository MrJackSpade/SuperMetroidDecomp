using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Bosses.Crocomire;

/// <summary>
/// Checks that Crocomire's tongue is drawn against the body picture he shows on an update in which he steps (#2031): his
/// update places the tongue before his animation's step moves him, so the tongue's picture follows the step while the
/// tongue itself, which Samus touches, stays where the update placed it.
/// </summary>
internal static class CrocomireTonguePlacementVerification
{
    /// <summary>
    /// Walks Crocomire forward update by update, placing his tongue as his fight update does and then running his
    /// animation: every update the tongue lies where it was placed against his body before the step, and its picture lies
    /// at its offset from where the step leaves his body, where his body's picture is placed.
    /// </summary>
    internal static void VerifyCrocomireTongueDrawnAgainstSteppedBody()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(),
            definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        RoomEnemyPlace body = enemies.Places.InUpdateOrder[0];
        body.Bind(EnemyDefinitionId.Crocomire, RoomEnemyDefinitionLookup.Get(EnemyDefinitionId.Crocomire));
        body.Position = body.Position.WithWhole(new(576, 144));
        body.Radius = new(body.Definition.Radius.X, body.Definition.Radius.Y);
        var state = new CrocomireEnemyState(body)
        {
            Phase = CrocomirePhase.SteppingForward,
            BodyPlayback = new AnimationPlayback<CrocomireBodySegment>(
                CrocomireBodyAnimations.StartingWith(CrocomireBodyAnimation.StepForward), AnimationPlace.StartOfAnimation),
        };
        body.SetSpeciesState(state);
        enemies.VerificationServices().ReadRandom = () => ushort.MaxValue;
        enemies.VerificationServices().NextRandom = () => ushort.MaxValue;
        RoomEnemyPlace tongue = enemies.Places.InUpdateOrder[1];
        tongue.Bind(EnemyDefinitionId.CrocomireTongue, RoomEnemyDefinitionLookup.Get(EnemyDefinitionId.CrocomireTongue));
        tongue.SetSpeciesState(new CrocomireTongueState(state) { BodyXOffset = CrocomireFightDefinitions.TongueBodyXOffset });
        state.JoinTongue(tongue);
        body.AnimationClock.StepOnNextUpdate();

        var tongueOffset = new Offset16<RoomPixels>(CrocomireFightDefinitions.TongueBodyXOffset, 0);
        int steps = 0;
        for (int update = 0; update < 120; update++)
        {
            CrocomireBodyLayer.PlaceCrocomireTongue(state);
            Position16<RoomPixels> placedAgainst = body.Position.Whole;
            EnemyAnimationPlayer.AdvanceAnimation(body, samus: RoomEnemyFrameFixtures.StandaloneSamus(),
                level: RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), Position16<RoomPixels>.Zero, controllerInput: 0);
            if (body.Position.Whole != placedAgainst)
                steps++;
            AssertEqual(placedAgainst + tongueOffset, tongue.Position.Whole,
                $"update {update}: the tongue lies where his update placed it, before his step");
            AssertEqual(body.Position.Whole + tongueOffset, tongue.Position.Whole + tongue.DrawOffset,
                $"update {update}: the tongue's picture lies against where his body's picture is placed");
        }
        AssertTrue(steps > 0, "Crocomire steps during the walk");
        Console.WriteLine($"Crocomire tongue placement: across {steps} steps the tongue's picture stays against his body's while the tongue keeps its place.");
    }
}

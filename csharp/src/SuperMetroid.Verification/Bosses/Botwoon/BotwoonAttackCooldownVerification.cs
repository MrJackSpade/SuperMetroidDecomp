using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
namespace SuperMetroid.Verification.Bosses.Botwoon;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>
/// Botwoon's spit cooldown is a countdown that expires by running below zero: the expiring step pins
/// it back to zero and returns the head to following its movement, and the hidden-spit state waits while it is nonzero.
/// </summary>
internal static class BotwoonAttackCooldownVerification
{
    /// <summary>
    /// Drives the production main AI (<c>RunBotwoonMainAi</c>) on a constructed head with its thirteen real body
    /// segments. In the moving frame the cooldown at one steps to zero and keeps cooling down, then at zero runs to
    /// the negative branch, is pinned to zero and hands the head back to movement. In the hidden-spit state the gate
    /// keeps running the cooldown while the timer is nonzero and leaves the state once it reads zero.
    /// </summary>
    internal static void VerifyBotwoonAttackCooldown()
    {
        // Moving frame: the only path on which the cooldown starts at zero and runs below it.
        (RoomEnemySystem moving, RoomEnemySlot movingHead, BotwoonEnemyState movingState) = NewBotwoon();
        movingState.Phase = BotwoonPhase.ChooseNextAction;
        movingState.Movement = BotwoonMovement.MoveTowardHole;
        movingState.HeadAction = BotwoonHeadAction.AttackCooldown;
        movingState.AttackTimer = 1;

        moving.BotwoonBehavior.RunBotwoonMainAi(movingHead, Frame());
        AssertEqual(0, movingState.AttackTimer, "a cooldown at one steps to zero");
        AssertEqual(BotwoonHeadAction.AttackCooldown, movingState.HeadAction, "reaching zero does not end the cooldown");

        moving.BotwoonBehavior.RunBotwoonMainAi(movingHead, Frame());
        AssertEqual(0, movingState.AttackTimer, "the cooldown that runs below zero is pinned back to zero");
        AssertEqual(BotwoonHeadAction.AnimateFromMovement, movingState.HeadAction, "the expired cooldown returns the head to following its movement");

        // Hidden spit: the state's gate runs the cooldown while it is nonzero and leaves once it reads zero.
        (RoomEnemySystem hidden, RoomEnemySlot hiddenHead, BotwoonEnemyState hiddenState) = NewBotwoon();
        hiddenState.Phase = BotwoonPhase.SpitWhileHidden;
        hiddenState.HeadAction = BotwoonHeadAction.AttackCooldown;
        hiddenState.AttackTimer = 1;

        hidden.BotwoonBehavior.RunBotwoonMainAi(hiddenHead, Frame());
        AssertEqual(0, hiddenState.AttackTimer, "the hidden-spit gate runs a nonzero cooldown");
        AssertEqual(BotwoonPhase.SpitWhileHidden, hiddenState.Phase, "a running cooldown keeps Botwoon spitting from the hole");

        hidden.BotwoonBehavior.RunBotwoonMainAi(hiddenHead, Frame());
        AssertEqual(0, hiddenState.AttackTimer, "the gate does not decrement a zero cooldown");
        AssertEqual(BotwoonPhase.FollowAuthoredPath, hiddenState.Phase, "a zero cooldown ends the hidden spit");
        AssertEqual(BotwoonHeadAction.AnimateFromMovement, hiddenState.HeadAction, "leaving the hidden spit returns the head to movement");

        Console.WriteLine("  Botwoon attack cooldown: steps to zero, runs below it and pins to zero, and gates the hidden spit on zero.");

        static ClockedEnemyAiFrame Frame() => new(
            RoomEnemyFrameFixtures.Standalone(RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), 0) with { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, Samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) },
            new FrameCounters(0));
    }

    /// <summary>
    /// A head in slot zero bound as Botwoon, away from every hole, with its thirteen body segments spawned by the
    /// production spawner and its health palette complete so the update reads no artwork.
    /// </summary>
    private static NewBotwoonHeadResult NewBotwoon()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().NextRandom = () => 0;
        RoomEnemySlot head = enemies.SlotTable.Slots[0];
        head.Bind(EnemyDefinitionId.Botwoon, head.Definition);
        head.Position = head.Position.WithWhole(new(1000, 1000));
        var state = new BotwoonEnemyState()
        {
            HealthBandsPassed = BotwoonHealthBandDefinitions.BandCount,
            SegmentSpacingEntries = 3,
            Speed = 0,
        };
        enemies.BotwoonBehavior.AttachBotwoon(head, state);
        for (int segmentIndex = state.BodySegments.Length - 1; segmentIndex >= 0; segmentIndex--)
            enemies.BotwoonBehavior.SpawnBotwoonBodySegment(head, state, segmentIndex);
        return new NewBotwoonHeadResult(enemies, head, state);
    }

    /// <summary>Runs the Botwoon attack-cooldown check.</summary>
    [VerificationCommand("--botwoon-attack-cooldown")]
    internal static int BotwoonAttackCooldownCommand()
    {
        Suite(nameof(VerifyBotwoonAttackCooldown), () => VerifyBotwoonAttackCooldown());
        return 0;
    }
}

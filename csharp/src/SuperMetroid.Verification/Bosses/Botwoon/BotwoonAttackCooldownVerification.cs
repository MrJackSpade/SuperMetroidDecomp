using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
namespace SuperMetroid.Verification.Bosses.Botwoon;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>
/// #2267: Botwoon's spit cooldown is a boss countdown that each volley starts with its own duration. The game held one
/// signed timer for it, loaded with 48 as he started spitting from a hole and with zero as he started a walk: in a hole
/// it counted down from the volley and he left on the update it read zero, the 49th after the volley; outside one it ran
/// below zero on the update after the volley and was pinned back to zero. These checks hold those updates.
/// </summary>
internal static class BotwoonAttackCooldownVerification
{
    /// <summary>
    /// Drives the production main AI (<c>RunBotwoonMainAi</c>) on a constructed head with its thirteen real body
    /// segments. Outside a hole the immediate volley's cooldown returns the head to following his movement on the update
    /// after the volley. From a hole the aimed volley's cooldown returns it on the 48th update after the volley and he
    /// leaves the hole on the 49th, as the game's count of 48 read zero there.
    /// </summary>
    internal static void VerifyBotwoonAttackCooldown()
    {
        (RoomEnemySystem moving, RoomEnemyPlace movingHead, BotwoonEnemyState movingState) = NewBotwoon();
        movingState.Phase = BotwoonPhase.ChooseNextAction;
        movingState.Movement = BotwoonMovement.MoveTowardHole;
        movingState.HeadAction = BotwoonHeadAction.SpawnImmediateVolley;

        moving.BotwoonBehavior.RunBotwoonMainAi(movingHead.HeldOccupant(), movingHead, Frame());
        AssertEqual(BotwoonHeadAction.AttackCooldown, movingState.HeadAction, "the immediate volley starts the cooldown");
        moving.BotwoonBehavior.RunBotwoonMainAi(movingHead.HeldOccupant(), movingHead, Frame());
        AssertEqual(BotwoonHeadAction.AnimateFromMovement, movingState.HeadAction,
            "the immediate volley's cooldown returns the head to following its movement on the next update");

        (RoomEnemySystem hidden, RoomEnemyPlace hiddenHead, BotwoonEnemyState hiddenState) = NewBotwoon();
        hiddenState.Phase = BotwoonPhase.SpitWhileHidden;
        hiddenState.HeadAction = BotwoonHeadAction.SpitAimedVolley;

        hidden.BotwoonBehavior.RunBotwoonMainAi(hiddenHead.HeldOccupant(), hiddenHead, Frame());
        AssertEqual(BotwoonHeadAction.AttackCooldown, hiddenState.HeadAction, "the aimed volley starts the cooldown");
        for (int update = 1; update <= 47; update++)
        {
            hidden.BotwoonBehavior.RunBotwoonMainAi(hiddenHead.HeldOccupant(), hiddenHead, Frame());
            AssertEqual(BotwoonHeadAction.AttackCooldown, hiddenState.HeadAction, $"the head still cools down {update} updates after the volley");
            AssertEqual(BotwoonPhase.SpitWhileHidden, hiddenState.Phase, $"Botwoon still spits from the hole {update} updates after the volley");
        }
        hidden.BotwoonBehavior.RunBotwoonMainAi(hiddenHead.HeldOccupant(), hiddenHead, Frame());
        AssertEqual(BotwoonHeadAction.AnimateFromMovement, hiddenState.HeadAction, "the cooldown runs out on the 48th update after the volley");
        AssertEqual(BotwoonPhase.SpitWhileHidden, hiddenState.Phase, "Botwoon stays in the hole on the update the cooldown runs out");
        hidden.BotwoonBehavior.RunBotwoonMainAi(hiddenHead.HeldOccupant(), hiddenHead, Frame());
        AssertEqual(BotwoonPhase.FollowAuthoredPath, hiddenState.Phase, "Botwoon leaves the hole on the 49th update after the volley");
        AssertEqual(BotwoonHeadAction.AnimateFromMovement, hiddenState.HeadAction, "leaving the hidden spit returns the head to movement");

        Console.WriteLine("  Botwoon attack cooldown: the immediate volley cools down one update, the aimed volley 48, and he leaves the hole on the 49th.");
    }

    /// <summary>
    /// #2538: Botwoon waits in his hole <see cref="BotwoonFightDefinitions.OpeningDelay"/>, 256 updates, as the game
    /// counted 256 down to zero, and chooses his first action on the update after.
    /// </summary>
    internal static void VerifyBotwoonOpeningWait()
    {
        (RoomEnemySystem enemies, RoomEnemyPlace head, BotwoonEnemyState state) = NewBotwoon();
        AssertEqual(256, BotwoonFightDefinitions.OpeningDelay, "the opening wait lasts the 256 updates the game counted");
        AssertEqual(BotwoonPhase.InitialDelay, state.Phase, "the fight starts in its opening wait");
        state.OpeningWait.LastFor(BotwoonFightDefinitions.OpeningDelay);
        for (int update = 1; update < BotwoonFightDefinitions.OpeningDelay; update++)
        {
            enemies.BotwoonBehavior.RunBotwoonMainAi(head.HeldOccupant(), head, Frame());
            AssertEqual(BotwoonPhase.InitialDelay, state.Phase, $"Botwoon still waits in his hole on update {update}");
        }
        enemies.BotwoonBehavior.RunBotwoonMainAi(head.HeldOccupant(), head, Frame());
        AssertEqual(BotwoonPhase.ChooseNextAction, state.Phase, "the opening wait runs out on its 256th update");
        Console.WriteLine("  Botwoon opening wait: 256 updates in his hole, then he chooses his first action.");
    }

    /// <summary>
    /// #2267: once his tail has finished entering the wall after the fatal hit, Botwoon waits sixteen updates, that update
    /// included, and his head falls from the next: the game counted 240 up to 256 from that update.
    /// </summary>
    internal static void VerifyBotwoonPreFallWait()
    {
        (RoomEnemySystem enemies, RoomEnemyPlace head, BotwoonEnemyState state) = NewBotwoon();
        state.Phase = BotwoonPhase.FollowAuthoredPath;
        state.FatallyHit = true;
        state.ExitTransitionComplete = true;
        for (int update = 1; update <= 15; update++)
        {
            enemies.BotwoonBehavior.RunBotwoonMainAi(head.HeldOccupant(), head, Frame());
            AssertEqual(BotwoonPhase.DeathDelay, state.Phase, $"Botwoon still waits to fall on update {update} of his wait");
        }
        enemies.BotwoonBehavior.RunBotwoonMainAi(head.HeldOccupant(), head, Frame());
        AssertEqual(BotwoonPhase.HeadFalling, state.Phase, "Botwoon's head falls from the update after the sixteenth of his wait");
        Console.WriteLine("  Botwoon pre-fall wait: sixteen updates from his tail entering the wall, then his head falls.");
    }

    /// <summary>
    /// #2463: once his body's death starts, each segment waits by the enemy-projectile place it was spawned into, that
    /// update included, and falls from the next, as the game counted 96 plus eight a place up to 256 from that update; the
    /// dying segment's frame hold stops, so it keeps the picture it shows.
    /// </summary>
    internal static void VerifyBotwoonSegmentFallWait()
    {
        (RoomEnemySystem enemies, _, BotwoonEnemyState state) = NewBotwoon();
        state.BodyDeathStarted = true;
        static int GameWait(BotwoonSegmentState segment) => 256 - (96 + 8 * segment.Projectile.Place);
        int longest = state.Body.Max(GameWait);
        for (int update = 1; update <= longest; update++)
        {
            foreach (BotwoonSegmentState segment in state.Body.Where(segment => update <= GameWait(segment)))
            {
                enemies.BotwoonBehavior.MoveBotwoonBodySegment(segment.Projectile, 0);
                string at = $"segment {segment.Index} in projectile place {segment.Projectile.Place} on update {update} of its death";
                AssertEqual(update == GameWait(segment) ? BotwoonBodyPhase.Falling : BotwoonBodyPhase.DyingDelay, segment.Phase, at);
                AssertTrue(segment.Projectile.IsFrameHoldStopped, $"{at}: the dying segment holds its picture");
            }
        }
        Console.WriteLine("  Botwoon segment fall wait: each dying segment falls by its projectile place, later places sooner.");
    }

    /// <summary>A frame of Botwoon's main AI, with Samus standing clear of him.</summary>
    private static ClockedEnemyAiFrame Frame() => new(
        RoomEnemyFrameFixtures.Standalone(RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), 0) with { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, Samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) },
        new FrameCounters(0));

    /// <summary>
    /// A head in slot zero bound as Botwoon, away from every hole, with its thirteen body segments spawned by the
    /// production spawner and its health palette complete so the update reads no artwork.
    /// </summary>
    private static NewBotwoonHeadResult NewBotwoon()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().NextRandom = () => 0;
        RoomEnemyPlace head = enemies.Places.InUpdateOrder[0];
        head.Bind(EnemyDefinitionId.Botwoon, head.Definition);
        head.Position = head.Position.WithWhole(new(1000, 1000));
        var state = new BotwoonEnemyState(enemies.BotwoonBehavior.SpawnBotwoonBody(head.HeldOccupant(), head))
        {
            HealthBand = BotwoonHealthBandDefinitions.BelowEighth,
        };
        head.SetSpeciesState(state);
        return new NewBotwoonHeadResult(enemies, head, state);
    }

    /// <summary>Runs the Botwoon attack-cooldown check.</summary>
    [VerificationCommand("--botwoon-attack-cooldown")]
    internal static int BotwoonAttackCooldownCommand()
    {
        Suite(nameof(VerifyBotwoonAttackCooldown), () => VerifyBotwoonAttackCooldown());
        Suite(nameof(VerifyBotwoonPreFallWait), () => VerifyBotwoonPreFallWait());
        Suite(nameof(VerifyBotwoonSegmentFallWait), () => VerifyBotwoonSegmentFallWait());
        Suite(nameof(VerifyBotwoonOpeningWait), () => VerifyBotwoonOpeningWait());
        return 0;
    }
}

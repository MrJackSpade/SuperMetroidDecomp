using SuperMetroid.Core;
using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.Dragon;

/// <summary>Checks Dragon's six facing and phase selectors against retail pointers and live handoffs.</summary>
internal static class DragonAnimationDefinitionsVerification
{
    /// <summary>Compares the selector table with the cartridge, then exercises installs and both facings.</summary>
    internal static void VerifyDragonAnimationDefinitions(CartridgeImportAddressSpace rom)
    {
        for (int index = 0; index < 6; index++)
        {
            // The cartridge pairs its six ordinals as facing left (even) and right (odd) within each phase.
            DragonSegment selector = Selector(index);
            AssertEqual(rom.ReadCartridgeWord((0xa2e5ef + index * 2)),
                NativeEnemyAnimationOracle.EntryOf(DragonNativeListAddresses.Entries, selector),
                $"Dragon animation selector {selector}");
        }
        foreach (HorizontalDirection facing in new[] { HorizontalDirection.Left, HorizontalDirection.Right })
        {
            int side = facing == HorizontalDirection.Left ? 0 : 1;
            AssertEqual(Selector(side), DragonPhases.Idle(facing), $"Dragon idle facing {facing}");
            AssertEqual(Selector(2 | side), DragonPhases.Wings(facing), $"Dragon wings facing {facing}");
            AssertEqual(Selector(4 | side), DragonPhases.Attacking(facing), $"Dragon attack facing {facing}");
        }

        Suite(nameof(VerifyAllDragonAnimationInstalls), () => VerifyAllDragonAnimationInstalls());
        Suite(nameof(VerifyLiveDragonAnimationHandoffs), () => VerifyLiveDragonAnimationHandoffs(facingLeft: false));
        Suite(nameof(VerifyLiveDragonAnimationHandoffs), () => VerifyLiveDragonAnimationHandoffs(facingLeft: true));

        Suite(nameof(VerifyDragonRequestForPlayingAnimation), () => VerifyDragonRequestForPlayingAnimation());
        Suite(nameof(VerifyDragonAttackStartsOverAfterFireball), () => VerifyDragonAttackStartsOverAfterFireball());
        Console.WriteLine(
            "Dragon animation definitions: six native selectors, phase/facing mappings, all six installs, a request for the playing animation, the attack starting over after each fireball, and both live body/wing facing and attack handoffs pass with the pointer table forbidden.");
    }

    /// <summary>The animation the cartridge's table ordinal <paramref name="index"/> names.</summary>
    private static DragonSegment Selector(int index) =>
        DragonNativeWords.Animations.Decode(index, "Dragon animation ordinal");

    /// <summary>A request for the animation a part plays leaves it playing where it stands and its frame timer as it is (#2145).</summary>
    private static void VerifyDragonRequestForPlayingAnimation()
    {
        RoomEnemyPlace place = NewDragonAnimationPlace(0);
        DragonPartState state = NewDragonAnimationState();
        var playing = new AnimationPlayback<DragonSegment>(DragonAnimations.StartingWith(DragonSegment.AttackingFacingLeft), new AnimationPlace(0, 2));
        state.Playback = playing;
        DragonBehavior.StartDragonAnimation(place, state, DragonSegment.AttackingFacingLeft);
        AssertEqual(playing, state.Playback, "Dragon keeps playing the requested animation where it stands");
        AssertEqual(0x2222, place.AnimationClock.Hold, "Dragon installs nothing for the playing animation");
    }

    /// <summary>
    /// The update a finished attack spits its fireball, the attack rests on its last pose; the next update it starts over,
    /// which spaces the three fireballs by a whole attack animation (#2145; the cartridge clears the installed ordinal).
    /// </summary>
    private static void VerifyDragonAttackStartsOverAfterFireball()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace body = enemies.Places.InUpdateOrder[0];
        RoomEnemyPlace wing = enemies.Places.InUpdateOrder[1];
        PlaceDragon(enemies, body, wing);
        DragonEnemyState bodyState = enemies.DragonStates[0] ?? throw new InvalidOperationException("Verification requires enemies.DragonStates[0].");
        Animation<DragonSegment> attack = DragonAnimations.StartingWith(DragonSegment.AttackingFacingRight);
        var playedOut = new AnimationPlayback<DragonSegment>(attack, AnimationPlace.HoldingIn(0, attack.Phases[0].BeatCount));
        bodyState.Action = DragonAction.AttackFinished;
        bodyState.Facing = HorizontalDirection.Right;
        bodyState.AttackCounter = 3;
        bodyState.Playback = playedOut;

        enemies.DragonBehavior.RunDragonAttacking(body.HeldOccupant(), body, bodyState);
        AssertEqual(2, bodyState.AttackCounter, "the finished attack spits a fireball");
        AssertEqual(playedOut, bodyState.Playback, "the attack rests on its last pose the update it spits");
        AssertEqual(DragonAction.RestartingAttack, bodyState.Action, "a fireball with shots left restarts the attack next");

        enemies.DragonBehavior.RunDragonAttacking(body.HeldOccupant(), body, bodyState);
        AssertDragonAnimationHandoff(body, bodyState, DragonSegment.AttackingFacingRight, "the attack after a fireball");
        AssertEqual(2, bodyState.AttackCounter, "the restarted attack spits nothing until it finishes again");
        AssertEqual(DragonAction.Attacking, bodyState.Action, "the restarted attack plays until it finishes");

        enemies.DragonBehavior.RunDragonAttacking(body.HeldOccupant(), body, bodyState);
        AssertEqual(new AnimationPlayback<DragonSegment>(attack, AnimationPlace.StartOfAnimation), bodyState.Playback,
            "the restarted attack asked for again plays on where it stands");
    }

    /// <summary>Forces each selector into a fresh slot to confirm pointer and timer installation.</summary>
    private static void VerifyAllDragonAnimationInstalls()
    {
        for (int index = 0; index < 6; index++)
        {
            DragonSegment requested = Selector(index);
            var place = NewDragonAnimationPlace(0);
            DragonPartState state = NewDragonAnimationState();
            DragonBehavior.StartDragonAnimation(place, state, requested);
            AssertDragonAnimationHandoff(place, state, requested, $"Dragon {requested}");
        }
    }

    /// <summary>Runs body and wing through idle, rise, and attack transitions for one Samus-facing direction.</summary>
    private static void VerifyLiveDragonAnimationHandoffs(
        bool facingLeft)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace body = enemies.Places.InUpdateOrder[0];
        RoomEnemyPlace wing = enemies.Places.InUpdateOrder[1];
        wing.Position = wing.Position.WithWholeX(0x0100);
        body.Position = body.Position.WithWholeX(0x0100);
        wing.Position = wing.Position.WithWholeY(0x0100);
        body.Position = body.Position.WithWholeY(0x0100);

        PlaceDragon(enemies, body, wing);
        DragonEnemyState bodyState = (enemies.DragonStates[0] ?? throw new InvalidOperationException("Verification requires enemies.DragonStates[0]."));
        // The body's state holds the wing's, which the body's steps start the wing's animations on.
        DragonWingState wingState = bodyState.WingState;
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
        {
            Position = new(new(unchecked((ushort)(body.Position.X.Whole + (facingLeft ? -0x10 : 0x10))), 0), new(0, 0)),
        };

        DragonBehavior.RunDragonWaitToRise(body, (bodyState), samus);
        DragonSegment idle = facingLeft
            ? DragonSegment.IdleFacingLeft
            : DragonSegment.IdleFacingRight;
        DragonSegment wings = facingLeft
            ? DragonSegment.WingsFacingLeft
            : DragonSegment.WingsFacingRight;
        AssertDragonAnimationHandoff(
            body,
            bodyState,
            idle,
            facingLeft ? "live left-facing Dragon body" : "live right-facing Dragon body",
            expectedFrameHold: facingLeft ? (ushort)0 : (ushort)1);
        AssertDragonAnimationHandoff(
            wing,
            (wingState),
            wings,
            facingLeft ? "live left-facing Dragon wing" : "live right-facing Dragon wing",
            expectedFrameHold: facingLeft ? (ushort)0 : (ushort)1);

        bodyState.ActionTimer = UpdateCountdown.Ended();
        DragonBehavior.RunDragonRising(body, bodyState);
        enemies.DragonBehavior.RunDragonAttacking(body.HeldOccupant(), body, bodyState);
        DragonSegment attacking = facingLeft
            ? DragonSegment.AttackingFacingLeft
            : DragonSegment.AttackingFacingRight;
        AssertDragonAnimationHandoff(
            body,
            bodyState,
            attacking,
            facingLeft ? "live left-facing Dragon attack" : "live right-facing Dragon attack");
    }

    /// <summary>Places a Dragon body and the wing listed after it, each bound as its own part and initialized.</summary>
    private static void PlaceDragon(RoomEnemySystem enemies, RoomEnemyPlace body, RoomEnemyPlace wing)
    {
        body.SetSpawnParameter1(EnemyDefinitionId.Dragon, 0);
        wing.SetSpawnParameter1(EnemyDefinitionId.Dragon, 1);
        body.Bind(EnemyDefinitionId.Dragon, body.KeptDefinition);
        wing.Bind(EnemyDefinitionId.Dragon, wing.KeptDefinition);
        enemies.DragonBehavior.InitializeDragonBody(body);
        DragonBehavior.InitializeDragonWing(wing, new DragonWingState());
    }

    /// <summary>Starts with nondefault instruction values so an omitted install remains observable.</summary>
    private static RoomEnemyPlace NewDragonAnimationPlace(int index) =>
        new RoomEnemyPlace(index, EnemyPlaceFixtures.Registry.Common, new RemovedEnemyLeftovers<IEnemyEntity>()).WithAnimationClock(hold: 0x2222);

    /// <summary>Provides a fresh Dragon state for a standalone animation slot.</summary>
    private static DragonWingState NewDragonAnimationState() => new();

    /// <summary>Confirms the requested animation started from its beginning, clearing the previous loop timer.</summary>
    private static void AssertDragonAnimationHandoff(
        RoomEnemyPlace place,
        DragonPartState state,
        DragonSegment expected,
        string context,
        ushort expectedFrameHold = 1)
    {
        AssertEqual(new AnimationPlayback<DragonSegment>(DragonAnimations.StartingWith(expected), AnimationPlace.StartOfAnimation),
            state.Playback,
            $"{context} animation");
        AssertEqual(expectedFrameHold, place.AnimationClock.Hold,
            $"{context} instruction timer");
    }

}

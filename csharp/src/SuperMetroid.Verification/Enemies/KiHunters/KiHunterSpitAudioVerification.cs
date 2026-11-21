using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.KiHunters;

using SuperMetroid.Verification.Audio;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

/// <summary>Checks that one Ki-Hunter spit requests one sound and cannot prolong a door sound wait.</summary>
internal static class KiHunterSpitAudioVerification
{
    /// <summary>Checks the native sound opcode, real projectile spawn, single SPC start, and door transition.</summary>
    internal static void VerifyKiHunterSpitAudio()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        byte[] nativeQueueCall = [0xa9, 0x4c, 0x00, 0x22, 0xcb, 0x90, 0x80];
        AssertSequenceEqual(nativeQueueCall, Enumerable.Range(0, nativeQueueCall.Length)
            .Select(offset => bus.ReadCartridgeByte(0xa8f6dc + offset)),
            "the spit instruction requests the acid-spit sound once");
        (SuperMetroidGame game, SuperMetroidRuntime runtime, SamusState samus, RoomEnemyPlace body, KiHunterBodyState state) = StageSpit();
        var spit = EnemySound.AcidSpit;
        int requests = 0, starts = 0;
        void Step()
        {
            runtime.AudioRequests.BeginRecordedUpdate();
            var frame = game.Step(0);
            requests += runtime.AudioRequests.SoundsQueued.Count(sound => sound == spit);
            starts += frame.Audio.Events.Count(audioEvent => audioEvent is AudioEvent.StartSoundEffect started && started.SoundEffect == spit);
        }
        Step();
        AssertEqual(1, runtime.Enemies.Projectiles.EnemyProjectiles.Count(projectile => projectile.Kind == RoomEnemyProjectileKind.KiHunterAcidSpit && !projectile.Velocity.X.IsNegative), "staged attack spawns one real acid projectile");
        AssertTrue(runtime.AudioRequests.SoundsQueued.Contains(spit), "the spit sound is queued");
        // Remain in the post-spit waiting state without introducing another attack.
        // The right spit animation's final sleep.
        state.Playback = new(KiHunterAnimations.StartingWith(KiHunterSegment.SpitRight), new AnimationPlace(0, 8));
        body.AnimationClock.StepOnNextUpdate();
        for (int frame = 1; frame < 20; frame++) Step();
        Console.WriteLine($"One KiHunter spit: {requests} requests, {starts} SPC starts over 20 frames.");
        var level = runtime.Room.LoadedLevelData;
        bool selected = false;
        for (int y = 0; y < level.Size.Y && !selected; y++)
        for (int x = 0; x < level.Size.X && !selected; x++)
        {
            var block = level.GetCollisionBlock(new(checked((ushort)x), checked((ushort)y)));
            if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
            runtime.MeetDoorBlock(block, samus.Pose.ForRoomCollision());
            selected = true;
        }
        AssertTrue(selected, "real KiHunter room has an exit");
        game.Session.Enter(DoorTransitionStep.EnteringDoor);
        int waitFrames = 0;
        for (int frame = 0; frame < 32; frame++)
        {
            if (game.DoorTransitionPhaseForVerification is DoorTransitionPhase.Running.WaitForSoundQueues) waitFrames++;
            Step();
            if (game.DoorTransitionPhaseForVerification is DoorTransitionPhase.Running.FadeOutSourceRoom) break;
        }
        Console.WriteLine($"Door sound wait: {waitFrames} frames; phase={game.DoorTransitionPhaseForVerification}.");
        AssertEqual(1, requests, "one native spit opcode publishes exactly one request, including door-wait frames");
        AssertEqual(1, starts, "the SPC starts the acknowledged spit once");
        AssertEqual(new DoorTransitionPhase.Running.FadeOutSourceRoom(), game.DoorTransitionPhaseForVerification, "completed spit cannot refill the door sound wait");
        AssertTrue(waitFrames <= 1, "already-drained spit queue allows the first door sound-wait pass to finish");
    }

    /// <summary>
    /// Checks the spit's priority as the queue shows it: the spit's request is admitted at normal priority, so behind five
    /// library-two requests made earlier in its update it joins the queue, and behind six the queue turns it away.
    /// </summary>
    internal static void VerifyKiHunterSpitPriority()
    {
        foreach ((int competing, bool joins) in new[] { (5, true), (6, false) })
        {
            (SuperMetroidGame game, SuperMetroidRuntime runtime, _, _, _) = StageSpit();
            runtime.AudioRequests.BeginRecordedUpdate();
            runtime.AudioRequests.FillQueueAhead(EnemySound.Beacon, competing);
            _ = game.Step(0);
            AssertEqual(1, runtime.Enemies.Projectiles.EnemyProjectiles.Count(projectile => projectile.Kind == RoomEnemyProjectileKind.KiHunterAcidSpit),
                $"the staged attack spits behind {competing} waiting requests");
            AssertEqual(joins, runtime.AudioRequests.SoundsQueued.Contains(EnemySound.AcidSpit),
                $"behind {competing} waiting library-two requests the normal-priority spit sound {(joins ? "joins" : "is turned away")}");
        }
        Console.WriteLine("  KiHunter spit priority: the spit's sound joins behind five waiting requests and not behind six.");
    }

    /// <summary>
    /// A retail game in Crateria's Ki-Hunter room with Samus locked out to its left and the green Ki-Hunter on the ground,
    /// its right spit animation staged on the spit cue for its next update.
    /// </summary>
    private static (SuperMetroidGame Game, SuperMetroidRuntime Runtime, SamusState Samus, RoomEnemyPlace Body, KiHunterBodyState State) StageSpit()
    {
        var random = new SharedRandom();
        var game = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps), renderGameplayFrames: false,
            hostServices: global::SuperMetroid.Core.Frontend.SuperMetroidGameHostServicesFixtures.With(soundAcceptance: NextUpdateSoundAcceptance.Instance), random: random);
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true, random: random, audio: game.AudioForVerification);
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.CrateriaKihunter);
        var samus = runtime.Player.IntroducedSamus;
        samus.PoseId = SamusPoseId.FacingRightNormalPose;
        samus.Position = samus.Position.WithWhole(new(64, 128));
        samus.Control.LockOutPlayer();
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        var body = runtime.Enemies.Places.InUpdateOrder.First(place => place.Holds(EnemyDefinitionId.KihunterGreen));
        var state = runtime.Enemies.KiHunterBodyStates[body.Ordinal] ?? throw new InvalidOperationException("Verification requires state.");
        body.Position = body.Position.WithWhole(new(128, 128));
        state.Action = KiHunterAction.AnimatingOnGround;
        // Stage the real spit opcode; subsequent frames must not replay its one-shot call.
        // The spit cue, four poses into the right spit animation.
        state.Playback = new(KiHunterAnimations.StartingWith(KiHunterSegment.SpitRight), new AnimationPlace(0, 4));
        body.AnimationClock.StepOnNextUpdate();
        game.EnterMainGameplay(runtime);
        return (game, runtime, samus, body, state);
    }

    /// <summary>Runs the focused Ki-Hunter spit sound-routing check.</summary>
    [VerificationCommand("--kihunter-spit-audio")]
    internal static int KihunterSpitAudioCommand()
    {
        VerifyKiHunterSpitAudio();
        VerifyKiHunterSpitPriority();
        return 0;
    }
}

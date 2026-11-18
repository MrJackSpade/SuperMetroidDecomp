using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
namespace SuperMetroid.Verification.Audio;

/// <summary>
/// Checks that every gameplay system queues its sound into the session's queues the moment it makes it, in the update's
/// run order across systems, as the game's callers each queue theirs directly while they run (#1620).
/// </summary>
internal static class GameplayAudioOrderVerification
{
    /// <summary>
    /// One gameplay update of the Crateria Ki Hunter room in which Samus fires a missile, a Ki Hunter spits and the HUD
    /// shows the newly selected missiles. The main gameplay routine runs Samus's input handler (which fires) before the
    /// enemies (which spit) and both before the HUD (which queues the selection sound), so the update's requests are the
    /// missile, the spit and the selection, in that order, and Samus's sound library receives the missile before the
    /// selection. The port's former per-system join queued the HUD's selection first and the enemies' spit last.
    /// </summary>
    internal static void VerifyGameplaySoundsQueueInRunOrder()
    {
        (SuperMetroidRuntime runtime, AudioQueues audio) = KiHunterRoomWithSessionAudio();
        _ = KiHunterAtItsSpit(runtime);
        SamusState samus = runtime.Player.IntroducedSamus;
        samus.Missiles = samus.MaxMissiles = 5;
        samus.SelectedHudItem = HudItem.Missile;
        SoundRequestsMark samusLibrary = audio.MarkSoundRequestsForVerification(SoundEffectLibrary.Samus);
        SoundRequestsMark enemyLibrary = audio.MarkSoundRequestsForVerification(SoundEffectLibrary.Enemies);

        runtime.AudioRequests.BeginRecordedUpdate();
        runtime.StepFrame(ControllerButton.X, new RuntimeFrameRequest { Policy = RuntimeFramePolicy.Gameplay with { RunsLowHealthCheck = true } });

        SoundEffectId missile = SamusProjectileSoundRoutingDefinitions.MissileRequest;
        SoundEffectId[] observed = [.. runtime.AudioRequests.SoundRequests.Select(request => request.SoundEffect)
            .Where(sound => sound == missile || sound == EnemySound.AcidSpit || sound == SamusSound.HudWeaponSelect)];
        AssertSequenceEqual(new[] { missile, EnemySound.AcidSpit, SamusSound.HudWeaponSelect }, observed,
            "the update's requests follow its run order: Samus's missile, the Ki Hunter's spit, then the HUD's selection");
        AssertSequenceEqual(new[] { missile, SamusSound.HudWeaponSelect },
            audio.SoundsQueuedSinceForVerification(samusLibrary).Where(sound => sound == missile || sound == SamusSound.HudWeaponSelect),
            "Samus's library receives the missile, queued as it fired, before the HUD's later selection");
        AssertEqual(1, audio.SoundsQueuedSinceForVerification(enemyLibrary).Count(sound => sound == EnemySound.AcidSpit),
            "the enemies' library receives the spit once, as the Ki Hunter makes it");
        Console.WriteLine("  Gameplay sound order: missile, Ki Hunter spit and HUD selection are queued in the update's run order.");
    }

    /// <summary>
    /// The update after the Ki Hunter's spit makes no enemy request: the spit reaches the enemies' library once across both
    /// updates, and the later update requests no spit.
    /// </summary>
    internal static void VerifyLaterUpdateQueuesNothingStale()
    {
        (SuperMetroidRuntime runtime, AudioQueues audio) = KiHunterRoomWithSessionAudio();
        RoomEnemyPlace kiHunter = KiHunterAtItsSpit(runtime);
        SoundRequestsMark enemyLibrary = audio.MarkSoundRequestsForVerification(SoundEffectLibrary.Enemies);
        runtime.AudioRequests.BeginRecordedUpdate();
        runtime.StepFrame(0, new RuntimeFrameRequest { Policy = RuntimeFramePolicy.Gameplay with { RunsLowHealthCheck = true } });
        AssertTrue(runtime.AudioRequests.SoundRequests.Any(request => request.SoundEffect == EnemySound.AcidSpit),
            "the first update requests the Ki Hunter's spit");

        // The Ki Hunter's next step lies far ahead, so the second update's enemies request nothing.
        kiHunter.AnimationClock.HoldFor(200);
        runtime.AudioRequests.BeginRecordedUpdate();
        runtime.StepFrame(0, new RuntimeFrameRequest { Policy = RuntimeFramePolicy.Gameplay with { RunsLowHealthCheck = true } });

        AssertTrue(!runtime.AudioRequests.SoundRequests.Any(request => request.SoundEffect == EnemySound.AcidSpit),
            "the second update requests no spit");
        AssertEqual(1, audio.SoundsQueuedSinceForVerification(enemyLibrary).Count(sound => sound == EnemySound.AcidSpit),
            "the spit reaches the enemies' library once across both updates");
        Console.WriteLine("  Stale audio: an update that makes no enemy request queues none of an earlier update's.");
    }

    /// <summary>
    /// The Speed Booster's echo is answered by its queue after the update's earlier sounds already joined it: a landing
    /// queued as Samus lands is waiting when the echo is requested, so the echo is admitted behind it, and a landing after
    /// the echo follows it. While a power bomb explodes the echo is still tested against its queue but nothing joins it.
    /// </summary>
    internal static void VerifyEchoFollowsTheUpdatesEarlierSounds()
    {
        (SuperMetroidRuntime runtime, AudioQueues audio) = KiHunterRoomWithSessionAudio();
        SamusState samus = runtime.Player.IntroducedSamus;
        samus.LiquidPhysics.BeginFrameSoundRequests();
        audio.AdvanceFrame(default);
        // The echo Samus's animation requests at its cue joins her audio requests (#2092).
        SoundQueueAdmission echo() => samus.AudioRequests.QueueSound(RoomSound.SpeedBooster, SoundRequestPriority.Normal);

        samus.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(samus,
            SamusMovementType.Standing, SamusPoseId.FacingRightNormalPose, new UnsignedSixteenSixteen(5, 0));
        AssertEqual(SoundQueueAdmission.Admitted(1), echo(), "the echo is admitted behind the landing already waiting");
        samus.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(samus,
            SamusMovementType.Standing, SamusPoseId.FacingRightNormalPose, new UnsignedSixteenSixteen(1, 1));

        var played = new List<SoundEffectId>();
        SoundEffectId? acknowledgement = null;
        for (int frame = 0; frame < 128; frame++)
        {
            foreach (AudioRequest command in audio.AdvanceFrame(new SoundAcknowledgements<SoundEffectLibrary, SoundEffectId>(
                [SoundEffectLibrary.Room], _ => SoundAcknowledgement<SoundEffectId>.Of(acknowledgement))))
            {
                bool recognized = false;
                foreach (SoundEffectId? value in new SoundEffectId?[] { null, RoomSound.SpeedBooster, RoomSound.HardLanding, RoomSound.Landing })
                {
                    if (command != AudioRequest.ForSound(SoundEffectLibrary.Room, value)) continue;
                    acknowledgement = value;
                    if (value is SoundEffectId sound) played.Add(sound);
                    recognized = true;
                    break;
                }
                AssertTrue(recognized, "the room library is asked only for the landings and the echo");
            }
        }
        AssertSequenceEqual(new SoundEffectId[] { RoomSound.HardLanding, RoomSound.SpeedBooster, RoomSound.Landing }, played,
            "the earlier landing, the echo and the later landing each play once, in request order");

        runtime.Room.LoadedScope.BombProjectiles.PowerBombExplosion.ForceActive(true);
        AssertEqual(SoundQueueAdmission.Admitted(0), echo(), "an exploding power bomb leaves the echo's admission readable");
        AssertTrue(!audio.HasQueuedSounds, "an exploding power bomb lets no echo join its queue");
        Console.WriteLine("  Echo order: the echo's queue holds the update's earlier sounds when it answers.");
    }

    /// <summary>
    /// A runtime built outside a session (the tooling factory given no queues) runs over queues of its own (#2057): every
    /// request it makes is tested against them and joins them, so each answers with what its queue did, the first finding
    /// the queue empty and the next finding the first waiting; no request reaches no queue.
    /// </summary>
    internal static void VerifyRuntimeOutsideASessionAnswersFromItsOwnQueues()
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture();
        AssertEqual(SoundQueueAdmission.Admitted(0), runtime.AudioRequests.QueueSound(RoomSound.SpeedBooster, SoundRequestPriority.Normal),
            "a runtime outside a session answers its first request from its own empty queue");
        AssertEqual(SoundQueueAdmission.Admitted(1), runtime.AudioRequests.QueueSound(RoomSound.SpeedBooster, SoundRequestPriority.Normal),
            "its next request finds the first waiting in its own queue");
    }

    /// <summary>
    /// The Crateria Ki Hunter room with Samus standing free to move and fire, run by a game whose session queues receive
    /// the runtime's requests.
    /// </summary>
    private static (SuperMetroidRuntime Runtime, AudioQueues Audio) KiHunterRoomWithSessionAudio()
    {
        var random = new SharedRandom();
        SuperMetroidGame game = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps), renderGameplayFrames: false, random: random);
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true, random: random, audio: game.AudioForVerification);
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.CrateriaKihunter);
        SamusState samus = runtime.Player.IntroducedSamus;
        samus.PoseId = SamusPoseId.FacingLeftNormalPose;
        samus.Position = samus.Position.WithWhole(new(64, 128));
        samus.Control.Unlock();
        runtime.Player.GroundedSamusMovementEnabled = true;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        game.AdoptRuntime(runtime);
        game.Session.LastAudioRoomState = runtime.Room.LoadedRoom.State;
        return (runtime, game.AudioForVerification);
    }

    /// <summary>The room's green Ki Hunter, its AI idle and its script one update from the spit.</summary>
    private static RoomEnemyPlace KiHunterAtItsSpit(SuperMetroidRuntime runtime)
    {
        RoomEnemyPlace body = runtime.Enemies.Places.InUpdateOrder.First(place => place.DefinitionId == EnemyDefinitionId.KihunterGreen);
        KiHunterBodyState state = runtime.Enemies.KiHunterBodyStates[body.Ordinal]
            ?? throw new InvalidOperationException("The Ki Hunter has no state.");
        body.Position = body.Position.WithWhole(new(128, 128));
        state.Action = KiHunterAction.AnimatingOnGround;
        // The spit cue, four poses into the right spit animation, which queues the acid spit (library two).
        state.Playback = new(KiHunterAnimations.StartingWith(KiHunterSegment.SpitRight), new AnimationPlace(0, 4));
        body.AnimationClock.StepOnNextUpdate();
        return body;
    }
}

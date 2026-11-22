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
        // The Attic after Phantoon, whose wingless Ki-Hunter spits at Samus on its left as its own AI and animation lead it to.
        (SuperMetroidGame game, SuperMetroidRuntime runtime) = KiHunterSpitStaging.AtticGame(
            global::SuperMetroid.Core.Frontend.SuperMetroidGameHostServicesFixtures.With(soundAcceptance: NextUpdateSoundAcceptance.Instance));
        var samus = runtime.Player.Samus;
        var spit = EnemySound.AcidSpit;
        int requests = 0, starts = 0;
        void Step()
        {
            runtime.AudioRequests.BeginRecordedUpdate();
            FrontendFrame frame = game.StepCaptured(0).Frame;
            requests += runtime.AudioRequests.SoundsQueued.Count(sound => sound == spit);
            starts += frame.Audio.Events.Count(audioEvent => audioEvent is AudioEvent.StartSoundEffect started && started.SoundEffect == spit);
        }
        while (requests == 0)
            Step();
        AssertEqual(1, runtime.Enemies.Projectiles.EnemyProjectiles.Count(projectile => projectile.Kind == RoomEnemyProjectileKind.KiHunterAcidSpit && projectile.Velocity.X.IsNegative),
            "the spit spawns one real acid projectile, toward Samus on its left");
        AssertTrue(runtime.AudioRequests.SoundsQueued.Contains(spit), "the spit sound is queued");
        // The Ki-Hunter holds its spent spit and then waits; its next spit lies far beyond these updates and the door's.
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
        AssertTrue(selected, "the Attic has an exit");
        // The game enters the transition through the door Samus's collision entered, as it does when she reaches it.
        game.Session.EnterDoor(runtime.EnteredDoor ?? throw new InvalidOperationException("Samus's collision entered no door."));
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
            (SuperMetroidGame game, SuperMetroidRuntime runtime) = StageSpit();
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
    /// The Attic after Phantoon run to the update before its wingless Ki-Hunter's own spit, which a replica run of the same
    /// room counts (#2554): the next update spits.
    /// </summary>
    private static (SuperMetroidGame Game, SuperMetroidRuntime Runtime) StageSpit()
    {
        int updatesBeforeTheSpit = KiHunterSpitStaging.UpdatesBeforeTheSpit(() =>
        {
            (SuperMetroidGame replica, _) = KiHunterSpitStaging.AtticGame();
            return (replica.AudioForVerification, () => replica.StepCaptured(0));
        });
        (SuperMetroidGame game, SuperMetroidRuntime runtime) = KiHunterSpitStaging.AtticGame();
        for (int update = 0; update < updatesBeforeTheSpit; update++)
            game.StepCaptured(0);
        return (game, runtime);
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

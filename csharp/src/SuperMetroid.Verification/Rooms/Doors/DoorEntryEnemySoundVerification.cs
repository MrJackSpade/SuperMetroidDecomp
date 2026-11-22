using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core;
namespace SuperMetroid.Verification.Rooms.Doors;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Verification.Enemies.KiHunters;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rooms.Doors;

/// <summary>Reproduces enemy sound ordering on the first door-entry update.</summary>
internal static class DoorEntryEnemySoundVerification
{
    /// <summary>
    /// Confirms state $0A ($82:E1B7) runs enemy instructions before the door's $71
    /// cancel and DisableSounds. The 100% movie exposed an entry-frame enemy sound
    /// that lengthened the Metal Pirates exit's sound-queue wait.
    /// </summary>
    internal static void VerifyDoorEntryEnemySound()
    {
        // The Attic after Phantoon runs to the update before its Ki-Hunter's own spit, which a replica run of the same room
        // counts; the spit's cue (library two $4C) then runs during the entry frame's enemy pass.
        int updatesBeforeTheSpit = KiHunterSpitStaging.UpdatesBeforeTheSpit(() =>
        {
            (SuperMetroidGame replica, _) = KiHunterSpitStaging.AtticGame();
            return (replica.AudioForVerification, () => replica.StepCaptured(0));
        });
        (SuperMetroidGame game, SuperMetroidRuntime runtime) = KiHunterSpitStaging.AtticGame();
        for (int update = 0; update < updatesBeforeTheSpit; update++)
            game.StepCaptured(0);
        var samus = runtime.Player.Samus;
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
        var audio = game.AudioForVerification;
        SoundRequestsMark start = audio.MarkSoundRequestsForVerification(SoundEffectLibrary.Enemies);
        // The game enters the transition through the door Samus's collision entered, as it does when she reaches it.
        game.Session.EnterDoor(runtime.EnteredDoor ?? throw new InvalidOperationException("Samus's collision entered no door."));
        game.StepCaptured(0);

        IReadOnlyList<SoundEffectId> queued = audio.SoundsQueuedSinceForVerification(start);
        AssertTrue(queued.Count >= 2, "the entry frame queues at least the enemy sound and the door's cancel");
        AssertEqual<SoundEffectId>(EnemySound.AcidSpit, queued[0],
            "the entry frame's enemy pass queues the spit first");
        AssertEqual<SoundEffectId>(EnemySoundStop.Any, queued[1],
            "the door's library-two cancel follows the enemy sound");
        Console.WriteLine("Door entry enemy sound: the entry frame's enemy sounds precede the door's cancel and DisableSounds.");
    }

    /// <summary>Runs the focused enemy-before-door sound ordering check.</summary>
    [VerificationCommand("--door-entry-enemy-sound")]
    internal static int DoorEntryEnemySoundCommand()
    {
        VerifyDoorEntryEnemySound();
        return 0;
    }
}

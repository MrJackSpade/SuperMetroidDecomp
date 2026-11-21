using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
namespace SuperMetroid.Verification.Rooms.Doors;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Verification.Enemies.KiHunters;

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
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        (SuperMetroidGame game, SuperMetroidRuntime runtime) = KiHunterRoom();
        var samus = runtime.Player.Samus;
        // The real spit opcode (library two $4C) executes during the entry frame's enemy pass: the room runs, nothing pressed,
        // to the update before the one its wingless Ki Hunter spits on, as a run of the same room counts them.
        KiHunterSpitStaging.Run(runtime, KiHunterSpitStaging.UpdatesBeforeTheSpit(() => KiHunterRoom().Runtime));
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
        AssertTrue(selected, "the Ki Hunter room has an exit");
        var audio = game.AudioForVerification;
        SoundRequestsMark start = audio.MarkSoundRequestsForVerification(SoundEffectLibrary.Enemies);
        game.Session.Enter(DoorTransitionStep.EnteringDoor);
        game.Step(0);

        IReadOnlyList<SoundEffectId> queued = audio.SoundsQueuedSinceForVerification(start);
        AssertTrue(queued.Count >= 2, "the entry frame queues at least the enemy sound and the door's cancel");
        AssertEqual<SoundEffectId>(EnemySound.AcidSpit, queued[0],
            "the entry frame's enemy pass queues the spit first");
        AssertEqual<SoundEffectId>(EnemySoundStop.Any, queued[1],
            "the door's library-two cancel follows the enemy sound");
        Console.WriteLine("Door entry enemy sound: the entry frame's enemy sounds precede the door's cancel and DisableSounds.");
    }

    /// <summary>The Crateria Ki Hunter room, its green Ki Hunter placed wingless and Samus standing locked out, adopted by a game.</summary>
    private static (SuperMetroidGame Game, SuperMetroidRuntime Runtime) KiHunterRoom()
    {
        var random = new SharedRandom();
        var game = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps), renderGameplayFrames: false, random: random);
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true, random: random, audio: game.AudioForVerification);
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus(runtime.Room.LoadedScope);
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.CrateriaKihunter);
        var samus = runtime.Player.Samus;
        samus.PoseId = SamusPoseId.FacingRightNormalPose;
        samus.Position = samus.Position.WithWhole(new(64, 128));
        samus.Control.LockOutPlayer();
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        _ = KiHunterSpitStaging.PlaceWingless(runtime);
        game.AdoptRuntime(runtime);
        global::SuperMetroid.Core.PrivateState.Set(game.Session, "LastAudioRoomState", (global::SuperMetroid.Core.World.RoomStates.RoomStateSettings?)(runtime.Room.LoadedRoom.State));
        return (game, runtime);
    }

    /// <summary>Runs the focused enemy-before-door sound ordering check.</summary>
    [VerificationCommand("--door-entry-enemy-sound")]
    internal static int DoorEntryEnemySoundCommand()
    {
        VerifyDoorEntryEnemySound();
        return 0;
    }
}

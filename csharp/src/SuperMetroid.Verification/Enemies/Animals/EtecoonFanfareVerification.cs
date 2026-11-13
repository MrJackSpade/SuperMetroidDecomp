using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation;
namespace SuperMetroid.Verification.Enemies.Animals;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks when the three Etecoons request their fanfare after an elevator door transition.</summary>
internal static class EtecoonFanfareVerification
{
    /// <summary>Verifies the dormant transition gate, single post-fade sound request, and audible advancing melody.</summary>
    internal static void VerifyEtecoonFanfareAfterDoor()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        for (int index = 0; index < 3; index++)
        {
            var slot = enemies.SlotTable.Slots[index];
            slot.Bind(EnemyDefinitionId.Etecoon, slot.Definition);
            slot.Bind(slot.DefinitionId, RoomEnemyHeaders.Get(slot.DefinitionId));
            slot.Health = slot.Definition.Health;
            slot.Radius = new(slot.Definition.Radius.X, slot.Definition.Radius.Y);
            slot.Position = slot.Position.WithWhole(new((ushort)(0x25f + index * 16), 0xb98));
            slot.Contact = EnemyContact.IgnoresSamus;
            slot.Processing = EnemyProcessing.OffScreen;
            slot.Presence = EnemyPresence.None;
            slot.SetSpawnParameter2(EnemyDefinitionId.Etecoon, (ushort)index);
            EtecoonBehavior.InitializeEtecoon(slot);
        }
        var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Position = new(new(0x240, 0), new(0xb98, 0)), Health = 999, MaxHealth = 999 };
        RoomLevelData level = VerificationHelpersVerification.CreateEmptyRoom(new(48, 192));
        enemies.ElevatorBehavior.ElevatorDoorTransitionActive = true;
        for (int frame = 0; frame < 3; frame++)
        {
            enemies.StepStandaloneFrame(enemies.StandaloneFrame(level) with { Camera = new(0x200, 0xb00), TimeIsFrozen = false, Samus = samus });
            AssertEqual(0, enemies.Audio.SoundRequests.Count, "transition does not consume the Etecoon fanfare");
            foreach (var state in enemies.EtecoonStates.Where(state => state is not null))
                AssertTrue((state ?? throw new InvalidOperationException("Verification requires state.")).AwaitingSamus, "the transition keeps the Etecoon dormant");
        }
        enemies.ElevatorBehavior.ElevatorDoorTransitionActive = false;
        enemies.StepStandaloneFrame(enemies.StandaloneFrame(level) with { Camera = new(0x200, 0xb00), TimeIsFrozen = false, Samus = samus });
        AssertEqual(1, enemies.Audio.SoundRequests.Count, "trio publishes one fanfare after fade");
        var request = enemies.Audio.SoundRequests[0];
        AssertEqual(EnemySound.EtecoonTheme, request.SoundEffect,
            "native Etecoon fanfare identity");
        AssertEqual(15, request.Priority.Requests, "native wake request uses Max15");
        AssertEqual((ushort)256, (enemies.EtecoonStates[0] ?? throw new InvalidOperationException("Verification requires enemies.EtecoonStates[0].")).FunctionTimer.RawValue, "wake begins flexing timer");
        AssertTrue(!(enemies.EtecoonStates[0] ?? throw new InvalidOperationException("Verification requires enemies.EtecoonStates[0].")).AwaitingSamus,
            "Samus coming level wakes the Etecoon");

        var renderer = AudioSessionAccess.CreateSynthesizing(RepositoryInstallation.Installation.LoadAudio());
        var queue = AudioQueues.Standalone();
        renderer.RenderFrame(queue.AdvanceFrame(renderer.Acknowledgements));
        renderer.RenderFrame([new AudioRequest.ChangeMusicDataSet(MusicDataSet.GreenBrinstar)]);
        queue.QueueSound(request.SoundEffect, request.Priority);
        AssertTrue(queue.HasQueuedSounds, "post-fade fanfare reaches CPU queue");
        enemies.StepStandaloneFrame(enemies.StandaloneFrame(level) with { Camera = new(0x200, 0xb00), TimeIsFrozen = false, Samus = samus });
        AssertEqual(0, enemies.Audio.SoundRequests.Count, "next frame does not repeat fanfare");
        var channels = renderer.PlayerForVerification.soundChannels;
        var notes = new HashSet<int>();
        int peak = 0;
        for (int frame = 0; frame < 240; frame++)
        {
            var pcm = renderer.RenderFrame(queue.AdvanceFrame(renderer.Acknowledgements));
            foreach (short sample in pcm) peak = Math.Max(peak, Math.Abs((int)sample));
            if (channels[1][0].Disabled == 0) notes.Add(channels[1][0].Note);
        }
        AssertTrue(peak > 0 && notes.Count >= 4, "post-fade fanfare produces PCM and advances melody notes");
        Console.WriteLine($"Etecoon door fanfare: dormant gate, one Max15 request and audible melody verified (peak {peak}, notes {notes.Count}).");
    }

    /// <summary>Runs the Etecoon door fanfare regression from the command line.</summary>
    [VerificationCommand("--etecoon-door-fanfare")]
    internal static int EtecoonDoorFanfareCommand()
    {
        Suite(nameof(VerifyEtecoonFanfareAfterDoor), () => VerifyEtecoonFanfareAfterDoor());
        return 0;
    }
}

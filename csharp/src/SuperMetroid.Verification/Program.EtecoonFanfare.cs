using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyEtecoonFanfareAfterDoor()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        for (int index = 0; index < 3; index++)
        {
            var slot = enemies.Slots[index];
            slot.EnemyDefinitionPointer = 0xe5bf;
            slot.Definition = RoomEnemyDefinitionCatalog.Get(slot.EnemyDefinitionPointer);
            slot.AiBank = slot.Definition.Bank;
            slot.Health = slot.Definition.Health;
            slot.XRadius = slot.Definition.XRadius;
            slot.YRadius = slot.Definition.YRadius;
            slot.XPosition = (ushort)(0x25f + index * 16);
            slot.YPosition = 0xb98;
            slot.Properties = 0x0c00;
            slot.Parameter2 = (ushort)index;
            typeof(RoomEnemySystem).GetMethod("InitializeEtecoon", flags)!.Invoke(enemies, [slot]);
        }
        var samus = new SamusState { XPosition = 0x240, YPosition = 0xb98, Health = 999, MaxHealth = 999 };
        var level = new RoomLevelData(48, 192, new ushort[48 * 192], new byte[48 * 192], new ushort[48 * 192], new byte[8]);
        enemies.ElevatorDoorTransitionActive = true;
        for (int frame = 0; frame < 3; frame++)
        {
            enemies.StepFrame(0x200, 0xb00, false, samus, level: level);
            AssertEqual(0, enemies.SoundRequests.Count, "transition does not consume the Etecoon fanfare");
            foreach (var state in enemies.EtecoonStates.Where(state => state is not null))
                AssertEqual((ushort)0xffff, state!.FunctionTimer, "transition preserves dormant Etecoon timer");
        }
        enemies.ElevatorDoorTransitionActive = false;
        enemies.StepFrame(0x200, 0xb00, false, samus, level: level);
        AssertEqual(1, enemies.SoundRequests.Count, "trio publishes one fanfare after fade");
        var request = enemies.SoundRequests[0];
        AssertEqual(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x35), request.SoundEffect,
            "native Etecoon fanfare identity");
        AssertEqual(15, request.MaximumQueued, "native wake request uses Max15");
        AssertEqual((ushort)256, enemies.EtecoonStates[0]!.FunctionTimer, "wake begins flexing timer");

        var renderer = new CartridgeAudioRenderer(RepositoryInstallation.Installation.LoadAudio());
        var queue = new CartridgeAudioState();
        renderer.RenderFrame(queue.AdvanceFrame(bus, renderer.ReadAcknowledgements()));
        renderer.RenderFrame([CartridgeAudioCommand.Upload(AudioUploadAddresses.GreenBrinstar)]);
        queue.QueueSound(request.SoundEffect, request.MaximumQueued);
        AssertTrue(queue.HasQueuedSounds, "post-fade fanfare reaches CPU queue");
        enemies.StepFrame(0x200, 0xb00, false, samus, level: level);
        AssertEqual(0, enemies.SoundRequests.Count, "next frame does not repeat fanfare");
        var channels = (ManagedSpcSoundChannel[][])typeof(ManagedSpcPlayer)
            .GetField("soundChannels", flags)!.GetValue(renderer.Player)!;
        var notes = new HashSet<byte>();
        int peak = 0;
        for (int frame = 0; frame < 240; frame++)
        {
            var pcm = renderer.RenderFrame(queue.AdvanceFrame(bus, renderer.ReadAcknowledgements()));
            foreach (short sample in pcm) peak = Math.Max(peak, Math.Abs((int)sample));
            if (channels[1][0].Disabled == 0) notes.Add(channels[1][0].Note);
        }
        AssertTrue(peak > 0 && notes.Count >= 4, "post-fade fanfare produces PCM and advances melody notes");
        Console.WriteLine($"Etecoon door fanfare: dormant gate, one Max15 request and audible melody verified (peak {peak}, notes {notes.Count}).");
    }
}

using System.Reflection;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

/// <summary>Isolates an already-playing spin voice across a real retail item PLM/message handoff.</summary>
internal static class ItemMessageAudioAudit
{
    public static int Run(string rom, string audioDirectory)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var runtime = FlatFloorMovementFixture.Create(bus, water: false);
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.MorphBallRoom, 0, 0);
        runtime.Samus!.InputLocked = true;
        var game = new SuperMetroidGame(bus, gameOptions: null, renderGameplayFrames: false);
        typeof(SuperMetroidGame).GetField("runtime", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(game, runtime);
        typeof(SuperMetroidGame).GetProperty(nameof(game.GameState))!.SetValue(game, SuperMetroidGameState.MainGameplay);
        var initialFrame = game.Step(0); // Initialize the retail collectible's visible/touchable phase.
        // Only library one is rendered below. Echo writes on the other queues so
        // unrelated room requests cannot wedge their handshakes in this fixture.
        byte[] otherAcknowledgements = new byte[4];
        foreach (var command in initialFrame.AudioCommands)
            if (command.Kind == CartridgeAudioCommandKind.WritePort && command.Port is 2 or 3)
                otherAcknowledgements[command.Port] = command.Value;

        var assets = ExtractedAudioAssetCatalog.Load(audioDirectory);
        var player = new ManagedSpcPlayer();
        player.Upload(assets.GetUpload(AudioUploadAddresses.SpcEngine).ToArray());
        player.SetSampleBank(assets.GetSampleBank(AudioUploadAddresses.SpcEngine));
        var pcm = new short[1600];
        player.WritePort(1, SoundEffectLibrary1Sounds.SpinJump.Value);
        for (int frame = 0; frame < 30; frame++)
        {
            player.GenerateFrame(pcm);
            player.WritePort(1, 0);
        }
        if (!pcm.Any(sample => sample != 0)) throw new InvalidDataException("Spin voice was not audible before pickup.");
        runtime.Samus.Pose = SamusPoseIds.SpinJumpRightPose;
        runtime.Samus.InitializeAnimation(bus);
        var item = runtime.Plms.Collectibles.Single(item => item.Kind == InWorldCollectibleKind.MorphBall);
        if (!runtime.Plms.TryNotifyCollectibleTouch(item.BlockIndex))
            throw new InvalidDataException("Retail Morph Ball PLM rejected touch.");

        int cancellations = 0, audibleFrozenFrames = 0;
        int[] cancellationCounts = new int[SoundEffectLibraries.Count];
        for (int frame = 0; frame < 400; frame++)
        {
            game.SetAudioAcknowledgements(new(0, player.ReadPort(1), otherAcknowledgements[2], otherAcknowledgements[3]));
            var result = game.Step(0);
            foreach (var command in result.AudioCommands)
            {
                if (command.Kind == CartridgeAudioCommandKind.WritePort && command.Port is 2 or 3)
                    otherAcknowledgements[command.Port] = command.Value;
                if (command.Kind == CartridgeAudioCommandKind.WritePort && command.Port is >= 1 and <= 3 &&
                    command.Value == (command.Port switch
                    {
                        1 => SoundEffectLibrary1Sounds.CancelAll.Value,
                        2 => SoundEffectLibrary2Sounds.CancelAll.Value,
                        _ => SoundEffectLibrary3Sounds.CancelAll.Value,
                    })) cancellationCounts[command.Port - 1]++;
                // Isolate the sustained spin from music and unrelated room voices.
                // The cancellation must still originate in the real frontend handoff.
                if (command.Kind != CartridgeAudioCommandKind.WritePort || command.Port != 1) continue;
                if (command.Value == SoundEffectLibrary1Sounds.CancelAll.Value) cancellations++;
                player.WritePort(command.Port, command.Value);
            }
            player.GenerateFrame(pcm);
            if (frame == 0 && !runtime.MessageBox.IsActive)
                throw new InvalidDataException("Item did not enter the real message coroutine.");
            if (frame >= 30 && pcm.Any(sample => sample != 0)) audibleFrozenFrames++;
        }
        if (!runtime.MessageBox.IsActive)
            throw new InvalidDataException("Unacknowledged item message unexpectedly closed.");
        if (cancellations != 1 || cancellationCounts.Any(count => count != 1) || audibleFrozenFrames != 0)
            throw new InvalidDataException($"Item message left spin audio active: cancellations={cancellations}, audible frozen frames={audibleFrozenFrames}.");
        for (int frame = 0; frame < 80; frame++)
        {
            game.SetAudioAcknowledgements(new(0, player.ReadPort(1), 0, 0));
            var result = game.Step(frame == 0 ? (ushort)SnesButton.A : (ushort)0);
            foreach (var command in result.AudioCommands)
                if (command.Kind == CartridgeAudioCommandKind.WritePort && command.Port == 1)
                {
                    if (command.Value == SoundEffectLibrary1Sounds.CancelAll.Value)
                        throw new InvalidDataException("Message exit repeated the entry cancellation.");
                    player.WritePort(1, command.Value);
                }
            player.GenerateFrame(pcm);
        }
        if (runtime.MessageBox.IsActive) throw new InvalidDataException("Item message failed to acknowledge/close.");
        Console.WriteLine("Retail pickup/message handoff: one cancellation, audible spin before pickup, no spin PCM after release through the 400-frame message wait.");
        return 0;
    }
}

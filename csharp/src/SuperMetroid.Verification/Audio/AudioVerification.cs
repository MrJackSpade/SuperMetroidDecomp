using static SuperMetroid.Verification.Audio.AudioUploadCatalogVerification;
using static SuperMetroid.Verification.Rooms.Doors.DoorSoundDisableVerification;
using static SuperMetroid.Verification.Audio.GameplayAudioPublicationVerification;
using static SuperMetroid.Verification.Audio.SoundQueueReturnVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.Core;
namespace SuperMetroid.Verification.Audio;
using SuperMetroid.Verification.Harness;

/// <summary>Verifies cartridge music dispatch, delayed SFX handshakes and SPC upload traversal without an audio device.</summary>
internal static class AudioVerification
{
    /// <summary>
    /// Exercises bank $80's delayed music queue, LoROM upload stream traversal, and bank
    /// $82's real request/acknowledge/clear SFX handshake independently of an audio device.
    /// </summary>
    internal static void VerifyCartridgeAudioQueues()
    {
        Suite(nameof(VerifyDoorSoundDisableGuard), () => VerifyDoorSoundDisableGuard());
        var retail = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyAudioUploadCatalog), () => VerifyAudioUploadCatalog(retail));
        Suite(nameof(SoundAcceptanceClockVerification.VerifySoundAcceptanceClockMatchesEmulatedDriver),
            () => SoundAcceptanceClockVerification.VerifySoundAcceptanceClockMatchesEmulatedDriver());
        Suite(nameof(TickAccumulatorVerification.VerifyTickAccumulator),
            () => TickAccumulatorVerification.VerifyTickAccumulator());
        var invalidMusicData = new CartridgeAudioState();
        invalidMusicData.AdvanceFrame(default);
        invalidMusicData.QueueMusicDelayed8(MusicCommand.LoadData(0x01));
        // The first post-queue handler call admits the entry; eight more calls expire
        // its native delay because this fixture drained reset before queueing it.
        for (int delayFrame = 0; delayFrame < 8; delayFrame++)
            invalidMusicData.AdvanceFrame(default);
        AssertThrows<InvalidDataException>(
            () => invalidMusicData.AdvanceFrame(default),
            "queued non-identity music-data offset fails at dispatch");

        MusicCommand dataLoad = MusicCommand.LoadData(0x21);
        AssertEqual(0xff21, dataLoad.RawValue, "music data-load word");
        AssertTrue(dataLoad is MusicCommand.DataLoad, "music data-load classification");
        AssertTrue(dataLoad.UsesDataUploadPath, "music data-load takes native upload branch");
        AssertEqual(0x21, dataLoad.DataIndex, "music data-load table index");

        MusicCommand trackSelection = MusicCommand.SelectTrack(5);
        AssertEqual(5, trackSelection.RawValue, "music track-selection word");
        AssertTrue(trackSelection is MusicCommand.TrackSelection, "music track classification");
        AssertTrue(MusicCommand.Stop is MusicCommand.StopPlayback, "music stop classification");

        MusicCommand unknownCommand = MusicCommand.FromCartridge(0x1234);
        AssertTrue(unknownCommand is MusicCommand.Unresolved, "unknown music command classification");
        AssertEqual(0x1234, unknownCommand.RawValue, "unknown music command round trip");
        AssertEqual(8, MusicCommandDelay.FromDelayedYArgument(0).Frames,
            "music delay applies native minimum");
        AssertEqual(0x0168, MusicCommandDelay.FromDelayedYArgument(0x0168).Frames,
            "music delay preserves longer countdown");
        AssertThrows<ArgumentOutOfRangeException>(
            () => MusicCommand.SelectTrack(0),
            "zero track must use explicit stop command");
        AssertThrows<InvalidDataException>(
            () => MusicCommandDelay.FromEffectiveFrames(7),
            "invalid effective music delay fails loudly");
        AssertTrue(
            MetadataShape.DeclaresMethod(typeof(CartridgeAudioState), nameof(CartridgeAudioState.QueueMusicDelayed8), [typeof(MusicCommand)]),
            "music queue accepts typed commands");
        AssertTrue(
            !MetadataShape.DeclaresMethod(typeof(CartridgeAudioState), nameof(CartridgeAudioState.QueueMusicDelayed8), [typeof(ushort)]),
            "music queue exposes no raw-word overload");

        byte[] rom = new byte[0x10_0000];

        // Prepare a second fixture before constructing the mapper, which intentionally
        // takes ownership of a stable ROM copy.
        WriteAudioRomByte(rom, 0x90fffb, 0x02); // record length = 2
        WriteAudioRomByte(rom, 0x90fffc, 0x00);
        WriteAudioRomByte(rom, 0x90fffd, 0x00); // SPC target = $2000
        WriteAudioRomByte(rom, 0x90fffe, 0x20);
        WriteAudioRomByte(rom, 0x90ffff, 0xaa);
        WriteAudioRomByte(rom, 0x918000, 0xbb);
        WriteAudioRomByte(rom, 0x918001, 0x00); // terminator
        WriteAudioRomByte(rom, 0x918002, 0x00);
        var bus = new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom);
        Suite(nameof(VerifySoundQueueAccumulator), () => VerifySoundQueueAccumulator());
        Suite(nameof(VerifyGameplayAudioPublication), () => VerifyGameplayAudioPublication());

        var audio = new CartridgeAudioState();
        audio.QueueMusicDelayed8(MusicCommand.LoadData(0x03));
        audio.QueueMusicDelayed8(MusicCommand.SelectTrack(5));

        IReadOnlyList<AudioRequest> reset =
            audio.AdvanceFrame(default);
        AssertEqual(3, reset.Count, "audio reset command count");
        AssertEqual(
            new AudioRequest.ChangeMusicDataSet(AudioRomData.Assets.InitialAudioBank),
            reset[0],
            "initial SPC bank upload");
        AssertEqual(new AudioRequest.StopMusic(), reset[1], "reset music port");
        AssertEqual(AudioRequest.ForSound(SoundEffectLibrary.Library2, 0), reset[2], "reset SFX2 port");

        for (int delayFrame = 0; delayFrame < 7; delayFrame++)
            AssertEqual(0, audio.AdvanceFrame(default).Count, $"music upload delay {delayFrame}");
        IReadOnlyList<AudioRequest> upload = audio.AdvanceFrame(default);
        AssertEqual(4, upload.Count, "music upload plus SFX-downtime command count");
        AssertEqual(
            new AudioRequest.ChangeMusicDataSet(AudioBank.TitleSequence),
            upload[0],
            "compiled music data routing ignores absent ROM pointer bytes");

        // Each music operation starts eight SFX-downtime frames. Those frames deliberately
        // write zero to all three ports, so filter port-zero music commands separately.
        // `$80:8FA2` returns after a data upload without loading the next entry; the
        // following call underflows the zero timer, fetches the track and its eight-frame
        // delay, so the track write lands on the ninth call after the upload.
        int trackCall = 0;
        for (int call = 1; call <= 9; call++)
        {
            foreach (AudioRequest command in audio.AdvanceFrame(default))
            {
                if (command == AudioRequest.ForTrack(MusicTrack.NumberedOrNone(5)))
                    trackCall = call;
            }
        }
        AssertEqual(9, trackCall, "delayed track write follows the data upload's separate fetch call");

        // Every permanent item uses the same $84:8BDD -> $82:E118 sequence. Seed a stale
        // request to prove queue clearing, then observe track two, the 360-frame fanfare
        // hold/silence, and restoration of the room track that was live at acquisition.
        audio.QueueMusicDelayed8(MusicCommand.SelectTrack(7));
        audio.QueuePermanentItemFanfare();
        var permanentItemPortWrites = new List<int>();
        for (int frame = 0; frame < 384; frame++)
        {
            foreach (AudioRequest command in audio.AdvanceFrame(default))
            {
                if (command is AudioRequest.PlayMusicTrack play)
                    permanentItemPortWrites.Add(play.Track.Number);
                else if (command is AudioRequest.StopMusic)
                    permanentItemPortWrites.Add(0);
            }
        }
        AssertSequenceEqual(
            new[] { 2, 0, 5 },
            permanentItemPortWrites,
            "permanent-item fanfare and room-track restoration");

        // Ceres post-battle rooms use `(0, 0)` specifically to retain track seven.
        // `$82:E071/$E0D5` return on each zero field; zero is not a stop command.
        var inheritedRoomMusic = new CartridgeAudioState();
        inheritedRoomMusic.AdvanceFrame(default);
        inheritedRoomMusic.QueueMusicDelayed8(MusicCommand.SelectTrack(7));
        // The first handler call copies the newly queued entry into the active timer;
        // the following eight calls count down the native delay and emit the port write.
        for (int frame = 0; frame < 9; frame++)
            inheritedRoomMusic.AdvanceFrame(default);
        AssertEqual(7, inheritedRoomMusic.MusicTrackIndex,
            "escape track is live before zero-music room transition");
        inheritedRoomMusic.QueueRoomMusic(dataIndex: 0, trackIndex: 0);
        AssertTrue(!inheritedRoomMusic.HasQueuedMusic,
            "zero room-music fields preserve the inherited Ceres escape track");
        AssertEqual(7, inheritedRoomMusic.MusicTrackIndex,
            "zero room track does not silence escape music");

        // RoomState_CeresRidley_0 ($8F:E0C7) contains music ($24,$80).
        // $82:E0D5 queues nonzero bytes verbatim; $80:8F0C masks the low seven
        // bits only at dispatch. Thus $80 means an explicit stop, unlike room byte
        // zero (inherit). Cover every possible room byte, not only this one room.
        for (int roomTrack = 0; roomTrack <= byte.MaxValue; roomTrack++)
        {
            var roomMusic = new CartridgeAudioState();
            roomMusic.AdvanceFrame(default);
            roomMusic.QueueRoomMusic(dataIndex: 0, trackIndex: (byte)roomTrack);
            var portWrites = new List<AudioRequest>();
            for (int frame = 0; frame < 9; frame++)
                portWrites.AddRange(roomMusic.AdvanceFrame(default).Where(command =>
                    command is AudioRequest.PlayMusicTrack or AudioRequest.StopMusic));
            if (roomTrack == 0)
                AssertEqual(0, portWrites.Count, "room byte zero inherits rather than queues stop");
            else
                AssertSequenceEqual(new[] { AudioRequest.ForTrack(MusicTrack.NumberedOrNone((byte)(roomTrack & AudioRomData.MusicWireFormat.TrackMask))) }, portWrites,
                    $"native room track ${roomTrack:X2} dispatches its low seven bits");
        }

        // The Zebes approach interstitial and Landing Site both call their song "track
        // five", but use different SPC data banks. `$82:E0E6-$E104` compares the packed
        // bank/track pair, so the latter track must be resent after bank six uploads.
        var postCeresLandingMusic = new CartridgeAudioState();
        postCeresLandingMusic.AdvanceFrame(default);
        postCeresLandingMusic.QueueMusicDelayed8(MusicCommand.SelectTrack(5));
        for (int frame = 0; frame < 9; frame++)
            postCeresLandingMusic.AdvanceFrame(default);
        postCeresLandingMusic.QueueRoomMusic(dataIndex: 6, trackIndex: 5);
        var postCeresMusicCommands = new List<AudioRequest>();
        for (int frame = 0; frame < 40; frame++)
            postCeresMusicCommands.AddRange(postCeresLandingMusic.AdvanceFrame(default));
        AssertTrue(
            postCeresMusicCommands.Any(command =>
                command is AudioRequest.ChangeMusicDataSet),
            "Landing Site changes the post-Ceres SPC data bank");
        AssertTrue(
            postCeresMusicCommands.Any(command =>
                command == AudioRequest.ForTrack(MusicTrack.NumberedOrNone(5))),
            "Landing Site restarts same-numbered track after SPC data-bank change");

        var sfx = new CartridgeAudioState();
        sfx.AdvanceFrame(default); // consume reset upload/port writes
        sfx.QueueSound(SoundEffectLibrary2Sounds.DoorOpening, maximumQueued: 6);
        IReadOnlyList<AudioRequest> request = sfx.AdvanceFrame(default);
        AssertEqual(AudioRequest.ForSound(SoundEffectLibrary.Library2, 0x57), request.Single(), "SFX request write");
        AssertEqual(
            0,
            sfx.AdvanceFrame(new SoundAcknowledgements(0, 0x57, 0)).Count,
            "SFX acknowledgement enters two-frame clear delay");
        AssertEqual(0, sfx.AdvanceFrame(default).Count, "SFX clear delay frame one");
        AssertEqual(
            AudioRequest.ForSound(SoundEffectLibrary.Library2, 0),
            sfx.AdvanceFrame(default).Single(),
            "SFX request clear");

        var cancellation = new CartridgeAudioState();
        cancellation.AdvanceFrame(default);
        cancellation.QueueCancelSoundEffects();
        IReadOnlyList<AudioRequest> cancelWrites =
            cancellation.AdvanceFrame(default);
        AssertEqual(AudioRomData.Queues.SoundLibraryCount, cancelWrites.Count,
            "sound cancellation writes every native SFX library");
        AssertTrue(cancelWrites.Contains(AudioRequest.ForSound(SoundEffectLibrary.Library1, SoundEffectLibrary1Sounds.CancelAll.Value)),
            "sound cancellation queues library one sentinel");

        // Library numbers are an exclusive cartridge domain: only queues one through
        // three exist. A forged enum value must fail at the public queue boundary rather
        // than indexing some unrelated host collection or silently selecting a queue.
        AssertEqual(
            SoundEffectLibrary.Library3,
            SoundEffectLibraries.FromCartridge(3, "audio verifier"),
            "cartridge SFX library decoding");
        AssertThrows<InvalidDataException>(
            () => SoundEffectLibraries.FromCartridge(0, "audio verifier"),
            "zero is not a cartridge SFX library");
        AssertThrows<ArgumentOutOfRangeException>(
            () => sfx.QueueSound(new SoundEffectId((SoundEffectLibrary)4, 1), maximumQueued: 1),
            "forged SFX library is rejected at queue boundary");

        // The same relative number in two libraries denotes two different sounds. The
        // paired value must preserve that distinction, retain unknown cartridge IDs, and
        // be the only shape accepted by QueueSound—there is no reorderable library/ID API.
        SoundEffectId libraryOneUnknown =
            SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 0xfe);
        SoundEffectId libraryTwoUnknown =
            SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0xfe);
        AssertTrue(libraryOneUnknown != libraryTwoUnknown,
            "library-relative equal numbers remain distinct sound identities");
        AssertEqual(0xfe, libraryTwoUnknown.Value,
            "unknown cartridge sound number remains losslessly representable");
        AssertEqual(SoundEffectLibrary.Library2, libraryTwoUnknown.Library,
            "unknown cartridge sound retains its library");
        AssertTrue(
            MetadataShape.DeclaresMethod(typeof(CartridgeAudioState), nameof(CartridgeAudioState.QueueSound), [typeof(SoundEffectId), typeof(byte)]),
            "QueueSound accepts one indivisible sound identity");
        AssertTrue(
            !MetadataShape.DeclaresMethod(typeof(CartridgeAudioState), nameof(CartridgeAudioState.QueueSound), [typeof(SoundEffectLibrary), typeof(byte), typeof(byte)]),
            "QueueSound exposes no reversible library-and-ID argument list");
        AssertThrows<InvalidDataException>(
            () => SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 0x0100),
            "word-sized sound values outside the SPC port fail loudly");
        AssertThrows<ArgumentOutOfRangeException>(
            () => sfx.QueueSound(default, maximumQueued: 1),
            "default sound identity cannot silently select a queue");
        AssertEqual(SoundEffectLibrary1Sounds.MenuCursor,
            SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 0x37),
            "named library-one menu catalog retains cartridge identity");
        AssertEqual(SoundEffectLibrary2Sounds.DoorOpening,
            SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x57),
            "named library-two door catalog retains cartridge identity");

        // Verify the upload reader follows contiguous ROM pointer arithmetic across a
        // physical LoROM bank boundary: $90:FFFF continues at $91:8000, not $91:0000.
        AssertSequenceEqual(
            new byte[] { 2, 0, 0, 0x20, 0xaa, 0xbb, 0, 0 },
            SpcUploadStreamReader.Read(bus, 0x90fffb),
            "cross-bank SPC upload stream");
        AssertThrows<ArgumentOutOfRangeException>(
            () => SpcUploadStreamReader.Read(bus, 0x907fff),
            "SPC upload rejects the unmapped LoROM lower window");
        AssertThrows<ArgumentOutOfRangeException>(
            () => SpcUploadStreamReader.Read(
                bus, AudioRomData.SpcUpload.MaximumSnesAddress + 1),
            "SPC upload rejects addresses wider than 24 bits");

        Console.WriteLine("  Audio: typed/lossless music commands and delays, 25 compiled upload routes, invalid-route rejection, inherited Ceres track, post-Ceres bank/track restart, item fanfare, paired SFX identities/catalogs, handshake, and LoROM stream agree.");
    }

    /// <summary>Seeds a synthetic upload byte at its mapped LoROM offset for cross-bank reader checks.</summary>
    private static void WriteAudioRomByte(byte[] rom, int snesAddress, byte value) =>
        rom[SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace.ToRomOffset(snesAddress)] = value;

    /// <summary>Runs the cartridge audio queue and upload-stream verification command.</summary>
    [VerificationCommand("--audio-queues")]
    internal static int AudioQueuesCommand()
    {
        Suite(nameof(VerifyCartridgeAudioQueues), () => VerifyCartridgeAudioQueues());
        return 0;
    }
}

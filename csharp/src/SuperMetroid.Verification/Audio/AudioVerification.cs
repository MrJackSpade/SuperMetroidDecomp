using static SuperMetroid.Verification.Audio.AudioUploadCatalogVerification;
using static SuperMetroid.Verification.Rooms.Doors.DoorSoundDisableVerification;
using static SuperMetroid.Verification.Audio.GameplayAudioPublicationVerification;
using static SuperMetroid.Verification.Audio.SoundQueueReturnVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.AssetExtraction.Rooms;
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
        // #1410: a music data set is a name, so a data index the retail data never holds cannot reach the queue; the
        // room-state import rejects it where the cartridge byte is decoded. This replaces the queued raw $FF01 word
        // that failed at dispatch.
        AssertThrows<InvalidDataException>(
            () => RoomMusicCodec.Decode(0x01, 5),
            "a music data index that names no data set fails at room-state import");

        // #1410: the requests are asserted by kind and identity instead of by their cartridge queue words.
        AssertEqual<MusicRequest>(new MusicRequest.ChangeDataSet(MusicDataSet.MotherBrain), MusicRequest.LoadDataSet(MusicDataSet.MotherBrain),
            "a data set change names its data set");
        AssertEqual<MusicRequest>(new MusicRequest.PlayTrack(new MusicTrack(5)), MusicRequest.SelectTrack(5),
            "a track request names its track");
        AssertTrue(MusicRequest.Stop is MusicRequest.StopMusic, "the stop request");
        AssertEqual(MusicRequest.Stop, MusicRequest.SelectTrackOrStop(null), "no track stops the music");
        AssertEqual(8, MusicQueueDelay.AtLeastShortest(0).Updates,
            "music delay applies native minimum");
        AssertEqual(0x0168, MusicQueueDelay.AtLeastShortest(0x0168).Updates,
            "music delay preserves longer countdown");
        AssertThrows<ArgumentOutOfRangeException>(
            () => MusicRequest.SelectTrack(0),
            "zero track must use explicit stop command");
        AssertThrows<InvalidDataException>(
            () => MusicQueueDelay.FromEffectiveUpdates(7),
            "invalid effective music delay fails loudly");
        AssertTrue(
            MetadataShape.DeclaresMethod(typeof(AudioQueues), nameof(AudioQueues.QueueMusicDelayed8), [typeof(MusicRequest)]),
            "music queue accepts typed commands");
        AssertTrue(
            !MetadataShape.DeclaresMethod(typeof(AudioQueues), nameof(AudioQueues.QueueMusicDelayed8), [typeof(ushort)]),
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

        var audio = new AudioQueues();
        audio.QueueMusicDelayed8(MusicRequest.LoadDataSet(MusicDataSet.TitleSequence));
        audio.QueueMusicDelayed8(MusicRequest.SelectTrack(5));

        IReadOnlyList<AudioRequest> reset =
            audio.AdvanceFrame(default);
        AssertEqual(3, reset.Count, "audio reset command count");
        AssertEqual(
            new AudioRequest.ChangeMusicDataSet(null),
            reset[0],
            "power-on loads only the shared sounds");
        AssertEqual(new AudioRequest.StopMusic(), reset[1], "reset music port");
        AssertEqual(new AudioRequest.ClearSoundRequest(SoundEffectLibrary.Library2), reset[2], "reset withdraws library two's request");

        for (int delayFrame = 0; delayFrame < 7; delayFrame++)
            AssertEqual(0, audio.AdvanceFrame(default).Count, $"music upload delay {delayFrame}");
        IReadOnlyList<AudioRequest> upload = audio.AdvanceFrame(default);
        AssertEqual(4, upload.Count, "music upload plus SFX-downtime command count");
        AssertEqual(
            new AudioRequest.ChangeMusicDataSet(MusicDataSet.TitleSequence),
            upload[0],
            "the queued data set change dispatches its data set");

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
        audio.QueueMusicDelayed8(MusicRequest.SelectTrack(7));
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
        var inheritedRoomMusic = new AudioQueues();
        inheritedRoomMusic.AdvanceFrame(default);
        inheritedRoomMusic.QueueMusicDelayed8(MusicRequest.SelectTrack(7));
        // The first handler call copies the newly queued entry into the active timer;
        // the following eight calls count down the native delay and emit the port write.
        for (int frame = 0; frame < 9; frame++)
            inheritedRoomMusic.AdvanceFrame(default);
        AssertEqual(new MusicTrack(7), inheritedRoomMusic.PlayingTrack,
            "escape track is live before zero-music room transition");
        inheritedRoomMusic.QueueRoomMusic(RoomMusic.Inherited);
        AssertTrue(!inheritedRoomMusic.HasQueuedMusic,
            "inherited room music preserves the Ceres escape track");
        AssertEqual(new MusicTrack(7), inheritedRoomMusic.PlayingTrack,
            "an inherited room track does not silence escape music");

        // RoomState_CeresRidley_0 ($8F:E0C7) contains music ($24,$80). $82:E0D5 queues a nonzero track byte verbatim and
        // the music handler keeps its low seven bits, so $80 is an explicit stop, unlike track zero (inherit). #1410: the
        // room-state import decodes the byte once into the room's track choice; every byte the retail data can hold is
        // decoded and queued here, and every byte above $80, which no retail room state holds, fails at import.
        for (int roomTrack = 0; roomTrack <= byte.MaxValue; roomTrack++)
        {
            if (roomTrack > 0x80)
            {
                AssertThrows<InvalidDataException>(() => RoomMusicCodec.Decode(0, (byte)roomTrack),
                    $"room track ${roomTrack:X2} is not one retail room states hold");
                continue;
            }
            var roomMusic = new AudioQueues();
            roomMusic.AdvanceFrame(default);
            roomMusic.QueueRoomMusic(RoomMusicCodec.Decode(0, (byte)roomTrack));
            var portWrites = new List<AudioRequest>();
            for (int frame = 0; frame < 9; frame++)
                portWrites.AddRange(roomMusic.AdvanceFrame(default).Where(command =>
                    command is AudioRequest.PlayMusicTrack or AudioRequest.StopMusic));
            if (roomTrack == 0)
                AssertEqual(0, portWrites.Count, "room byte zero inherits rather than queues stop");
            else
                AssertSequenceEqual(new[] { AudioRequest.ForTrack(MusicTrack.NumberedOrNone(roomTrack & 0x7f)) }, portWrites,
                    $"native room track ${roomTrack:X2} dispatches its low seven bits");
        }

        // The Zebes approach interstitial and Landing Site both call their song "track
        // five", but use different SPC data banks. `$82:E0E6-$E104` compares the packed
        // bank/track pair, so the latter track must be resent after bank six uploads.
        var postCeresLandingMusic = new AudioQueues();
        postCeresLandingMusic.AdvanceFrame(default);
        postCeresLandingMusic.QueueMusicDelayed8(MusicRequest.SelectTrack(5));
        for (int frame = 0; frame < 9; frame++)
            postCeresLandingMusic.AdvanceFrame(default);
        postCeresLandingMusic.QueueRoomMusic(new RoomMusic(MusicDataSet.EmptyCrateria, RoomMusicTrack.Plays(5)));
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

        var sfx = new AudioQueues();
        sfx.AdvanceFrame(default); // consume reset upload/port writes
        sfx.QueueSound(Library2Sound.DudShotOnDoor, maximumQueued: SoundQueueLimit.Max6);
        IReadOnlyList<AudioRequest> request = sfx.AdvanceFrame(default);
        AssertEqual(new AudioRequest.RequestSound(Library2Sound.DudShotOnDoor), request.Single(), "SFX request write");
        AssertEqual(
            0,
            sfx.AdvanceFrame(SoundAcknowledgements<SoundEffectId>.Of(null, Library2Sound.DudShotOnDoor, null)).Count,
            "SFX acknowledgement enters two-frame clear delay");
        AssertEqual(0, sfx.AdvanceFrame(default).Count, "SFX clear delay frame one");
        AssertEqual(
            new AudioRequest.ClearSoundRequest(SoundEffectLibrary.Library2),
            sfx.AdvanceFrame(default).Single(),
            "SFX request clear");

        var cancellation = new AudioQueues();
        cancellation.AdvanceFrame(default);
        cancellation.QueueCancelSoundEffects();
        IReadOnlyList<AudioRequest> cancelWrites =
            cancellation.AdvanceFrame(default);
        AssertEqual(AudioQueueDefinitions.SoundLibraryCount, cancelWrites.Count,
            "sound cancellation writes every native SFX library");
        AssertTrue(cancelWrites.Contains(new AudioRequest.RequestSound(Library1Sound.Silence)),
            "sound cancellation queues library one's silence");

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
        // The same request number in two libraries denotes two different sounds: a sound's
        // identity carries its library, and the import decodes each library's numbering on
        // its own. A number no retail sound has fails loudly at import instead of becoming
        // an identity; the default identity, which names no sound, cannot select a queue.
        // (This used to assert that an unknown number stayed representable; sounds no longer
        // carry numbers, so it asserts the decode boundary instead.)
        AssertTrue(SoundEffectCommands.Decode(SoundEffectLibrary.Library1, 0x02) != SoundEffectCommands.Decode(SoundEffectLibrary.Library2, 0x02),
            "library-relative equal numbers decode to distinct sounds");
        AssertEqual<SoundEffectId>(Library1Sound.Silence, SoundEffectCommands.Decode(SoundEffectLibrary.Library1, 0x02),
            "library one's request two is its silence");
        AssertEqual<SoundEffectId>(Library2Sound.SmallEnergyPickup, SoundEffectCommands.Decode(SoundEffectLibrary.Library2, 0x01),
            "library two's request one is the small energy pickup");
        AssertThrows<InvalidDataException>(
            () => SoundEffectCommands.Decode(SoundEffectLibrary.Library1, 0xfe),
            "a request number no library-one sound has fails at import");
        AssertThrows<InvalidDataException>(
            () => SoundEffectCommands.DecodeOptional(SoundEffectLibrary.Library1, 0x0100),
            "word-sized sound values outside a request byte fail loudly");
        AssertTrue(
            MetadataShape.DeclaresMethod(typeof(AudioQueues), nameof(AudioQueues.QueueSound), [typeof(SoundEffectId), typeof(SoundQueueLimit), typeof(bool)]),
            "QueueSound accepts one indivisible sound identity");
        AssertTrue(
            !MetadataShape.DeclaresMethod(typeof(AudioQueues), nameof(AudioQueues.QueueSound), [typeof(SoundEffectLibrary), typeof(int), typeof(int)]),
            "QueueSound exposes no reversible library-and-ID argument list");
        AssertThrows<ArgumentOutOfRangeException>(
            () => sfx.QueueSound(default, maximumQueued: SoundQueueLimit.Max1),
            "default sound identity cannot silently select a queue");
        AssertEqual<SoundEffectId>(Library1Sound.MenuCursor,
            SoundEffectCommands.Decode(SoundEffectLibrary.Library1, 0x37),
            "the import decodes library one's request $37 to the menu cursor");
        AssertEqual<SoundEffectId>(Library2Sound.DudShotOnDoor,
            SoundEffectCommands.Decode(SoundEffectLibrary.Library2, 0x57),
            "the import decodes library two's request $57 to the door's dud-shot sound");
        AssertEqual(0x57, SoundEffectCommands.Command(Library2Sound.DudShotOnDoor),
            "the door's dud-shot sound encodes back to library two's request $57");

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

        Console.WriteLine("  Audio: music requests and delays, 25 compiled upload routes, room music import rejection, inherited Ceres track, post-Ceres bank/track restart, item fanfare, paired SFX identities/catalogs, handshake, and LoROM stream agree.");
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

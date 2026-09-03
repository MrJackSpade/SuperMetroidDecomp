using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Exercises bank $80's delayed music queue, LoROM upload stream traversal, and bank
    /// $82's real request/acknowledge/clear SFX handshake independently of an audio device.
    /// </summary>
    private static void VerifyCartridgeAudioQueues()
    {
        MusicCommand dataLoad = MusicCommand.LoadData(0x21);
        AssertEqual((ushort)0xff21, dataLoad.RawValue, "music data-load word");
        AssertEqual(MusicCommandKind.LoadData, dataLoad.Kind, "music data-load classification");
        AssertTrue(dataLoad.UsesDataUploadPath, "music data-load takes native upload branch");
        AssertEqual((byte)0x21, dataLoad.DataIndex, "music data-load table index");

        MusicCommand trackSelection = MusicCommand.SelectTrack(5);
        AssertEqual((ushort)5, trackSelection.RawValue, "music track-selection word");
        AssertEqual(MusicCommandKind.SelectTrack, trackSelection.Kind, "music track classification");
        AssertEqual(MusicCommandKind.Stop, MusicCommand.Stop.Kind, "music stop classification");

        MusicCommand unknownCommand = MusicCommand.FromCartridge(0x1234);
        AssertEqual(MusicCommandKind.Unknown, unknownCommand.Kind, "unknown music command classification");
        AssertEqual((ushort)0x1234, unknownCommand.RawValue, "unknown music command round trip");
        AssertEqual((ushort)8, MusicCommandDelay.FromDelayedYArgument(0).Frames,
            "music delay applies native minimum");
        AssertEqual((ushort)0x0168, MusicCommandDelay.FromDelayedYArgument(0x0168).Frames,
            "music delay preserves longer countdown");
        AssertThrows<ArgumentOutOfRangeException>(
            () => MusicCommand.SelectTrack(0),
            "zero track must use explicit stop command");
        AssertThrows<InvalidDataException>(
            () => MusicCommandDelay.FromEffectiveFrames(7),
            "invalid effective music delay fails loudly");
        AssertTrue(
            typeof(CartridgeAudioState).GetMethod(
                nameof(CartridgeAudioState.QueueMusicDelayed8),
                [typeof(MusicCommand)]) is not null,
            "music queue accepts typed commands");
        AssertTrue(
            typeof(CartridgeAudioState).GetMethod(
                nameof(CartridgeAudioState.QueueMusicDelayed8),
                [typeof(ushort)]) is null,
            "music queue exposes no raw-word overload");

        byte[] rom = new byte[0x10_0000];

        // Music data command $FF03 indexes the 24-bit table by its low byte, not by a
        // multiplied host index. Point that exact odd-byte table entry at $90:8000.
        WriteAudioRomByte(rom, 0x8fe7e4, 0x00);
        WriteAudioRomByte(rom, 0x8fe7e5, 0x80);
        WriteAudioRomByte(rom, 0x8fe7e6, 0x90);

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
        var bus = new SuperMetroidAddressSpace(rom);

        var audio = new CartridgeAudioState();
        audio.QueueMusicDelayed8(MusicCommand.LoadData(0x03));
        audio.QueueMusicDelayed8(MusicCommand.SelectTrack(5));

        IReadOnlyList<CartridgeAudioCommand> reset =
            audio.AdvanceFrame(bus, default);
        AssertEqual(3, reset.Count, "audio reset command count");
        AssertEqual(CartridgeAudioCommand.Upload(0xcf8000), reset[0], "initial SPC bank upload");
        AssertEqual(CartridgeAudioCommand.WritePort(0, 0), reset[1], "reset music port");
        AssertEqual(CartridgeAudioCommand.WritePort(2, 0), reset[2], "reset SFX2 port");

        for (int delayFrame = 0; delayFrame < 7; delayFrame++)
            AssertEqual(0, audio.AdvanceFrame(bus, default).Count, $"music upload delay {delayFrame}");
        IReadOnlyList<CartridgeAudioCommand> upload = audio.AdvanceFrame(bus, default);
        AssertEqual(4, upload.Count, "music upload plus SFX-downtime command count");
        AssertEqual(CartridgeAudioCommand.Upload(0x908000), upload[0], "music data table lookup");

        // Each music operation starts eight SFX-downtime frames. Those frames deliberately
        // write zero to all three ports, so filter port-zero music commands separately.
        CartridgeAudioCommand? track = null;
        for (int frame = 0; frame < 8; frame++)
        {
            foreach (CartridgeAudioCommand command in audio.AdvanceFrame(bus, default))
            {
                if (command == CartridgeAudioCommand.WritePort(0, 5))
                    track = command;
            }
        }
        AssertEqual(CartridgeAudioCommand.WritePort(0, 5), track!.Value, "delayed track write");

        // Every permanent item uses the same $84:8BDD -> $82:E118 sequence. Seed a stale
        // request to prove queue clearing, then observe track two, the 360-frame fanfare
        // hold/silence, and restoration of the room track that was live at acquisition.
        audio.QueueMusicDelayed8(MusicCommand.SelectTrack(7));
        audio.QueuePermanentItemFanfare();
        var permanentItemPortWrites = new List<byte>();
        for (int frame = 0; frame < 384; frame++)
        {
            foreach (CartridgeAudioCommand command in audio.AdvanceFrame(bus, default))
            {
                if (command.Kind == CartridgeAudioCommandKind.WritePort && command.Port == 0)
                    permanentItemPortWrites.Add(command.Value);
            }
        }
        AssertSequenceEqual(
            new byte[] { 2, 0, 5 },
            permanentItemPortWrites,
            "permanent-item fanfare and room-track restoration");

        // Ceres post-battle rooms use `(0, 0)` specifically to retain track seven.
        // `$82:E071/$E0D5` return on each zero field; zero is not a stop command.
        var inheritedRoomMusic = new CartridgeAudioState();
        inheritedRoomMusic.AdvanceFrame(bus, default);
        inheritedRoomMusic.QueueMusicDelayed8(MusicCommand.SelectTrack(7));
        // The first handler call copies the newly queued entry into the active timer;
        // the following eight calls count down the native delay and emit the port write.
        for (int frame = 0; frame < 9; frame++)
            inheritedRoomMusic.AdvanceFrame(bus, default);
        AssertEqual((byte)7, inheritedRoomMusic.MusicTrackIndex,
            "escape track is live before zero-music room transition");
        inheritedRoomMusic.QueueRoomMusic(dataIndex: 0, trackIndex: 0);
        AssertTrue(!inheritedRoomMusic.HasQueuedMusic,
            "zero room-music fields preserve the inherited Ceres escape track");
        AssertEqual((byte)7, inheritedRoomMusic.MusicTrackIndex,
            "zero room track does not silence escape music");

        // The Zebes approach interstitial and Landing Site both call their song "track
        // five", but use different SPC data banks. `$82:E0E6-$E104` compares the packed
        // bank/track pair, so the latter track must be resent after bank six uploads.
        var postCeresLandingMusic = new CartridgeAudioState();
        postCeresLandingMusic.AdvanceFrame(bus, default);
        postCeresLandingMusic.QueueMusicDelayed8(MusicCommand.SelectTrack(5));
        for (int frame = 0; frame < 9; frame++)
            postCeresLandingMusic.AdvanceFrame(bus, default);
        postCeresLandingMusic.QueueRoomMusic(dataIndex: 6, trackIndex: 5);
        var postCeresMusicCommands = new List<CartridgeAudioCommand>();
        for (int frame = 0; frame < 40; frame++)
            postCeresMusicCommands.AddRange(postCeresLandingMusic.AdvanceFrame(bus, default));
        AssertTrue(
            postCeresMusicCommands.Any(command =>
                command.Kind == CartridgeAudioCommandKind.Upload),
            "Landing Site changes the post-Ceres SPC data bank");
        AssertTrue(
            postCeresMusicCommands.Any(command =>
                command == CartridgeAudioCommand.WritePort(0, 5)),
            "Landing Site restarts same-numbered track after SPC data-bank change");

        var sfx = new CartridgeAudioState();
        sfx.AdvanceFrame(bus, default); // consume reset upload/port writes
        sfx.QueueSound(SoundEffectLibrary2Sounds.DoorOpening, maximumQueued: 6);
        IReadOnlyList<CartridgeAudioCommand> request = sfx.AdvanceFrame(bus, default);
        AssertEqual(CartridgeAudioCommand.WritePort(2, 0x57), request.Single(), "SFX request write");
        AssertEqual(
            0,
            sfx.AdvanceFrame(bus, new CartridgeAudioAcknowledgements(0, 0, 0x57, 0)).Count,
            "SFX acknowledgement enters two-frame clear delay");
        AssertEqual(0, sfx.AdvanceFrame(bus, default).Count, "SFX clear delay frame one");
        AssertEqual(
            CartridgeAudioCommand.WritePort(2, 0),
            sfx.AdvanceFrame(bus, default).Single(),
            "SFX request clear");

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
            typeof(CartridgeAudioState).GetMethod(
                nameof(CartridgeAudioState.QueueSound),
                [typeof(SoundEffectId), typeof(byte)]) is not null,
            "QueueSound accepts one indivisible sound identity");
        AssertTrue(
            typeof(CartridgeAudioState).GetMethod(
                nameof(CartridgeAudioState.QueueSound),
                [typeof(SoundEffectLibrary), typeof(byte), typeof(byte)]) is null,
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

        Console.WriteLine("  Audio: typed/lossless music commands and delays, inherited Ceres track, post-Ceres bank/track restart, item fanfare, upload lookup, paired SFX identities/catalogs, handshake, and LoROM stream agree.");
    }

    private static void WriteAudioRomByte(byte[] rom, int snesAddress, byte value) =>
        rom[SuperMetroidAddressSpace.ToRomOffset(snesAddress)] = value;
}

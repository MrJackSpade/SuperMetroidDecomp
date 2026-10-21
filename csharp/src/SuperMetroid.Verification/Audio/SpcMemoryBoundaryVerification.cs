using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Audio.Synthesis;
namespace SuperMetroid.Verification.Audio;
using SuperMetroid.Verification.Harness;

/// <summary>Checks typed SPC RAM views, address wrapping, record order, aliasing, and APU port limits.</summary>
internal static class SpcMemoryBoundaryVerification
{
    /// <summary>
    /// #632: the typed driver-memory views keep native byte order and 16-bit wrapping, advance stream
    /// cursors after reading, round-trip instrument records, upload records across the address-space end,
    /// expose the pan-table overrun as the FIR preset table it aliases, and type the APU ports.
    /// </summary>
    [SpcMemoryBoundary("exact APU memory test: typed driver-memory views against APU RAM")]
    internal static void VerifySpcMemoryBoundary()
    {
        var player = new ManagedSpcPlayer();

        // Upload records wrap at the end of the 64 KiB address space.
        player.Uploads.Upload([0x03, 0x00, 0xff, 0xff, 0x11, 0x22, 0x33, 0x00, 0x00]);
        AssertEqual((byte)0x11, player.ReadApuByteForVerification(0xffff), "upload writes $FFFF first");
        AssertEqual((byte)0x22, player.ReadApuByteForVerification(0x0000), "upload wraps to $0000");
        AssertEqual((byte)0x33, player.ReadApuByteForVerification(0x0001), "upload continues after the wrap");

        // A pointer word's high byte wraps at 16 bits.
        AssertEqual((ushort)0x2211, player.Voices.ReadWord(0xffff), "pointer words wrap their high byte");
        AssertEqual((ushort)0x3322, player.Voices.ReadPointerEntry(0x0000, 0), "pointer-table entry zero");

        // Sequence streams read, then advance with 16-bit wrap.
        int cursor = 0xffff;
        AssertEqual((byte)0x11, player.Music.NextSequenceByte(ref cursor), "sequence byte at $FFFF");
        AssertEqual(0x0000, cursor, "sequence cursor wraps after $FFFF");
        AssertEqual((byte)0x22, player.Music.PeekSequenceByte(0x0000), "peek does not advance");

        // Instrument records: six bytes with a big-endian pitch base.
        var record = new SpcInstrumentRecord(0x87, 0xff, 0xe0, 0x7f, 0x1234);
        player.Uploads.WriteInstrument(0x6c00, record);
        AssertEqual((byte)0x12, player.ReadApuByteForVerification(0x6c04), "pitch base high byte is stored first");
        AssertEqual((byte)0x34, player.ReadApuByteForVerification(0x6c05), "pitch base low byte follows");
        AssertEqual(record, player.Voices.ReadInstrument(0x6c00), "instrument records round-trip");

        // Alias: pan indices from 21 read past the 21 pan samples into the echo FIR preset table.
        player.Uploads.Upload([0x02, 0x00, 0x32, 0x1e, 0x5a, 0xa5, 0x00, 0x00]);
        var (panBase, panNext) = player.Voices.ReadPanVolumePair(0x1e1d + 21);
        AssertEqual(0x5a, panBase, "pan index 21 aliases FIR preset byte 0");
        AssertEqual(unchecked((sbyte)0x5a), player.Music.ReadFirCoefficient(0), "FIR preset byte 0 through its own view");
        AssertEqual(unchecked((sbyte)0xa5), player.Music.ReadFirCoefficient(1), "FIR preset byte 1 through its own view");
        AssertEqual(0xa5, panNext, "pan index 21's next entry aliases FIR preset byte 1");

        // Requests are typed by library: a request outside 0 through 255, and a port that carries no library, are rejected.
        var clock = new SoundAcceptanceClock();
        clock.Request(2, 0x42);
        AssertThrows<ArgumentOutOfRangeException>(() => clock.Request(2, 0x100), "a sound request is 0 through 255");
        AssertThrows<ArgumentOutOfRangeException>(() => clock.Request(SoundAcceptanceClock.LibraryCount, 0), "a sound library index is 0 through 2");
        AssertThrows<ArgumentOutOfRangeException>(() => ApuPorts.LibraryIndexAt(ApuPort.Music), "the music port carries no sound library");
        AssertEqual(ApuPort.SoundLibrary2, new CartridgeAudioCommand.WritePort(ApuPort.SoundLibrary2, 1).InputPort, "port commands round-trip their port");
    }

    /// <summary>Runs the focused SPC memory and APU port boundary check.</summary>
    [VerificationCommand("--spc-memory-boundary")]
    internal static int SpcMemoryBoundaryCommand()
    {
        Suite(nameof(VerifySpcMemoryBoundary), () => VerifySpcMemoryBoundary());
        return 0;
    }
}

using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuiteTimingVerification;
using SuperMetroid.Core.Audio;

/// <summary>Represents spc memory boundary verification.</summary>
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
        player.Upload([0x03, 0x00, 0xff, 0xff, 0x11, 0x22, 0x33, 0x00, 0x00]);
        AssertEqual((byte)0x11, player.ReadApuByteForVerification(0xffff), "upload writes $FFFF first");
        AssertEqual((byte)0x22, player.ReadApuByteForVerification(0x0000), "upload wraps to $0000");
        AssertEqual((byte)0x33, player.ReadApuByteForVerification(0x0001), "upload continues after the wrap");

        // A pointer word's high byte wraps at 16 bits.
        AssertEqual((ushort)0x2211, (ushort)PrivateState.Invoke(player, "ReadWord", 0xffff)!, "pointer words wrap their high byte");
        AssertEqual((ushort)0x3322, (ushort)PrivateState.Invoke(player, "ReadPointerEntry", 0x0000, 0)!, "pointer-table entry zero");

        // Sequence streams read, then advance with 16-bit wrap.
        object[] cursor = [(ushort)0xffff];
        AssertEqual((byte)0x11, (byte)PrivateState.Invoke(player, "NextSequenceByte", cursor)!, "sequence byte at $FFFF");
        AssertEqual((ushort)0x0000, (ushort)cursor[0], "sequence cursor wraps after $FFFF");
        AssertEqual((byte)0x22, (byte)PrivateState.Invoke(player, "PeekSequenceByte", (ushort)0x0000)!, "peek does not advance");

        // Instrument records: six bytes with a big-endian pitch base.
        var record = new SpcInstrumentRecord(0x87, 0xff, 0xe0, 0x7f, 0x1234);
        PrivateState.Invoke(player, "WriteInstrument", 0x6c00, record);
        AssertEqual((byte)0x12, player.ReadApuByteForVerification(0x6c04), "pitch base high byte is stored first");
        AssertEqual((byte)0x34, player.ReadApuByteForVerification(0x6c05), "pitch base low byte follows");
        AssertEqual(record, (SpcInstrumentRecord)PrivateState.Invoke(player, "ReadInstrument", 0x6c00)!, "instrument records round-trip");

        // Alias: pan indices from 21 read past the 21 pan samples into the echo FIR preset table.
        player.Upload([0x02, 0x00, 0x32, 0x1e, 0x5a, 0xa5, 0x00, 0x00]);
        var (panBase, panNext) = ((byte, byte))PrivateState.Invoke(player, "ReadPanVolumePair", 0x1e1d + 21)!;
        AssertEqual((byte)0x5a, panBase, "pan index 21 aliases FIR preset byte 0");
        AssertEqual(unchecked((sbyte)0x5a), (sbyte)PrivateState.Invoke(player, "ReadFirCoefficient", 0)!, "FIR preset byte 0 through its own view");
        AssertEqual(unchecked((sbyte)0xa5), (sbyte)PrivateState.Invoke(player, "ReadFirCoefficient", 1)!, "FIR preset byte 1 through its own view");
        AssertEqual((byte)0xa5, panNext, "pan index 21's next entry aliases FIR preset byte 1");

        // Ports are typed; an out-of-range port is rejected.
        player.WritePort(ApuPort.SoundLibrary3, 0x42);
        AssertThrows<ArgumentOutOfRangeException>(() => player.WritePort((ApuPort)4, 0), "APU input ports are $2140-$2143");
        AssertThrows<ArgumentOutOfRangeException>(() => player.ReadPort((ApuPort)4), "APU output ports are $2140-$2143");
        AssertEqual(ApuPort.SoundLibrary2, CartridgeAudioCommand.WritePort(ApuPort.SoundLibrary2, 1).InputPort, "port commands round-trip their port");
    }

    /// <summary>spc memory boundary command.</summary>
    [VerificationCommand("--spc-memory-boundary")]
    internal static int SpcMemoryBoundaryCommand()
    {
        Suite(nameof(VerifySpcMemoryBoundary), () => VerifySpcMemoryBoundary());
        return 0;
    }
}

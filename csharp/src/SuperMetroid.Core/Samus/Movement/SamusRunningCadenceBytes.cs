using System.Buffers.Binary;
using static SuperMetroid.Core.Samus.Movement.SamusRunningCadenceDefinitions;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>The bytes of the running-cadence animation region: its delay streams, loop markers and reset words.</summary>
internal static class SamusRunningCadenceBytes
{
    /// <summary>
    /// The cadence and reset selection the Speed Booster echo's queue call leaves (<c>$90:85A6</c>): the occupancy
    /// the call saw when admitted, or <see cref="SamusRunningCadenceDefinitions.RejectedEchoSelection"/> when the queue already held six requests.
    /// </summary>
    internal static byte EchoQueueSelection(SoundQueueAdmission admission) =>
        admission.IsAdmitted ? checked((byte)admission.Occupancy) : RejectedEchoSelection;

    /// <summary>
    /// Resolves the speed-stage cadence selected by `$90:853E`. Selection five is the
    /// native sound-queue clobber: bytes three/three at the first cadence frames become
    /// a stream starting in the floor update residue.
    /// </summary>
    internal static SamusRunningCadence SpeedBoostCadence(int selection) => selection switch
    {
        <= 4 => SamusRunningCadence.BoostStage0 + selection,
        MaximumSoundQueueSelection => SamusRunningCadence.SoundQueueClobber,
        _ => throw new InvalidDataException(
            $"Speed Booster sound-queue selection {selection} exceeds the native Max6 result domain."),
    };

    /// <summary>Where a compiled cadence's stream starts in the region.</summary>
    private static int StreamOffset(SamusRunningCadence cadence) => cadence switch
    {
        SamusRunningCadence.Ordinary => OrdinaryDelays,
        SamusRunningCadence.BoostStage0 or SamusRunningCadence.BoostStage1 or SamusRunningCadence.BoostStage2 or
            SamusRunningCadence.BoostStage3 or SamusRunningCadence.BoostStage4 =>
            BoostDelays + (cadence - SamusRunningCadence.BoostStage0) * StreamLength,
        SamusRunningCadence.SoundQueueClobber => throw new ArgumentOutOfRangeException(
            nameof(cadence), cadence, "Stage five's stream is not compiled; it starts in the floor update residue."),
        _ => throw new ArgumentOutOfRangeException(nameof(cadence), cadence, "Undefined running cadence."),
    };

    /// <summary>Resolves the native low-byte reset word selected after a stage advance.</summary>
    internal static ushort ReadResetWord(int selection) => selection switch
    {
        <= 3 => 1,
        4 => 2,
        MaximumSoundQueueSelection => 0,
        _ => throw new InvalidDataException(
            $"Speed Booster reset selection {selection} exceeds the native Max6 result domain."),
    };

    /// <summary>Reads a cadence byte at its frame index.</summary>
    /// <param name="floorUpdateResidue">The session's floor update residue, which a read wrapping into the bank's low half reads.</param>
    /// <param name="cadence">The cadence read.</param>
    /// <param name="byteIndex">The frame index, which wraps within sixteen bits.</param>
    /// <param name="windows">Where the region sits in its bank, which a read leaving the region needs; null when none can.</param>
    internal static byte ReadAnimationByte(
        SuperMetroid.Core.Compatibility.IFloorUpdateResidue? floorUpdateResidue,
        SamusRunningCadence cadence,
        int byteIndex,
        SamusAnimationWindows? windows)
    {
        if (cadence == SamusRunningCadence.SoundQueueClobber)
        {
            // Stage five points at `$91:0303`. This is a genuine cartridge bug: the selected
            // delay comes from the Mode 7 transfer queue's residue (#1338), not cadence data.
            return ReadWindow(floorUpdateResidue, windows, unchecked((ushort)(SoundQueueClobberStart + byteIndex)));
        }
        int offset = unchecked((ushort)(StreamOffset(cadence) + byteIndex));
        if (offset < RegionByteCount)
            return CompiledByte(offset, windows);
        return ReadWindow(floorUpdateResidue, windows, unchecked((ushort)(RequireWindows(windows).RunningCadences + offset)));
    }

    /// <summary>
    /// Calculates the ten-frame delay, loop marker, and reset bytes <paramref name="offset"/> bytes into the region, and
    /// the bytes of the words naming the streams.
    /// </summary>
    /// <param name="offset">The byte's offset into the region, which must lie inside it.</param>
    /// <param name="windows">Where the region sits in its bank, which a byte of a word naming a stream needs.</param>
    internal static byte CompiledByte(int offset, SamusAnimationWindows? windows)
    {
        if (offset is OrdinaryStreamWord or OrdinaryStreamWord + 1)
            return WordByte(StreamPosition(windows, SamusRunningCadence.Ordinary), offset - OrdinaryStreamWord);
        if (offset is >= OrdinaryDelays and < StageStreamTable)
            return offset == StageStreamTable - 1 ? LoopMarker : OrdinaryDelay;
        if (offset is >= StageStreamTable and < BoostDelays)
        {
            int index = offset - StageStreamTable;
            return WordByte(StreamPosition(windows, SamusRunningCadence.BoostStage0 + index / 2), index & 1);
        }
        if (offset is >= BoostDelays and < ResetWords)
        {
            int index = offset - BoostDelays;
            int frame = index % StreamLength;
            if (frame == StreamLength - 1)
                return LoopMarker;
            return (index / StreamLength) switch
            {
                0 => 3,
                1 => (byte)(2 + (frame & 1)),
                2 => 2,
                3 => (byte)(1 + (frame & 1)),
                _ => 1,
            };
        }
        if (offset is >= ResetWords and < SoundQueueStageFiveReset)
        {
            int index = offset - ResetWords;
            return (index & 1) != 0 ? (byte)0 : index < 8 ? (byte)1 : (byte)2;
        }
        return 0;
    }

    /// <summary>Reads window position <paramref name="position"/>: the floor update residue in the low half, else the region.</summary>
    private static byte ReadWindow(SuperMetroid.Core.Compatibility.IFloorUpdateResidue? floorUpdateResidue, SamusAnimationWindows? windows, int position)
    {
        if (position < SuperMetroid.Core.Compatibility.FloorUpdateResidue.ResidueWindowEnd)
            return (floorUpdateResidue ?? throw new InvalidOperationException(
                "Running-cadence stage five reads the floor update residue; bind the session's to this Samus."))
                .ByteAt(position - SuperMetroid.Core.Samus.Poses.SamusAnimationDelayDefinitions.FloorUpdateResidueOffset);
        int offset = position - RequireWindows(windows).RunningCadences;
        if ((uint)offset < RegionByteCount)
            return CompiledByte(offset, windows);
        throw new InvalidDataException(
            $"Samus running-cadence window position ${position:X4} is not compiled cadence data or a mutable bank-$91 low-half alias.");
    }

    /// <summary>The window position <paramref name="cadence"/>'s stream starts at, as the words naming the streams hold it.</summary>
    private static int StreamPosition(SamusAnimationWindows? windows, SamusRunningCadence cadence) =>
        RequireWindows(windows).RunningCadences + StreamOffset(cadence);

    /// <summary>Byte <paramref name="index"/> of the little-endian word <paramref name="word"/>.</summary>
    private static byte WordByte(int word, int index)
    {
        Span<byte> bytes = stackalloc byte[sizeof(ushort)];
        BinaryPrimitives.WriteUInt16LittleEndian(bytes, unchecked((ushort)word));
        return bytes[index == 0 ? 0 : 1];
    }

    /// <summary>The installed window positions a read leaving the cadences needs.</summary>
    private static SamusAnimationWindows RequireWindows(SamusAnimationWindows? windows) => windows ?? throw new InvalidOperationException(
        "A running-cadence read leaving the compiled cadences needs the installed Samus overread catalog's animation windows.");
}

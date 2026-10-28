namespace SuperMetroid.Core.Samus.Movement;

/// <summary>NTSC running cadence and boost-stage timing: gameplay rules, not editable art.</summary>
/// <remarks>
/// Independently reviewed for #1165 against the complete NTSC J/U v1.0 catalog and
/// pinned bank_91.asm, bank_90 stage advancement, and bank_80 Max6 queue accumulator.
/// Keep these prior conversions: ordinary frames use two ticks; boost stages use
/// 3, 2+(frame&amp;1), 2, 1+(frame&amp;1), and 1 tick, respectively. Every stream ends
/// at frame ten with command $FF. Pointers advance by eleven bytes; reset words are
/// one for stages0..3 and two for stage4. Separate named proofs cover each logical
/// mapping and byte view. This is NTSC policy, not a PAL interpolation rule.
/// Stage5 is a bounded adjacent-data case: pointer $0303 aliases the first two stage0
/// frames, and reset zero aliases pose0's direction/movement bytes. Its delay remains
/// mutable WRAM. Wrapped restored indices may reach other compiled catalog bytes or
/// the low bank; unrelated upper-bank code stays rejected. No stored table remains.
/// The cadences are read by offset into their region; a read leaving it finds the region's place in the bank's 16-bit
/// window, and the words naming each stream, in the installed <see cref="SamusAnimationWindows"/> (#1411).
/// </remarks>
internal static class SamusRunningCadenceDefinitions
{
    /// <summary>AnimationDelayTable_Running_NoSpeedBooster_pointer, the region's first word: the word naming the ordinary stream.</summary>
    private const int OrdinaryStreamWord = 0;
    /// <summary>AnimationDelayTable_Running_NoSpeedBooster: eleven cadence/command bytes.</summary>
    private const int OrdinaryDelays = 2;
    /// <summary>AnimationDelayTable_Running_SpeedBooster_pointers: the five words naming each stage's stream.</summary>
    private const int StageStreamTable = 13;
    /// <summary>AnimationDelayTable_Running_SpeedBooster_0..4, five eleven-byte cadence streams.</summary>
    private const int BoostDelays = 23;
    /// <summary>SpeedBoostTimerResetValues: stages zero through three reset to one, stage four to two.</summary>
    private const int ResetWords = 78;
    /// <summary>
    /// The two zero bytes after the reset words, read when the native echo-sound call returns admitted queue occupancy five
    /// and the reset lookup crosses into pose zero.
    /// </summary>
    private const int SoundQueueStageFiveReset = 88;
    /// <summary>The bytes the region fills, from the ordinary stream's word to the stage-five reset word.</summary>
    internal const int RegionByteCount = 90;
    /// <summary>Where stage five's stream starts: window position $0303, in the floor update residue the bank's low half aliases.</summary>
    private const int SoundQueueClobberStart = 0x0303;
    /// <summary>Each native running cadence stream has ten frames followed by the $FF loop instruction.</summary>
    private const int StreamLength = 11;
    /// <summary>
    /// The maximum selector the real Max6 echo queue can return: admitted
    /// occupancy zero through five, or the rejected selection.
    /// </summary>
    internal const byte MaximumSoundQueueSelection = 5;

    /// <summary>
    /// The selection a rejected echo leaves: the cartridge's queue call returns the rejected request itself where
    /// an admitted call returns the occupancy, and the Speed Booster echo is library three's third request.
    /// </summary>
    private const byte RejectedEchoSelection = 3;

    /// <summary>
    /// The cadence and reset selection the Speed Booster echo's queue call leaves (<c>$90:85A6</c>): the occupancy
    /// the call saw when admitted, or <see cref="RejectedEchoSelection"/> when the queue already held six requests.
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
            return offset == StageStreamTable - 1 ? (byte)0xff : (byte)2;
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
                return 0xff;
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
    private static byte WordByte(int word, int index) =>
        index == 0 ? SnesLittleEndian.LowByte(unchecked((ushort)word)) : SnesLittleEndian.HighByte(unchecked((ushort)word));

    /// <summary>The installed window positions a read leaving the cadences needs.</summary>
    private static SamusAnimationWindows RequireWindows(SamusAnimationWindows? windows) => windows ?? throw new InvalidOperationException(
        "A running-cadence read leaving the compiled cadences needs the installed Samus overread catalog's animation windows.");
}

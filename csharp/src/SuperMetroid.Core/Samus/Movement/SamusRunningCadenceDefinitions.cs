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
/// </remarks>
internal static class SamusRunningCadenceDefinitions
{
    /// <summary>$91:B5D3 AnimationDelayTable_Running_NoSpeedBooster, eleven cadence/command bytes.</summary>
    private const int OrdinaryDelays = 0x91b5d3;
    /// <summary>$91:B5DE AnimationDelayTable_Running_SpeedBooster_pointers: the five words naming each stage's stream.</summary>
    private const int StageStreamTable = 0x91b5de;
    /// <summary>$91:B5E8..B61E AnimationDelayTable_Running_SpeedBooster_0..4, five eleven-byte cadence streams.</summary>
    private const int BoostDelays = 0x91b5e8;
    /// <summary>$91:B61F SpeedBoostTimerResetValues: stages zero through three reset to one, stage four to two.</summary>
    private const int ResetWords = 0x91b61f;
    /// <summary>Each native running cadence stream has ten frames followed by the $FF loop instruction.</summary>
    private const int StreamLength = 11;
    /// <summary>
    /// The maximum high-byte selector the real Max6 echo queue can return: admitted
    /// occupancy zero through five, or rejected sound ID three.
    /// </summary>
    internal const byte MaximumSoundQueueSelection = 5;

    /// <summary>
    /// Resolves the speed-stage cadence selected by `$90:853E`. Selection five is the
    /// native sound-queue clobber: bytes three/three at the first cadence frames become
    /// a stream starting in the Mode 7 transfer queue residue.
    /// </summary>
    internal static SamusRunningCadence SpeedBoostCadence(byte selection) => selection switch
    {
        <= 4 => SamusRunningCadence.BoostStage0 + selection,
        MaximumSoundQueueSelection => SamusRunningCadence.SoundQueueClobber,
        _ => throw new InvalidDataException(
            $"Speed Booster sound-queue selection {selection} exceeds the native Max6 result domain."),
    };

    /// <summary>Where a cadence's stream starts in the 16-bit window a wrapped frame index reads.</summary>
    private static ushort StreamStart(SamusRunningCadence cadence) => cadence switch
    {
        SamusRunningCadence.Ordinary => unchecked((ushort)OrdinaryDelays),
        SamusRunningCadence.BoostStage0 or SamusRunningCadence.BoostStage1 or SamusRunningCadence.BoostStage2 or
            SamusRunningCadence.BoostStage3 or SamusRunningCadence.BoostStage4 =>
            unchecked((ushort)(BoostDelays + (cadence - SamusRunningCadence.BoostStage0) * StreamLength)),
        SamusRunningCadence.SoundQueueClobber => 0x0303,
        _ => throw new ArgumentOutOfRangeException(nameof(cadence), cadence, "Undefined running cadence."),
    };

    /// <summary>Resolves the native low-byte reset word selected after a stage advance.</summary>
    internal static ushort ReadResetWord(byte selection) => selection switch
    {
        <= 3 => 1,
        4 => 2,
        MaximumSoundQueueSelection => 0,
        _ => throw new InvalidDataException(
            $"Speed Booster reset selection {selection} exceeds the native Max6 result domain."),
    };

    /// <summary>Reads a cadence byte at its wrapped frame index.</summary>
    internal static byte ReadAnimationByte(
        SuperMetroid.Core.Compatibility.IFloorUpdateResidue? mode7Residue,
        SamusRunningCadence cadence,
        int byteIndex)
    {
        int address = 0x910000 | unchecked((ushort)(StreamStart(cadence) + byteIndex));
        if (TryReadCompiledByte(address, out byte value))
            return value;

        // Stage five points at `$91:0303`. This is a genuine cartridge bug: the selected
        // delay comes from the Mode 7 transfer queue's residue (#1338), not cadence data.
        SnesAddress sourceAddress = SnesAddress.FromBusAddress(address);
        if (sourceAddress.Bank == 0x91 && !sourceAddress.IsUpperLoRomWindow)
            return (mode7Residue ?? throw new InvalidOperationException(
                "Running-cadence stage five reads the floor update residue; bind the session's to this Samus."))
                .ByteAt(sourceAddress.Offset - SuperMetroid.Core.Samus.Poses.SamusAnimationDelayDefinitions.FloorUpdateResidueOffset);

        throw new InvalidDataException(
            $"Samus running-cadence byte {sourceAddress} is not compiled cadence data " +
            "or a mutable bank-$91 low-half alias.");
    }

    /// <summary>The two compiled 16-bit words outside the cadence streams and reset table, valued by address.</summary>
    private enum StandaloneWord
    {
        /// <summary>$91:B5D1, AnimationDelayTable_Running_NoSpeedBooster_pointer: the word naming the ordinary stream.</summary>
        OrdinaryStreamWord = 0x91b5d1,
        /// <summary>$91:B629-$B62A, the two zero bytes read when the native echo-sound call returns admitted queue occupancy five and the reset lookup crosses into pose zero.</summary>
        SoundQueueStageFiveReset = 0x91b629,
    }

    /// <summary>Decodes the ordinary-stream word or stage-five zero reset word outside the cadence streams.</summary>
    private static ushort StandaloneWordValue(StandaloneWord word) => word switch
    {
        StandaloneWord.OrdinaryStreamWord => unchecked((ushort)OrdinaryDelays),
        StandaloneWord.SoundQueueStageFiveReset => 0,
        _ => throw new InvalidOperationException($"Undefined {nameof(StandaloneWord)} {(int)word}."),
    };

    /// <summary>Calculates pointer, ten-frame delay, loop marker, and reset bytes in the bounded bank-$91 catalog.</summary>
    internal static bool TryReadCompiledByte(int address, out byte value)
    {
        foreach (StandaloneWord word in Enum.GetValues<StandaloneWord>())
        {
            int byteOffset = address - (int)word;
            if (byteOffset is 0 or 1)
            {
                value = unchecked((byte)(StandaloneWordValue(word) >> (byteOffset * 8)));
                return true;
            }
        }
        if (address is >= OrdinaryDelays and < StageStreamTable)
        {
            value = address == StageStreamTable - 1 ? (byte)0xff : (byte)2;
            return true;
        }
        if (address is >= StageStreamTable and < BoostDelays)
        {
            int index = address - StageStreamTable;
            value = unchecked((byte)(StreamStart(SamusRunningCadence.BoostStage0 + index / 2) >> ((index & 1) * 8)));
            return true;
        }
        if (address is >= BoostDelays and < ResetWords)
        {
            int index = address - BoostDelays;
            int frame = index % StreamLength;
            if (frame == StreamLength - 1)
                value = 0xff;
            else value = (index / StreamLength) switch
            {
                0 => 3,
                1 => (byte)(2 + (frame & 1)),
                2 => 2,
                3 => (byte)(1 + (frame & 1)),
                _ => 1,
            };
            return true;
        }
        if (address is >= ResetWords and < (ResetWords + 10))
        {
            int index = address - ResetWords;
            value = (index & 1) != 0 ? (byte)0 : index < 8 ? (byte)1 : (byte)2;
            return true;
        }
        value = 0;
        return false;
    }
}

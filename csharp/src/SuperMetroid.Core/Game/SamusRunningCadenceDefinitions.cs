using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>NTSC running cadence and boost-stage timing: gameplay rules, not editable art.</summary>
internal static class SamusRunningCadenceDefinitions
{
    /// <summary>$91:B5D1 AnimationDelayTable_Running_NoSpeedBooster_pointer selects ten two-tick frames and a loop command.</summary>
    private const int OrdinaryPointer = 0x91b5d1;
    /// <summary>$91:B5D3 AnimationDelayTable_Running_NoSpeedBooster, eleven cadence/command bytes.</summary>
    private const int OrdinaryDelays = 0x91b5d3;
    /// <summary>$91:B5DE AnimationDelayTable_Running_SpeedBooster_pointers, five native bank-$91 pointers.</summary>
    private const int BoostPointers = 0x91b5de;
    /// <summary>$91:B5E8..B61E AnimationDelayTable_Running_SpeedBooster_0..4, five eleven-byte cadence streams.</summary>
    private const int BoostDelays = 0x91b5e8;
    /// <summary>$91:B61F SpeedBoostTimerResetValues: stages zero through three reset to one, stage four to two.</summary>
    private const int ResetWords = 0x91b61f;
    /// <summary>
    /// `$91:B629-$B62A` are the two zero bytes observed when the native echo-sound call
    /// returns admitted queue occupancy five and the reset lookup crosses into pose zero.
    /// </summary>
    private const int SoundQueueStageFiveResetWord = 0x91b629;
    /// <summary>Each native running cadence stream has ten frames followed by the $FF loop instruction.</summary>
    private const int StreamLength = 11;
    /// <summary>
    /// The maximum high-byte selector the real Max6 echo queue can return: admitted
    /// occupancy zero through five, or rejected sound ID three.
    /// </summary>
    internal const byte MaximumSoundQueueSelection = 5;

    /// <summary>Compiled pointer to the ordinary ten-frame/two-tick running cadence.</summary>
    internal static ushort DefaultRunningDelayListPointer => unchecked((ushort)OrdinaryDelays);

    /// <summary>
    /// Resolves the speed-stage pointer selected by `$90:853E`. Selection five is the
    /// native sound-queue clobber: bytes three/three at the first cadence frames become
    /// mutable low-bank pointer `$0303`.
    /// </summary>
    internal static ushort ReadSpeedBoostDelayListPointer(byte selection) => selection switch
    {
        <= 4 => unchecked((ushort)(BoostDelays + selection * StreamLength)),
        MaximumSoundQueueSelection => 0x0303,
        _ => throw new InvalidDataException(
            $"Speed Booster sound-queue selection {selection} exceeds the native Max6 result domain."),
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

    /// <summary>Reads a cadence byte from a typed list pointer plus its native wrapped index.</summary>
    internal static byte ReadAnimationByte(
        ISnesAddressSpace bus,
        ushort listPointer,
        ushort byteIndex)
    {
        ArgumentNullException.ThrowIfNull(bus);
        int address = 0x910000 | unchecked((ushort)(listPointer + byteIndex));
        if (TryReadCompiledByte(address, out byte value))
            return value;

        // Stage five points at `$91:0303`. This is a genuine cartridge bug: the selected
        // delay comes from the mutable low half of bank $91, not immutable cadence data.
        SnesAddress sourceAddress = SnesAddress.FromBusAddress(address);
        if (sourceAddress.Bank == 0x91 && !sourceAddress.IsUpperLoRomWindow)
            return bus.ReadByte(address);

        throw new InvalidDataException(
            $"Samus running-cadence byte {sourceAddress} is not compiled cadence data " +
            "or a mutable bank-$91 low-half alias.");
    }

    /// <summary>Returns one byte from the complete bounded native cadence catalog.</summary>
    internal static byte ReadCompiledByte(int address)
    {
        if (TryReadCompiledByte(address, out byte value))
            return value;
        throw new ArgumentOutOfRangeException(
            nameof(address), address, "Address is outside the compiled Samus running-cadence catalog.");
    }

    private static bool TryReadCompiledByte(int address, out byte value)
    {
        if (address is OrdinaryPointer or OrdinaryPointer + 1)
        {
            value = unchecked((byte)((ushort)OrdinaryDelays >> ((address - OrdinaryPointer) * 8)));
            return true;
        }
        if (address >= OrdinaryDelays && address < BoostPointers)
        {
            value = address == BoostPointers - 1 ? (byte)0xff : (byte)2;
            return true;
        }
        if (address >= BoostPointers && address < BoostDelays)
        {
            int index = address - BoostPointers;
            ushort pointer = unchecked((ushort)(BoostDelays + (index / 2) * StreamLength));
            value = unchecked((byte)(pointer >> ((index & 1) * 8)));
            return true;
        }
        if (address >= BoostDelays && address < ResetWords)
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
        if (address >= ResetWords && address < ResetWords + 10)
        {
            int index = address - ResetWords;
            value = (index & 1) != 0 ? (byte)0 : index < 8 ? (byte)1 : (byte)2;
            return true;
        }
        if (address is SoundQueueStageFiveResetWord or SoundQueueStageFiveResetWord + 1)
        {
            value = 0;
            return true;
        }

        value = 0;
        return false;
    }
}

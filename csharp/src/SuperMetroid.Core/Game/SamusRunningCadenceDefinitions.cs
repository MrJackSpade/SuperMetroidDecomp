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
    /// <summary>Each native running cadence stream has ten frames followed by the $FF loop instruction.</summary>
    private const int StreamLength = 11;

    internal static byte ReadByte(ISnesAddressSpace bus, int address)
    {
        if (address is OrdinaryPointer or OrdinaryPointer + 1)
            return unchecked((byte)((ushort)OrdinaryDelays >> ((address - OrdinaryPointer) * 8)));
        if (address >= OrdinaryDelays && address < BoostPointers)
            return address == BoostPointers - 1 ? (byte)0xff : (byte)2;
        if (address >= BoostPointers && address < BoostDelays)
        {
            int index = address - BoostPointers;
            ushort pointer = unchecked((ushort)(BoostDelays + (index / 2) * StreamLength));
            return unchecked((byte)(pointer >> ((index & 1) * 8)));
        }
        if (address >= BoostDelays && address < ResetWords)
        {
            int index = address - BoostDelays;
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
        if (address >= ResetWords && address < ResetWords + 10)
        {
            int index = address - ResetWords;
            return (index & 1) != 0 ? (byte)0 : index < 8 ? (byte)1 : (byte)2;
        }

        // Sound-queue clobber and restored indexes can select adjacent pose data or
        // low-bank WRAM. Those are not cadence definitions: retain the live bus read.
        return bus.ReadByte(address);
    }

    internal static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(ReadByte(bus, address) | (ReadByte(bus, address + 1) << 8)));
}

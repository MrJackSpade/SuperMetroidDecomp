using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Cartridge values and helpers of <see cref="DraygonCannonWorkRamAddresses"/> that only verification reads.</summary>
internal static class DraygonCannonWorkRamAddressesConstants
{
    /// <summary>Decodes a PLM argument's bank-$7E byte address into its cannon word, failing for any other address.</summary>
    /// <param name="pointer">Bank-relative WRAM byte address, not a cannon selector or a full 24-bit bus address.</param>
    public static DraygonCannonControlWord Decode(ushort pointer) =>
        DraygonCannonWorkRamAddresses.TryDecode(pointer, out DraygonCannonControlWord word)
            ? word
            : throw new InvalidDataException($"Draygon cannon PLM targeted unrecognized WRAM word ${pointer:X4}.");
}

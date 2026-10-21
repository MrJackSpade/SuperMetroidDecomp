using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// The bank-$7E addresses of Draygon's five cannon-control words, which room PLM data and Draygon's AI name them by in
/// the cartridge, for oracles that compare the port's words with cartridge memory.
/// </summary>
internal static class DraygonCannonControlWordAddresses
{
    /// <summary>Each cannon word by its bank-relative WRAM address.</summary>
    private static readonly Dictionary<ushort, DraygonCannonControlWord> Words = new()
    {
        [0x8802] = DraygonCannonControlWord.UpperLeft,
        [0x8804] = DraygonCannonControlWord.LowerLeft,
        [0x8806] = DraygonCannonControlWord.UpperRight,
        [0x8808] = DraygonCannonControlWord.LowerRight,
        [0x880a] = DraygonCannonControlWord.UnusedBottom,
    };

    /// <summary>The cannon word at bank-relative WRAM address <paramref name="address"/>, failing for any other address.</summary>
    public static DraygonCannonControlWord Decode(ushort address) => Words.TryGetValue(address, out DraygonCannonControlWord word)
        ? word
        : throw new InvalidDataException($"Draygon cannon PLM targeted unrecognized WRAM word ${address:X4}.");
}

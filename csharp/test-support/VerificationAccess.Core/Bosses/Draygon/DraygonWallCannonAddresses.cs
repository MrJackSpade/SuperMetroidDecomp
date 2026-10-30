using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// The bank-$7E addresses of Draygon's five cannon-control words, which room PLM data and Draygon's AI name them by in
/// the cartridge, for oracles that compare the port's words with cartridge memory.
/// </summary>
internal static class DraygonWallCannonAddresses
{
    /// <summary>Each cannon word by its bank-relative WRAM address.</summary>
    private static readonly Dictionary<ushort, DraygonWallCannon> Words = new()
    {
        [0x8802] = DraygonWallCannon.UpperLeft,
        [0x8804] = DraygonWallCannon.LowerLeft,
        [0x8806] = DraygonWallCannon.UpperRight,
        [0x8808] = DraygonWallCannon.LowerRight,
        [0x880a] = DraygonWallCannon.UnusedBottom,
    };

    /// <summary>The cannon word at bank-relative WRAM address <paramref name="address"/>, failing for any other address.</summary>
    public static DraygonWallCannon Decode(ushort address) => Words.TryGetValue(address, out DraygonWallCannon word)
        ? word
        : throw new InvalidDataException($"Draygon cannon PLM targeted unrecognized WRAM word ${address:X4}.");
}

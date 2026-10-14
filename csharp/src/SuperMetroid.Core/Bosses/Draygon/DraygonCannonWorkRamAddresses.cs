namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// The WRAM addresses of Draygon's five cannon-control words. Room PLM data names a cannon by its word's address
/// (the <c>PLM_Vars</c> argument), so the address is decoded here, at the boundary, into a
/// <see cref="DraygonCannonControlWord"/>.
/// </summary>
public static class DraygonCannonWorkRamAddresses
{
    /// <summary><c>$7E:8802</c>: the first cannon-control word, the upper-left cannon's.</summary>
    public const int ControlWordsStart = 0x7e8802;

    /// <summary>Each cannon word by the bank-relative address PLM arguments store it at.</summary>
    private static readonly Dictionary<ushort, DraygonCannonControlWord> ByPointer = new()
    {
        [0x8802] = DraygonCannonControlWord.UpperLeft,
        [0x8804] = DraygonCannonControlWord.LowerLeft,
        [0x8806] = DraygonCannonControlWord.UpperRight,
        [0x8808] = DraygonCannonControlWord.LowerRight,
        [0x880a] = DraygonCannonControlWord.UnusedBottom,
    };

    /// <summary>
    /// Decodes a PLM argument's bank-$7E byte address into its cannon word, or reports that it names none.
    /// </summary>
    /// <param name="pointer">Bank-relative WRAM byte address, not a cannon selector or a full 24-bit bus address.</param>
    /// <param name="word">The cannon word at <paramref name="pointer"/>.</param>
    public static bool TryDecode(ushort pointer, out DraygonCannonControlWord word) => ByPointer.TryGetValue(pointer, out word);

    /// <summary>Decodes a PLM argument's bank-$7E byte address into its cannon word, failing for any other address.</summary>
    /// <param name="pointer">Bank-relative WRAM byte address, not a cannon selector or a full 24-bit bus address.</param>
    public static DraygonCannonControlWord Decode(ushort pointer) =>
        TryDecode(pointer, out DraygonCannonControlWord word)
            ? word
            : throw new InvalidDataException($"Draygon cannon PLM targeted unrecognized WRAM word ${pointer:X4}.");
}

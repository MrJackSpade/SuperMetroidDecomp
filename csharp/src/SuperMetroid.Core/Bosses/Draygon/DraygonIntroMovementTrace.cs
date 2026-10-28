namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// The traced Keiko Love path of Draygon's opening dance as signed X/Y deltas (#1461). Each record of
/// <see cref="DraygonIntroDanceDefinitions.PackedMovementHex"/> packs a delta pair as two biased nibbles, the X
/// delta above the Y delta; this type is the one reader of that packing.
/// </summary>
[EncodingOwner("Draygon intro dance biased-nibble movement records")]
internal static class DraygonIntroMovementTrace
{
    /// <summary>How many codes one nibble can hold; the X nibble is the record's high digit in this base.</summary>
    private const int CodesPerNibble = 16;

    /// <summary>The bias a packed nibble carries above its signed delta.</summary>
    private const int Bias = 8;

    /// <summary>The packed records, the two delete sentinels excluded.</summary>
    private static readonly byte[] Records = Convert.FromHexString(DraygonIntroDanceDefinitions.PackedMovementHex);

    /// <summary>The signed delta of packed record <paramref name="index"/>.</summary>
    /// <param name="index">A record index, the delete sentinels excluded.</param>
    internal static Offset16<RoomPixels> Delta(int index)
    {
        int record = Records[index];
        return new((short)(record / CodesPerNibble - Bias), (short)(record % CodesPerNibble - Bias));
    }
}

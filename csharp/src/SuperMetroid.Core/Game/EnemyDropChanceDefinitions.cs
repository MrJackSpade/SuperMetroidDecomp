namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled fixed enemy item-drop probability records authored at
/// <c>$B4:F1F4-$B4:F4B7</c>.
/// </summary>
internal static class EnemyDropChanceDefinitions
{
    /// <summary>Bank-$B4 address used by native enemy drop probability pointers.</summary>
    internal const int NativeBank = 0xb40000;

    /// <summary>Pointer of the first six-byte item-drop probability record.</summary>
    internal const ushort FirstPointer = 0xf1f4;

    /// <summary>Pointer of the final six-byte item-drop probability record.</summary>
    internal const ushort LastPointer = 0xf4b2;

    /// <summary>Number of probability bytes in one native item-drop record.</summary>
    internal const int RecordSize = 6;

    /// <summary>
    /// Number of contiguous native records, including the unused records at
    /// <c>$B4:F37A</c> and <c>$B4:F494</c>.
    /// </summary>
    internal const int RecordCount = 118;

    private const string PackedChanceHex =
        "3C3C3C053C0A1E5055280A0A3C3C3C053C0A1403558905053C3C3C050A3C00A5500005053C3C3C053C0A3C3C3C053C0A3719AA00050037197D003200" +
        "50461E1932001E4650460500461E504605004614505005003C3C3C053C0A3C3C3C053C0A008C0A0064053C3C3C053C0A00644605460A325F46001414" +
        "325A46051414321E504B0A0A37500A283C0A23780A143C0A0096050064005014325F05050A1414C305050A1414C305050A1414C305050A1414C30505" +
        "0A2DC5010101051E7800640001000000FE0000010000FE0000010000FE0000010000FE000100000000FE0005000000FA01640000009A0100000000FE" +
        "0001000000FE0100000000FE0100000000FE0100000000FE8214006400058214006400055014504105055050500505055019503C05053C3C3C3C0F00" +
        "140A55820505142855640505141437643705501E46460500461E464B050555500050000A00823C051E1E505050050505505050050505007832003223" +
        "323232053232461E4B4605053232464B050519325A0A321E000000FF000000823C051E1E2D50501E0A0A327850000500327850000500321900199B00" +
        "321E642D140A3278500005001E4650460500327850000500327850000500505050050505505050050505000000FF0000505050050505505050050505" +
        "5050500505055050500505055050500505055050500505055050500505055050500505053232320032377414413600003232320032371E1E323C5500" +
        "3232320032370A14C81900003232320032370A23C8000A00000ADC05140000786405140A323232003237141464690A00323232003237051E6E640505" +
        "323232003237000000FF0000000000FF0000000000FF0000000000FF0000000000FF0000000000FF0000000000FF0000000000FF0000000000FF0000" +
        "000000FF0000000000FF0000000000FF0000000000FF0000000000FF0000000000FF0000000000FF0000000000FF0000";

    private static readonly byte[] PackedChances = Convert.FromHexString(PackedChanceHex);

    /// <summary>
    /// Copies one aligned native probability record into <paramref name="destination"/>.
    /// Returns false for pointers outside the authored table or between record boundaries,
    /// allowing explicit diagnostic fixtures to retain their address-space-backed data.
    /// </summary>
    internal static bool TryCopy(ushort pointer, Span<byte> destination)
    {
        if (destination.Length < RecordSize)
        {
            throw new ArgumentException(
                $"Enemy drop chance destination requires {RecordSize} bytes.",
                nameof(destination));
        }

        int byteOffset = pointer - FirstPointer;
        if (byteOffset < 0 ||
            byteOffset % RecordSize != 0 ||
            byteOffset >= RecordCount * RecordSize)
        {
            return false;
        }

        PackedChances.AsSpan(byteOffset, RecordSize).CopyTo(destination);
        return true;
    }
}

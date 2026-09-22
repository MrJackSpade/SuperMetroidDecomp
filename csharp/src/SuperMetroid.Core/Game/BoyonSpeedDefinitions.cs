namespace SuperMetroid.Core.Game;

/// <summary>NTSC Boyon bounce curve and native eight-bit hardware multiplication.</summary>
public static class BoyonSpeedDefinitions
{
    /// <summary>$A2:86DF, BoyonData.speedMultipliers: parameter one's low byte selects a hardware multiplier.</summary>
    /// <remarks>
    /// #625 / #645 retained table: all eight unsigned words at $A2:86DF-$86EE
    /// match the pinned NTSC J/U v1.0 ROM and bank-A2 disassembly. Values double
    /// through index 3, then rise by 2, 3, 3, and 4; a piecewise rounded formula
    /// would obscure these eight authored choices. The initializer selects indices
    /// 0..7 and rejects 8, which would read the adjacent jump-height table.
    /// VerifyCompiledBoyonSpeeds covers all eight real initializer selections.
    /// </remarks>
    public static ushort InitialMultiplier(int index) => index switch
    {
        0 => 1, 1 => 2, 2 => 4, 3 => 8, 4 => 10, 5 => 13, 6 => 16, 7 => 20,
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };

    /// <summary>$A2:86EF, BoyonData.jumpHeights: parameter one's high byte selects one of nine 8.8 heights.</summary>
    /// <remarks>
    /// #625 / #644 exact NTSC J/U v1.0 algorithm: for high-byte selector i in 0..8,
    /// the unsigned height is $3000 + i*$1000. All nine words at $A2:86EF-$8700
    /// match the pinned ROM and bank-A2 disassembly. The native initializer doubles
    /// the selector for its word offset; selector 9 would read the adjacent curve
    /// and is outside this height domain. VerifyCompiledBoyonSpeeds exercises every
    /// authored height through the real initializer and rejects selector 9.
    /// </remarks>
    public static ushort InitialHeight(int index) => (uint)index < 9
        ? (ushort)(0x3000 + index * 0x1000)
        : throw new ArgumentOutOfRangeException(nameof(index));

    /// <summary>$A2:8701, BoyonData.speedTable: 23 triangular-number bytes.</summary>
    public const int ReferenceAddress = 0xa28701;
    /// <summary>$A2:875E/$880F/$885B clamp indices at or above 23 to $FF before multiplication.</summary>
    public const int StoredSampleCount = 23;

    /// <summary>The native curve is k(k+1)/2, with explicit byte saturation beyond its stored samples.</summary>
    /// <remarks>
    /// #625 / #643 exact NTSC J/U v1.0 proof: each of the 23 unsigned bytes at
    /// $A2:8701-$8717 equals k*(k+1)/2 for authored index k in 0..22, ending at $FD.
    /// The native caller reads first, then substitutes $FF for every index at or above
    /// 23 before its eight-bit hardware multiply; the nominal overread at index 23 is
    /// discarded. This method models that effective caller result, not a larger table.
    /// VerifyCompiledBoyonSpeeds checks all ROM bytes, all 65,536 input indices, and
    /// every low-byte multiplier through the real production calculation.
    /// </remarks>
    public static byte Sample(ushort index) => index < StoredSampleCount
        ? (byte)(index * (index + 1) / 2)
        : byte.MaxValue;

    /// <summary>Only the low multiplier byte reaches WRMPYB; the result is an unsigned word.</summary>
    public static ushort Multiply(ushort index, ushort multiplier) => (ushort)(Sample(index) * (byte)multiplier);
}

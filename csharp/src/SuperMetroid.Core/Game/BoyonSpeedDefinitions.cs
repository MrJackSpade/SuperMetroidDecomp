namespace SuperMetroid.Core.Game;

/// <summary>NTSC Boyon bounce curve and native eight-bit hardware multiplication.</summary>
public static class BoyonSpeedDefinitions
{
    /// <summary>$A2:86DF, BoyonData.speedMultipliers: parameter one's low byte selects a hardware multiplier.</summary>
    public static ushort InitialMultiplier(int index) => index switch
    {
        0 => 1, 1 => 2, 2 => 4, 3 => 8, 4 => 10, 5 => 13, 6 => 16, 7 => 20,
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };

    /// <summary>$A2:86EF, BoyonData.jumpHeights: parameter one's high byte selects one of nine 8.8 heights.</summary>
    public static ushort InitialHeight(int index) => (uint)index < 9
        ? (ushort)(0x3000 + index * 0x1000)
        : throw new ArgumentOutOfRangeException(nameof(index));

    /// <summary>$A2:8701, BoyonData.speedTable: 23 triangular-number bytes.</summary>
    public const int ReferenceAddress = 0xa28701;
    /// <summary>$A2:875E/$880F/$885B clamp indices at or above 23 to $FF before multiplication.</summary>
    public const int StoredSampleCount = 23;

    /// <summary>The native curve is k(k+1)/2, with explicit byte saturation beyond its stored samples.</summary>
    public static byte Sample(ushort index) => index < StoredSampleCount
        ? (byte)(index * (index + 1) / 2)
        : byte.MaxValue;

    /// <summary>Only the low multiplier byte reaches WRMPYB; the result is an unsigned word.</summary>
    public static ushort Multiply(ushort index, ushort multiplier) => (ushort)(Sample(index) * (byte)multiplier);
}

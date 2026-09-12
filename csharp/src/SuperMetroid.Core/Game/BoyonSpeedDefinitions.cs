namespace SuperMetroid.Core.Game;

/// <summary>NTSC Boyon bounce curve and native eight-bit hardware multiplication.</summary>
public static class BoyonSpeedDefinitions
{
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

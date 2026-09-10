/// <summary>Explicit controller timelines for #446; no charge-state injection.</summary>
internal static class ShortChargeInputs
{
    /// <summary>Original-CPU storage distances for the specified initial animation phase.</summary>
    public static uint StoredDistance(int taps, int pattern)
    {
        ReadOnlySpan<uint> distances =
        [
            0x010c2000, 0x00f1b000, 0x00d97000, 0x00d25000, 0x00cfe000,
            0x00ec1000, 0x00d1a000, 0x00b96000, 0x00b24000, 0x00afd000,
            0x00e3c000, 0x00c95000, 0x00b11000, 0x00a9f000, 0x00a78000
        ];
        return distances[(taps - 2) * 5 + pattern];
    }

    public static ushort At(int frame, bool left, int taps, int pattern, int shift)
    {
        bool release = pattern switch
        {
            0 => false,
            1 => frame == 11, // 11_13
            2 => frame is 9 or 24, // 9_14_
            3 => frame is 5 or 13 or 24, // 5_7_10_
            4 => frame is 3 or 8 or 14 or 24, // 3_4_5_9_
            5 => frame == 1, // Too little initial momentum.
            6 => frame is >= 11 and <= 40, // Long enough to restart the run.
            _ => throw new ArgumentOutOfRangeException(nameof(pattern))
        };
        ReadOnlySpan<int> magic = [25, 50, 70, 85];
        bool dash = false;
        for (int tap = 0; tap < taps; tap++)
            dash |= frame == magic[tap] + shift || (tap == taps - 1 && frame >= magic[tap] + shift);
        return (ushort)((release ? 0 : left ? 0x200 : 0x100) | (dash ? 0x8000 : 0));
    }
}

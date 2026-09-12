namespace SuperMetroid.Core.Frontend;

/// <summary>Fixed egg-shell and slime trajectories, independent of cinematic artwork.</summary>
internal static class IntroEggMotionDefinitions
{
    /// <summary>$8B:A9EA: six signed 16.16 shell-fragment horizontal velocities.</summary>
    public static (ushort Whole, ushort Fraction) FragmentX(int index) => Split(index switch
    {
        0 => -0xc000, 1 => 0x4000, 2 => -0x8000,
        3 => -0xe000, 4 => 0x8000, 5 => 0x2000,
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    });

    /// <summary>
    /// $8B:AA02: 38 shell gravity records. Fragments zero/one also consume three
    /// adjacent instruction-byte pairs at $AA9A-$AAA5 before the signed ground
    /// test deletes them. Preserve this native overread as data, not executable code.
    /// </summary>
    public static (ushort Whole, ushort Fraction) FragmentY(int frame) => frame switch
    {
        38 => (0x9dad, 0x991b),
        39 => (0x1b7d, 0x97ad),
        40 => (0x991a, 0x1a7d),
        _ => Curve(frame, 38, 2),
    };

    /// <summary>$8B:AB35: five authored horizontal slime records; four actors are normally spawned.</summary>
    public static (ushort Whole, ushort Fraction) SlimeX(int index) => Split(index switch
    {
        0 => -0x10000, 1 => -0x8000, 2 => 0x10000, 3 => 0x8000, 4 => -0x8000,
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    });

    /// <summary>$8B:AB49/AC41: 62 odd / 69 even gravity records, selected by actor parity, not frame parity.</summary>
    public static (ushort Whole, ushort Fraction) SlimeY(int frame, bool odd) => Curve(frame, odd ? 62 : 69, odd ? 2 : 3);

    private static (ushort Whole, ushort Fraction) Curve(int frame, int count, int negativeGroups)
    {
        if ((uint)frame >= count)
            throw new ArgumentOutOfRangeException(nameof(frame));
        int prefix = negativeGroups * 7;
        if (frame >= prefix)
            return Split((frame - prefix) * 0x2000);
        int within = frame % 7;
        int fraction = within == 0 ? 0 : 0xe000 - within * 0x2000;
        return Split((frame / 7 - negativeGroups) * 0x10000 + fraction);
    }

    private static (ushort Whole, ushort Fraction) Split(int value) =>
        (unchecked((ushort)(value >> 16)), unchecked((ushort)value));
}

namespace SuperMetroid.Core.Frontend;

/// <summary>Fixed egg-shell and slime trajectories, independent of cinematic artwork.</summary>
internal static class IntroEggMotionDefinitions
{
    /// <summary>
    /// <c>$8B:A97C</c>, the six interleaved shell-fragment X/Y origins before the
    /// cartridge adds its fixed $10/$3B placement biases.
    /// </summary>
    public const int InitialPositionReferenceAddress = 0x8ba97c;

    /// <summary>Returns one shell fragment's final world-space spawn position.</summary>
    /// <remarks>
    /// Issues #625 and #975: $8B:A958 indexes six interleaved 16-bit X/Y
    /// origin pairs at $8B:A97C..A993 with index*4, then adds $10 and $3B
    /// respectively with native 16-bit wrap. Pinned NTSC J/U v1.0 ROM and
    /// bank_8B.asm give source pairs ($5C,$58), ($63,$58), ($59,$5D),
    /// ($60,$5B), ($66,$5E), and ($63,$60); all twelve words match this
    /// selector after the biases. The spawn opcode creates exactly indices
    /// 0..5, and unknown indices throw. The irregular positions compose the
    /// shell artwork, so retaining six explicit pairs is clearer than a
    /// generated sequence or polynomial encoding.
    /// </remarks>
    public static (ushort X, ushort Y) FragmentInitialPosition(int index) => index switch
    {
        0 => (0x006c, 0x0093),
        1 => (0x0073, 0x0093),
        2 => (0x0069, 0x0098),
        3 => (0x0070, 0x0096),
        4 => (0x0076, 0x0099),
        5 => (0x0073, 0x009b),
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };

    /// <summary>$8B:A9EA: six signed 16.16 shell-fragment horizontal velocities.</summary>
    /// <remarks>
    /// Issues #625 and #976: pinned NTSC J/U v1.0 ROM and bank_8B.asm
    /// match all six high-word-first velocity pairs at $8B:A9EA..AA01:
    /// FFFF:4000, 0000:4000, FFFF:8000, FFFF:2000, 0000:8000,
    /// 0000:2000. $8B:A994 indexes them by the immutable fragment number
    /// 0..5, scaled by four bytes; the caller adds the fraction before the
    /// signed whole word and carries across. The directions and fractional
    /// speeds vary irregularly by shell piece, so retain the six authored
    /// launch velocities. Unknown indices throw rather than read adjacent data.
    /// </remarks>
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

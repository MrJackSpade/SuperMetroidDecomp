namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The codec between Torizo's typed latches and the words that hold them at the spawn record and the egg projectile: the
/// posture latches in the top three bits of the authored parameter 1 (beside its independent low thirteen bits), the
/// behaviour latches in the top four bits of the authored parameter 2, and the launching posture an egg keeps in its
/// projectile variable 0. It is the one place that knows where each latch sits.
/// </summary>
[SnesEncodingOwner]
internal static class TorizoParameterFlagCodec
{
    /// <summary>The bits of parameter 1 below the posture bits.</summary>
    internal const int PostureLowBitsMask = (1 << 13) - 1;

    /// <summary>One latch and the parameter-word bit that holds it.</summary>
    private readonly record struct LatchBit<TFlags>(TFlags Flag, int Bit) where TFlags : struct, Enum;

    /// <summary>Each posture latch's bit in parameter 1.</summary>
    private static readonly LatchBit<TorizoPostureFlags>[] PostureBits =
    [
        new(TorizoPostureFlags.OppositeLegForward, 1 << 13),
        new(TorizoPostureFlags.TurningAround, 1 << 14),
        new(TorizoPostureFlags.FacingRight, 1 << 15),
    ];

    /// <summary>Each behaviour latch's bit in parameter 2; its other bits name no behaviour.</summary>
    private static readonly LatchBit<TorizoBehaviorFlags>[] BehaviorBits =
    [
        new(TorizoBehaviorFlags.CaughtSuperMissile, 1 << 12),
        new(TorizoBehaviorFlags.Stunned, 1 << 13),
        new(TorizoBehaviorFlags.FaceBlownUp, 1 << 14),
        new(TorizoBehaviorFlags.GutBlownUp, 1 << 15),
    ];

    /// <summary>The posture and the low bits beside it as one parameter word, the form an egg carries.</summary>
    internal static ushort ToParameterWord(TorizoPostureFlags posture, int lowBits)
    {
        int word = Ensure.BetweenInclusive(0, PostureLowBitsMask, lowBits);
        foreach ((TorizoPostureFlags flag, int bit) in PostureBits)
            if ((posture & flag) != 0)
                word |= bit;
        return checked((ushort)word);
    }

    /// <summary>The posture latches of an authored parameter-1 word.</summary>
    internal static TorizoPostureFlags PostureOf(int parameterWord)
    {
        TorizoPostureFlags posture = TorizoPostureFlags.None;
        foreach ((TorizoPostureFlags flag, int bit) in PostureBits)
            if ((parameterWord & bit) != 0)
                posture |= flag;
        return posture;
    }

    /// <summary>The low thirteen bits of an authored parameter-1 word.</summary>
    internal static int PostureLowBitsOf(int parameterWord) => parameterWord & PostureLowBitsMask;

    /// <summary>The behaviour latches of an authored parameter-2 word.</summary>
    internal static TorizoBehaviorFlags BehaviorOf(int parameterWord)
    {
        TorizoBehaviorFlags behavior = 0;
        foreach ((TorizoBehaviorFlags flag, int bit) in BehaviorBits)
            if ((parameterWord & bit) != 0)
                behavior |= flag;
        return behavior;
    }
}

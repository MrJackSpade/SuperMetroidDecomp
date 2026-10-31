namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Where Torizo's latches sit in its spawn parameter words, for checks and fixtures that author or read them: the
/// posture latches in the top three bits of parameter 1 and the behaviour latches in the top four bits of parameter 2.
/// </summary>
[EncodingOwner("Torizo spawn parameter latch words, as checks read them")]
internal static class TorizoParameterFlagWords
{
    /// <summary>One latch and the parameter-word bit that holds it.</summary>
    internal readonly record struct LatchBit<TFlags>(TFlags Flag, int Bit) where TFlags : struct, Enum;

    /// <summary>Each posture latch's bit in parameter 1.</summary>
    internal static readonly LatchBit<TorizoPostureFlags>[] PostureBits =
    [
        new(TorizoPostureFlags.OppositeLegForward, 1 << 13),
        new(TorizoPostureFlags.TurningAround, 1 << 14),
        new(TorizoPostureFlags.FacingRight, 1 << 15),
    ];

    /// <summary>Each behaviour latch's bit in parameter 2; its other bits name no behaviour.</summary>
    internal static readonly LatchBit<TorizoBehaviorFlags>[] BehaviorBits =
    [
        new(TorizoBehaviorFlags.CaughtSuperMissile, 1 << 12),
        new(TorizoBehaviorFlags.Stunned, 1 << 13),
        new(TorizoBehaviorFlags.FaceBlownUp, 1 << 14),
        new(TorizoBehaviorFlags.GutBlownUp, 1 << 15),
    ];

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
    internal static int PostureLowBitsOf(int parameterWord) => parameterWord & TorizoParameterFlagCodec.PostureLowBitsMask;

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

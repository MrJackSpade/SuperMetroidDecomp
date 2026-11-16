namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Where Torizo's latches sit in its spawn parameter words, for checks and fixtures that author or read them: the
/// posture latches in the top three bits of parameter 1 and the behaviour latches in the top four bits of parameter 2.
/// </summary>
[EncodingOwner("Torizo spawn parameter latch words, as checks read them")]
internal static class TorizoParameterFlagWords
{
    /// <summary>The bits of parameter 1 below the posture latches, which name no behaviour.</summary>
    internal const int PostureLowBitsMask = (1 << 13) - 1;

    /// <summary>One latch and the parameter-word bit that holds it.</summary>
    internal readonly record struct LatchBit<TFlags>(TFlags Flag, int Bit) where TFlags : struct, Enum;

    /// <summary>The parameter-1 bit set when the Torizo leads with the foot away from its facing side.</summary>
    internal const int FarFootLeadsBit = 1 << 13;

    /// <summary>The parameter-1 bit set while the Torizo turns around.</summary>
    internal const int TurningBit = 1 << 14;

    /// <summary>The parameter-1 bit set while the Torizo faces right.</summary>
    internal const int FacingRightBit = 1 << 15;

    /// <summary>Each behaviour latch's bit in parameter 2; its other bits name no behaviour.</summary>
    internal static readonly LatchBit<TorizoBehaviorFlags>[] BehaviorBits =
    [
        new(TorizoBehaviorFlags.CaughtSuperMissile, 1 << 12),
        new(TorizoBehaviorFlags.Stunned, 1 << 13),
        new(TorizoBehaviorFlags.FaceBlownUp, 1 << 14),
        new(TorizoBehaviorFlags.GutBlownUp, 1 << 15),
    ];

    /// <summary>The posture of an authored parameter-1 word (#2237).</summary>
    internal static TorizoPosture PostureOf(int parameterWord)
    {
        HorizontalDirection facing = (parameterWord & FacingRightBit) != 0 ? HorizontalDirection.Right : HorizontalDirection.Left;
        HorizontalDirection farFoot = facing == HorizontalDirection.Right ? HorizontalDirection.Left : HorizontalDirection.Right;
        return new TorizoPosture(facing, (parameterWord & FarFootLeadsBit) != 0 ? farFoot : facing) { IsTurning = (parameterWord & TurningBit) != 0 };
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

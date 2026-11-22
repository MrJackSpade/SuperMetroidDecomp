namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Where Torizo's posture latches sit in its spawn parameter words, for checks and fixtures that author or read them: the
/// top three bits of parameter 1.
/// </summary>
internal static class TorizoParameterFlagWords
{
    /// <summary>The bits of parameter 1 below the posture latches, which name no behaviour.</summary>
    internal const int PostureLowBitsMask = (1 << 13) - 1;

    /// <summary>The parameter-1 bit set when the Torizo leads with the foot away from its facing side.</summary>
    internal const int FarFootLeadsBit = 1 << 13;

    /// <summary>The parameter-1 bit set while the Torizo turns around.</summary>
    internal const int TurningBit = 1 << 14;

    /// <summary>The parameter-1 bit set while the Torizo faces right.</summary>
    internal const int FacingRightBit = 1 << 15;

    /// <summary>The posture of an authored parameter-1 word (#2237).</summary>
    internal static TorizoPosture PostureOf(int parameterWord)
    {
        HorizontalDirection facing = (parameterWord & FacingRightBit) != 0 ? HorizontalDirection.Right : HorizontalDirection.Left;
        HorizontalDirection farFoot = facing == HorizontalDirection.Right ? HorizontalDirection.Left : HorizontalDirection.Right;
        TorizoPosture posture = TorizoPosture.AfterStep(facing, (parameterWord & FarFootLeadsBit) != 0 ? farFoot : facing);
        return (parameterWord & TurningBit) != 0 ? posture.TurningAround : posture;
    }

    /// <summary>The low thirteen bits of an authored parameter-1 word.</summary>
    internal static int PostureLowBitsOf(int parameterWord) => parameterWord & PostureLowBitsMask;
}

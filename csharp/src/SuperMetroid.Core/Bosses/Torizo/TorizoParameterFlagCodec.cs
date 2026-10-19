namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The codec between Torizo's typed flags and the words that hold them at the spawn record and the egg projectile: the
/// posture bits in the top three bits of the authored parameter 1 (beside its independent low thirteen bits), the
/// behaviour latches in the authored parameter 2, and the launching posture an egg keeps in its projectile variable 0.
/// </summary>
[SnesEncodingOwner]
internal static class TorizoParameterFlagCodec
{
    /// <summary>The bits of parameter 1 below the posture bits.</summary>
    internal const int PostureLowBitsMask = 0x1FFF;

    /// <summary>The posture bits as the word that stores them.</summary>
    internal static ushort ToNativeWord(this TorizoPostureFlags posture) => (ushort)posture;

    /// <summary>The posture and the low bits beside it as one parameter word, the form an egg carries.</summary>
    internal static ushort ToParameterWord(TorizoPostureFlags posture, int lowBits) =>
        checked((ushort)(lowBits | (ushort)posture));

    /// <summary>The posture bits of an authored parameter-1 word.</summary>
    internal static TorizoPostureFlags PostureOf(int parameterWord) => (TorizoPostureFlags)parameterWord & TorizoPostureFlags.All;

    /// <summary>The low thirteen bits of an authored parameter-1 word.</summary>
    internal static int PostureLowBitsOf(int parameterWord) => parameterWord & PostureLowBitsMask;

    /// <summary>The behaviour latches of an authored parameter-2 word; its other bits name no behaviour.</summary>
    internal static TorizoBehaviorFlags BehaviorOf(int parameterWord) =>
        (TorizoBehaviorFlags)parameterWord & (TorizoBehaviorFlags.CaughtSuperMissile | TorizoBehaviorFlags.Stunned |
            TorizoBehaviorFlags.FaceBlownUp | TorizoBehaviorFlags.GutBlownUp);
}

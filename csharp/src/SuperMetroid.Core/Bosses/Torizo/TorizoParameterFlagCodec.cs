namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The codec between Torizo's typed flags and the enemy-slot words that still hold them: the posture bits in the top
/// three bits of the body's parameter 1 (beside its independent low thirteen bits), the behaviour latches in its
/// parameter 2, and the launching posture an egg keeps in its projectile variable 0. Those slot words stay exact
/// because shared enemy code reads slot memory by offset. This type is the one place either flag set becomes one of
/// those words; the destination is Torizo-owned typed state holding the flags directly, which removes this codec.
/// </summary>
[SnesEncodingOwner]
internal static class TorizoParameterFlagCodec
{
    /// <summary>The posture bits as the slot word that stores them.</summary>
    internal static ushort ToNativeWord(this TorizoPostureFlags posture) => (ushort)posture;

    /// <summary>The behaviour latches as the slot word that stores them.</summary>
    internal static ushort ToNativeWord(this TorizoBehaviorFlags behavior) => (ushort)behavior;
}

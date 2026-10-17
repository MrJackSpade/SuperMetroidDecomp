namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The codec between <see cref="ZebetiteGenerationFlags"/> and the Zebetite slot word that still holds them. That
/// slot word stays exact because shared enemy code reads slot memory by offset; this type is the one place the flags
/// become that word. The destination is Zebetite-owned typed state holding the flags directly, which removes this
/// codec.
/// </summary>
[SnesEncodingOwner]
internal static class ZebetiteGenerationFlagsCodec
{
    /// <summary>The generation flags as the slot word that stores them.</summary>
    internal static ushort ToNativeWord(this ZebetiteGenerationFlags flags) => (ushort)flags;
}

using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Verification access to <see cref="SamusBeamLoadoutWord"/> members production does not use.</summary>
internal static class SamusBeamLoadoutWordAccess
{
    /// <summary>Low four bits of the beam word select the combination.</summary>
    private const ushort CombinationMask = 0x000f;

    /// <summary>Replaces only the low four-bit beam combination while preserving Charge and other raw bits.</summary>
    extension(SamusBeamLoadoutWord self)
    {
        /// <summary>
        /// Replaces the four-bit retail combination index while preserving Charge and every
        /// other raw bit. Debug menus use this to emulate changing only the selected beams.
        /// </summary>
        internal ushort WithCombinationIndex(int combinationIndex)
        {
            if ((uint)combinationIndex > CombinationMask)
                throw new ArgumentOutOfRangeException(nameof(combinationIndex));
            return (ushort)((self.Beams.ToNativeWord() & ~CombinationMask) | combinationIndex);
        }
    }
}

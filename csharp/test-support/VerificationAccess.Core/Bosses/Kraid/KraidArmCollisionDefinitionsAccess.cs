
namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Verification access to <see cref="KraidArmCollisionDefinitions"/> members production does not use.</summary>
internal static class KraidArmCollisionDefinitionsAccess
{
    /// <summary>A single-component frame: a two-byte count and one eight-byte hitbox.</summary>
    private const int SingleComponentFrameBytes = 2 + 8;

    /// <summary>Calculates Kraid arm collision frame roots from the native record stride.</summary>
    extension(KraidArmCollisionDefinitions)
    {
        /// <summary>Calculates the ordered physical frame roots from native component-record sizes.</summary>
        /// <remarks>Independently reviewed for #1165 against bank_A7.asm and every original
        /// frame count. General and rising/sinking groups have ten five-component frames
        /// each; the final two frames have one component. No pointer roster is stored.</remarks>
        internal static ushort FramePointer(int index)
        {
            if ((uint)index >= KraidArmCollisionDefinitions.FrameCount) throw new ArgumentOutOfRangeException(nameof(index));
            return (ushort)(index < 20 ? KraidArmCollisionDefinitions.FirstGeneralFrame + KraidArmCollisionDefinitions.GeneralFrameBytes * index
                : KraidArmCollisionDefinitionsConstants.FirstSingleComponentFrame + SingleComponentFrameBytes * (index - 20));
        }
    }
}

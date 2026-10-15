namespace SuperMetroid.Core.Hardware;

/// <summary>Verification access to <see cref="NativeWordCounter"/> members production does not use.</summary>
internal static class NativeWordCounterAccess
{
    /// <summary>Supplies a saturating timer decrement for native counter assertions.</summary>
    extension(NativeWordCounter)
    {
        /// <summary>
        /// Decrements a nonzero word and leaves zero saturated, matching the common
        /// <c>LDA timer / BEQ done / DEC timer</c> pattern rather than unconditional DEC.
        /// </summary>
        internal static ushort DecrementSaturating(ushort value) =>
            value == 0 ? (ushort)0 : unchecked((ushort)(value - 1));
    }
}

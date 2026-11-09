namespace SuperMetroid.Core.Compatibility;

/// <summary>Verification access to <see cref="LowHalfCountdownStep"/> members production does not use.</summary>
internal static class LowHalfCountdownStepAccess
{
    /// <summary>Exposes <see cref="LowHalfCountdownStep"/> state that only verification reads.</summary>
    extension(LowHalfCountdownStep self)
    {
        /// <summary>Whether the eight-bit count has not run past zero.</summary>
        public bool IsNonNegative => !self.IsNegative;

        /// <summary>The eight-bit count after the step.</summary>
        public byte Low => self.Remaining.EightBitCount;
    }
}

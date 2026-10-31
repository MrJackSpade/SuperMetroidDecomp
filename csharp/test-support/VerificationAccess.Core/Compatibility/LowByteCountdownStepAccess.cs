namespace SuperMetroid.Core.Compatibility;

/// <summary>Verification access to <see cref="LowByteCountdownStep"/> members production does not use.</summary>
internal static class LowByteCountdownStepAccess
{
    /// <summary>Exposes <see cref="LowByteCountdownStep"/> state that only verification reads.</summary>
    extension(LowByteCountdownStep self)
    {
        /// <summary>Whether the eight-bit count has not run past zero.</summary>
        public bool IsNonNegative => !self.IsNegative;

        /// <summary>The eight-bit count after the step.</summary>
        public byte Low => self.Remaining.EightBitCount;
    }
}

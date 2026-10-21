namespace SuperMetroid.Core.Compatibility;

/// <summary>Verification access to <see cref="LowByteCountdownStep"/> members production does not use.</summary>
internal static class LowByteCountdownStepAccess
{
    /// <summary>Exposes <see cref="LowByteCountdownStep"/> state that only verification reads.</summary>
    extension(LowByteCountdownStep self)
    {
        /// <summary>Whether the result would take an 8-bit BPL branch.</summary>
        public bool IsNonNegative => !self.IsNegative;
    }
}

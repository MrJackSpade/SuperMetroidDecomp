namespace SuperMetroid.Core.Hardware;

/// <summary>Verification access to <see cref="NativeLowByteCounterStep"/> members production does not use.</summary>
internal static class NativeLowByteCounterStepAccess
{
    /// <summary>Exposes <see cref="NativeLowByteCounterStep"/> state that only verification reads.</summary>
    extension(NativeLowByteCounterStep self)
    {
        /// <summary>Whether the result would take an 8-bit BPL branch.</summary>
        public bool IsNonNegative => !self.IsNegative;
    }
}

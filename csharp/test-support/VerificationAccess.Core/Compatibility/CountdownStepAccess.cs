namespace SuperMetroid.Core.Compatibility;

/// <summary>Verification access to <see cref="CountdownStep"/> members production does not use.</summary>
internal static class CountdownStepAccess
{
    /// <summary>Exposes the wrap result of a single countdown step.</summary>
    extension(CountdownStep self)
    {
        /// <summary>The countdown left after the step, as the original's timer word.</summary>
        internal ushort Value => self.Remaining.RawValue;

        /// <summary>Whether stepping zero wrapped to 65535.</summary>
        internal bool Underflowed => self.Remaining.RawValue == ushort.MaxValue;
    }
}

namespace SuperMetroid.Core.Compatibility;

/// <summary>Verification access to <see cref="CountdownStep"/> members production does not use.</summary>
internal static class CountdownStepAccess2
{
    /// <summary>Exposes <see cref="CountdownStep"/> state that only verification reads.</summary>
    extension(CountdownStep self)
    {
        /// <summary>Whether the countdown has not yet run past zero: a timer that ends on running past zero is still waiting.</summary>
        public bool IsNonNegative => !self.IsNegative;
    }
}

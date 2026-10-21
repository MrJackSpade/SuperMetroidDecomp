namespace SuperMetroid.Core.Compatibility;

/// <summary>Verification access to <see cref="CountdownStep"/> members production does not use.</summary>
internal static class CountdownStepAccess
{
    /// <summary>Exposes the wrap result of a single native word counter step.</summary>
    extension(CountdownStep self)
    {
        /// <summary>Whether decrementing zero wrapped through <c>$FFFF</c>.</summary>
        internal bool Underflowed => self.Value == ushort.MaxValue;
    }
}

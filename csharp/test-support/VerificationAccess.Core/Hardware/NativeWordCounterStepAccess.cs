namespace SuperMetroid.Core.Hardware;

/// <summary>Verification access to <see cref="NativeWordCounterStep"/> members production does not use.</summary>
internal static class NativeWordCounterStepAccess
{
    /// <summary>Exposes the wrap result of a single native word counter step.</summary>
    extension(NativeWordCounterStep self)
    {
        /// <summary>Whether decrementing zero wrapped through <c>$FFFF</c>.</summary>
        internal bool Underflowed => self.Value == ushort.MaxValue;
    }
}

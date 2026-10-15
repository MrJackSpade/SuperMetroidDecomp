namespace SuperMetroid.Core.Hardware;

/// <summary>Verification access to <see cref="FixedColorMemory"/> members production does not use.</summary>
internal static class FixedColorMemoryAccess
{
    /// <summary>Exposes <see cref="FixedColorMemory"/> state that only verification reads.</summary>
    extension(FixedColorMemory self)
    {
        /// <summary>The five-bit intensity of <see cref="FixedColorMemory.Red"/>.</summary>
        public byte RedIntensity => (byte)(self.Red & 31);
    }
}

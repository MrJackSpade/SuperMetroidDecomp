using SuperMetroid.Core.Hardware;

/// <summary>Verification access to <see cref="SnesAngle"/> members production does not use.</summary>
internal static class SnesAngleAccess
{
    /// <summary>Exposes table offsets and signed angular deltas matching native byte arithmetic.</summary>
    extension(SnesAngle self)
    {
        /// <summary>The byte offset for a table containing one 16-bit sample per angle.</summary>
        internal int SineTableByteOffset => self.TableIndex * sizeof(ushort);

        /// <summary>
        /// Returns the native signed shortest delta from this angle to <paramref name="target"/>.
        /// The exactly-opposite case is <see cref="short.MinValue"/>, matching 65816 subtraction.
        /// </summary>
        internal short SignedDeltaTo(SnesAngle target) =>
            unchecked((short)(target.RawValue - self.RawValue));

        /// <summary>
        /// Returns the signed high-byte delta used by byte-angle dispatchers. The opposite
        /// direction is <see cref="sbyte.MinValue"/>, matching native eight-bit truncation.
        /// </summary>
        internal sbyte SignedTableDeltaTo(SnesAngle target) =>
            unchecked((sbyte)(target.TableIndex - self.TableIndex));
    }
}

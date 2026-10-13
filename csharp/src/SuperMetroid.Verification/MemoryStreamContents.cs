/// <summary>The written bytes of a verification <see cref="MemoryStream"/>, viewed without a copy (#624).</summary>
internal static class MemoryStreamContents
{
    /// <summary>Views of one expandable memory stream's contents.</summary>
    extension(MemoryStream stream)
    {
        /// <summary>
        /// The bytes written so far, borrowed from the stream's buffer; valid until the stream is written again.
        /// A stream over a caller's fixed array does not expose its buffer and fails here.
        /// </summary>
        public ReadOnlySpan<byte> WrittenBytes() => stream.TryGetBuffer(out ArraySegment<byte> buffer)
            ? buffer.AsSpan()
            : throw new InvalidOperationException("This memory stream does not expose its buffer; it wraps a caller's array.");
    }
}

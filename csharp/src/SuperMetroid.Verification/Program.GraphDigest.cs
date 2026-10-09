using System.Security.Cryptography;
using SuperMetroid.Desktop;

internal static partial class Program
{
    /// <summary>
    /// SHA-256 of an object's complete debugger-state graph, for whole-state equality checks.
    /// Streams into the hash so comparing large graphs allocates no serialized copy.
    /// </summary>
    private static byte[] GraphDigest(object value)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        using (var stream = new HashingStream(hash))
            DebuggerObjectGraphSerializer.Serialize(stream, value);
        return hash.GetHashAndReset();
    }

    /// <summary>The serialized debugger-state graph, for tests that restore it.</summary>
    private static byte[] SerializeGraph(object value)
    {
        using var stream = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(stream, value);
        return stream.ToArray();
    }

    /// <summary>Write-only stream that appends serialized bytes directly to an incremental digest.</summary>
    /// <param name="hash">Digest updated by each write operation.</param>
    private sealed class HashingStream(IncrementalHash hash) : Stream
    {
        /// <summary>Always returns false because serialized data is not readable from the digest sink.</summary>
        public override bool CanRead => false;
        /// <summary>Always returns false because the digest sink has no seekable position.</summary>
        public override bool CanSeek => false;
        /// <summary>Always returns true; writes append bytes to the associated digest.</summary>
        public override bool CanWrite => true;
        /// <summary>Not supported because the stream does not retain serialized bytes.</summary>
        /// <exception cref="NotSupportedException">The digest sink has no stored length.</exception>
        public override long Length => throw new NotSupportedException();
        /// <summary>Not supported because the stream does not retain or address serialized bytes.</summary>
        /// <exception cref="NotSupportedException">The digest sink has no seek position.</exception>
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        /// <summary>Completes the no-op flush for this immediate digest sink.</summary>
        public override void Flush() { }
        /// <summary>Rejects read attempts because the stream only feeds bytes into a hash.</summary>
        /// <param name="buffer">Destination buffer, unused because reads are unsupported.</param>
        /// <param name="offset">Destination offset, unused because reads are unsupported.</param>
        /// <param name="count">Requested byte count, unused because reads are unsupported.</param>
        /// <exception cref="NotSupportedException">The stream is write-only.</exception>
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        /// <summary>Rejects seek attempts because prior serialized bytes are not stored.</summary>
        /// <param name="offset">Requested seek offset, unused because seeking is unsupported.</param>
        /// <param name="origin">Requested seek origin, unused because seeking is unsupported.</param>
        /// <exception cref="NotSupportedException">The stream is not seekable.</exception>
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        /// <summary>Rejects resizing because the stream retains no serialized byte sequence.</summary>
        /// <param name="value">Requested length, unused because resizing is unsupported.</param>
        /// <exception cref="NotSupportedException">The digest sink cannot be resized.</exception>
        public override void SetLength(long value) => throw new NotSupportedException();
        /// <summary>Appends the selected byte range to the running digest.</summary>
        /// <param name="buffer">Source bytes to hash.</param>
        /// <param name="offset">First source byte to append.</param>
        /// <param name="count">Number of bytes to append.</param>
        public override void Write(byte[] buffer, int offset, int count) => hash.AppendData(buffer, offset, count);
        /// <summary>Appends the supplied bytes to the running digest.</summary>
        /// <param name="buffer">Serialized byte span to hash.</param>
        public override void Write(ReadOnlySpan<byte> buffer) => hash.AppendData(buffer);
    }
}

internal static partial class Program
{
    /// <summary>
    /// True when every instance field of two objects of one type holds equal state: arrays of
    /// unmanaged elements by content, everything else by <see cref="object.Equals(object?, object?)"/>.
    /// Reflection covers fields added later, so a restore that forgets one fails here.
    /// </summary>
    private static bool InstanceStateEquals<T>(T actual, T expected) where T : class
    {
        for (Type? type = typeof(T); type is not null; type = type.BaseType)
            foreach (var field in type.GetFields(System.Reflection.BindingFlags.Instance |
                         System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
                         System.Reflection.BindingFlags.DeclaredOnly))
            {
                object? left = field.GetValue(actual), right = field.GetValue(expected);
                if (left is Array leftArray && right is Array rightArray)
                {
                    if (leftArray.GetType() != rightArray.GetType() || !leftArray.GetType().GetElementType()!.IsPrimitive)
                        throw new InvalidOperationException($"{type.Name}.{field.Name} needs an element comparison.");
                    if (!System.Runtime.InteropServices.MemoryMarshal.CreateReadOnlySpan(
                            ref System.Runtime.InteropServices.MemoryMarshal.GetArrayDataReference(leftArray), Buffer.ByteLength(leftArray))
                        .SequenceEqual(System.Runtime.InteropServices.MemoryMarshal.CreateReadOnlySpan(
                            ref System.Runtime.InteropServices.MemoryMarshal.GetArrayDataReference(rightArray), Buffer.ByteLength(rightArray))))
                        return false;
                }
                else if (!Equals(left, right)) return false;
            }
        return true;
    }
}

internal static partial class Program
{
    // Raster for a packet render consumed immediately by one comparison. Never keep the
    // returned array, and never compare two of these renders with each other.
    /// <summary>Reusable output buffer for a single render consumed immediately by a comparison.</summary>
    private static readonly SuperMetroid.Core.Assets.Rgba32[] comparisonRaster =
        new SuperMetroid.Core.Assets.Rgba32[SuperMetroid.Core.Hardware.SnesPpuLayout.ScreenWidthPixels *
            SuperMetroid.Core.Hardware.SnesPpuLayout.ScreenHeightPixels];

    /// <summary>Renders a frame snapshot into the shared comparison raster.</summary>
    /// <param name="frame">Frame snapshot to render for a one-shot comparison.</param>
    /// <returns>The shared raster buffer, valid for immediate comparison before its next render.</returns>
    private static SuperMetroid.Core.Assets.Rgba32[] RenderForComparison(SuperMetroid.Core.Rendering.RenderFrameSnapshot frame) =>
        SuperMetroid.Core.Rendering.SoftwareFrameSnapshotRenderer.Render(frame, comparisonRaster);

    /// <summary>Renders a layered snapshot into the shared comparison raster.</summary>
    /// <param name="frame">Layered snapshot to render for a one-shot comparison.</param>
    /// <returns>The shared raster buffer, valid for immediate comparison before its next render.</returns>
    private static SuperMetroid.Core.Assets.Rgba32[] RenderForComparison(SuperMetroid.Core.Rendering.LayeredRenderSnapshot frame) =>
        SuperMetroid.Core.Rendering.SoftwareLayeredSnapshotRenderer.Render(frame, comparisonRaster);
}

internal static partial class Program
{
    /// <summary>SHA-256 of a render packet's encoding, streamed without a serialized copy.</summary>
    private static byte[] PacketDigest(SuperMetroid.Core.Rendering.RenderFrameSnapshot frame)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        using (var stream = new HashingStream(hash))
            SuperMetroid.Core.Rendering.RenderFrameSnapshotCodec.Serialize(frame, stream);
        return hash.GetHashAndReset();
    }
}

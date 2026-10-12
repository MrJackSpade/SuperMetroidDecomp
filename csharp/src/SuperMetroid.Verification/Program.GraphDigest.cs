using System.Security.Cryptography;
using SuperMetroid.Desktop;

/// <summary>Represents graph digest verification.</summary>
internal static class GraphDigestVerification
{
    /// <summary>
    /// SHA-256 of an object's complete debugger-state graph, for whole-state equality checks.
    /// Streams into the hash so comparing large graphs allocates no serialized copy.
    /// </summary>
    internal static byte[] GraphDigest(object value)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        using (var stream = new HashingStream(hash))
            DebuggerObjectGraphSerializer.Serialize(stream, value);
        return hash.GetHashAndReset();
    }

    /// <summary>The serialized debugger-state graph, for tests that restore it.</summary>
    internal static byte[] SerializeGraph(object value)
    {
        using var stream = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(stream, value);
        return stream.ToArray();
    }

    /// <summary>Represents hashing stream.</summary>
    internal sealed class HashingStream(IncrementalHash hash) : Stream
    {
        /// <summary>Gets can read.</summary>
        public override bool CanRead => false;
        /// <summary>Gets can seek.</summary>
        public override bool CanSeek => false;
        /// <summary>Gets can write.</summary>
        public override bool CanWrite => true;
        /// <summary>Rejects length queries on the write-only hashing stream.</summary>
        public override long Length => throw new NotSupportedException();
        /// <summary>Rejects position queries and assignments on the write-only hashing stream.</summary>
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        /// <summary>Flush.</summary>
        public override void Flush() { }
        /// <summary>Rejects reads from the write-only hashing stream.</summary>
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        /// <summary>Seek.</summary>
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        /// <summary>Sets length.</summary>
        public override void SetLength(long value) => throw new NotSupportedException();
        /// <summary>Appends a byte array segment to the graph digest hash.</summary>
        public override void Write(byte[] buffer, int offset, int count) => hash.AppendData(buffer, offset, count);
        /// <summary>Appends a byte span to the graph digest hash.</summary>
        public override void Write(ReadOnlySpan<byte> buffer) => hash.AppendData(buffer);
    }
    /// <summary>
    /// True when every instance field of two objects of one type holds equal state: arrays of
    /// unmanaged elements by content, everything else by <see cref="object.Equals(object?, object?)"/>.
    /// Reflection covers fields added later, so a restore that forgets one fails here.
    /// </summary>
    internal static bool InstanceStateEquals<T>(T actual, T expected) where T : class
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
    // Raster for a packet render consumed immediately by one comparison. Never keep the
    // returned array, and never compare two of these renders with each other.
    /// <summary>Stores comparison raster.</summary>
    private static readonly SuperMetroid.Core.Assets.Rgba32[] comparisonRaster =
        new SuperMetroid.Core.Assets.Rgba32[SuperMetroid.Core.Hardware.SnesPpuLayout.ScreenWidthPixels *
            SuperMetroid.Core.Hardware.SnesPpuLayout.ScreenHeightPixels];

    /// <summary>Render for comparison.</summary>
    internal static SuperMetroid.Core.Assets.Rgba32[] RenderForComparison(SuperMetroid.Core.Rendering.RenderFrameSnapshot frame) =>
        SuperMetroid.Core.Rendering.SoftwareFrameSnapshotRenderer.Render(frame, comparisonRaster);

    /// <summary>Render for comparison.</summary>
    internal static SuperMetroid.Core.Assets.Rgba32[] RenderForComparison(SuperMetroid.Core.Rendering.LayeredRenderSnapshot frame) =>
        SuperMetroid.Core.Rendering.SoftwareLayeredSnapshotRenderer.Render(frame, comparisonRaster);
    /// <summary>SHA-256 of a render packet's encoding, streamed without a serialized copy.</summary>
    internal static byte[] PacketDigest(SuperMetroid.Core.Rendering.RenderFrameSnapshot frame)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        using (var stream = new HashingStream(hash))
            SuperMetroid.Core.Rendering.RenderFrameSnapshotWriter.Serialize(frame, stream);
        return hash.GetHashAndReset();
    }
}

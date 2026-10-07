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

    private sealed class HashingStream(IncrementalHash hash) : Stream
    {
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => hash.AppendData(buffer, offset, count);
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
    private static readonly SuperMetroid.Core.Assets.Rgba32[] comparisonRaster =
        new SuperMetroid.Core.Assets.Rgba32[SuperMetroid.Core.Hardware.SnesPpuLayout.ScreenWidthPixels *
            SuperMetroid.Core.Hardware.SnesPpuLayout.ScreenHeightPixels];

    private static SuperMetroid.Core.Assets.Rgba32[] RenderForComparison(SuperMetroid.Core.Rendering.RenderFrameSnapshot frame) =>
        SuperMetroid.Core.Rendering.SoftwareFrameSnapshotRenderer.Render(frame, comparisonRaster);

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

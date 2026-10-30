using SuperMetroid.Core.Assets;
using SuperMetroid.Core;
using System.Security.Cryptography;
using SuperMetroid.Diagnostics.SaveStates;
using SuperMetroid.TestSupport.FrameRendering;
namespace SuperMetroid.Verification.Harness;

/// <summary>Provides streamed debugger-graph hashes and temporary render rasters for focused state comparisons.</summary>
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

    /// <summary>Write-only stream that feeds serialized graph bytes directly into an incremental SHA-256 hash.</summary>
    internal sealed class HashingStream(IncrementalHash hash) : Stream
    {
        /// <summary>Hashing accepts writes only.</summary>
        public override bool CanRead => false;
        /// <summary>Serialized bytes cannot be repositioned in the hash.</summary>
        public override bool CanSeek => false;
        /// <summary>Serialized bytes are appended to the hash.</summary>
        public override bool CanWrite => true;
        /// <summary>A streamed hash has no retrievable byte length.</summary>
        public override long Length => throw new NotSupportedException();
        /// <summary>A streamed hash has no seekable position.</summary>
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        /// <summary>Hash updates are immediate and need no buffered flush.</summary>
        public override void Flush() { }
        /// <summary>Rejects reads from the write-only digest stream.</summary>
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        /// <summary>Rejects repositioning serialized input within the digest.</summary>
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        /// <summary>Rejects changing a digest stream's length after bytes were hashed.</summary>
        public override void SetLength(long value) => throw new NotSupportedException();
        /// <summary>Appends an array segment directly to the digest.</summary>
        public override void Write(byte[] buffer, int offset, int count) => hash.AppendData(buffer, offset, count);
        /// <summary>Appends a span directly to the digest.</summary>
        public override void Write(ReadOnlySpan<byte> buffer) => hash.AppendData(buffer);
    }
    /// <summary>
    /// True when every instance field of two objects of one type holds equal state: arrays of
    /// unmanaged elements by content, arrays of other value types element by element, an owned Core object by its own
    /// fields, everything else by <see cref="object.Equals(object?, object?)"/>.
    /// Reflection covers fields added later, so a restore that forgets one fails here.
    /// </summary>
    [ReflectionAudit("structural equality: every instance field is compared, so a restore that forgets a field added later fails")]
    internal static bool InstanceStateEquals<T>(T actual, T expected) where T : class =>
        FieldsEqual(typeof(T), actual, expected);

    /// <summary>Compares every instance field <paramref name="declared"/> and its bases declare.</summary>
    [ReflectionAudit("structural equality: every instance field is compared, so a restore that forgets a field added later fails")]
    private static bool FieldsEqual(Type declared, object actual, object expected)
    {
        for (Type? type = declared; type is not null; type = type.BaseType)
            foreach (var field in type.GetFields(System.Reflection.BindingFlags.Instance |
                         System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
                         System.Reflection.BindingFlags.DeclaredOnly))
            {
                object? left = field.GetValue(actual), right = field.GetValue(expected);
                if (left is Array leftArray && right is Array rightArray)
                {
                    Type element = leftArray.GetType().GetElementType()
                        ?? throw new InvalidOperationException("Verification requires leftArray.GetType().GetElementType().");
                    if (leftArray.GetType() != rightArray.GetType() || !element.IsValueType)
                        throw new InvalidOperationException($"{type.Name}.{field.Name} needs an element comparison.");
                    if (leftArray.Length != rightArray.Length)
                        return false;
                    if (element.IsPrimitive)
                    {
                        if (!System.Runtime.InteropServices.MemoryMarshal.CreateReadOnlySpan(
                                ref System.Runtime.InteropServices.MemoryMarshal.GetArrayDataReference(leftArray), Buffer.ByteLength(leftArray))
                            .SequenceEqual(System.Runtime.InteropServices.MemoryMarshal.CreateReadOnlySpan(
                                ref System.Runtime.InteropServices.MemoryMarshal.GetArrayDataReference(rightArray), Buffer.ByteLength(rightArray))))
                            return false;
                    }
                    else
                    {
                        // Value elements (typed blocks, behaviours) compare by their own equality, one by one.
                        for (int index = 0; index < leftArray.Length; index++)
                            if (!Equals(leftArray.GetValue(index), rightArray.GetValue(index)))
                                return false;
                    }
                }
                else if (left is not null && right is not null && !Equals(left, right) && left.GetType() == right.GetType() &&
                         !left.GetType().IsValueType && left.GetType().Assembly == typeof(Ensure).Assembly)
                {
                    // An object the state owns (the terrain past a room's grid) is compared by its own fields.
                    if (!FieldsEqual(left.GetType(), left, right))
                        return false;
                }
                else if (!Equals(left, right)) return false;
            }
        return true;
    }
    /// <summary>The pixels a packet shows, for a comparison.</summary>
    internal static RgbaPixel[] RenderForComparison(SuperMetroid.Core.Rendering.DisplayPacket frame) =>
        DisplayRendering.Render(frame);

    /// <summary>The pixels a layered scene shows, for a comparison.</summary>
    internal static RgbaPixel[] RenderForComparison(SuperMetroid.Core.Rendering.LayeredScene frame) =>
        DisplayRendering.Render(frame);
}

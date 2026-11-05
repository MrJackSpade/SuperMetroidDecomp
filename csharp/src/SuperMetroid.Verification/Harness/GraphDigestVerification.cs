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
    /// Streams into the hash so comparing large graphs allocates no serialized copy. The graph names what it reaches of
    /// <paramref name="content"/> by path, as a host's store names its bound content, so graphs bound to different
    /// content of the same shape digest alike. Content loaded since the list was first indexed is indexed before each
    /// capture, so what a graph names by path does not depend on which suites ran before it in the process.
    /// </summary>
    internal static byte[] GraphDigest(object value, params object[] content)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        using (var stream = new HashingStream(hash))
            DebuggerObjectGraphSerializer.Serialize(stream, value, Bound(content));
        return hash.GetHashAndReset();
    }

    /// <summary>
    /// The serialized debugger-state graph, naming what it reaches of <paramref name="content"/> by path, content loaded
    /// since the list was first indexed included.
    /// </summary>
    internal static byte[] SerializeGraph(object value, params object[] content)
    {
        using var stream = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(stream, value, Bound(content));
        return stream.ToArray();
    }

    /// <summary>Captures <paramref name="value"/> and restores it bound to <paramref name="content"/>, as a host's store does.</summary>
    internal static T RoundTrip<T>(T value, params object[] content) where T : class =>
        RestoreGraph<T>(SerializeGraph(value, content), content);

    /// <summary>Restores a serialized graph bound to <paramref name="content"/>, as a host's store does.</summary>
    internal static T RestoreGraph<T>(byte[] graph, params object[] content) where T : class
    {
        using var stream = new MemoryStream(graph);
        return DebuggerObjectGraphSerializer.Deserialize<T>(stream, Bound(content));
    }

    /// <summary>
    /// The bound objects a capture names by path: <paramref name="content"/>, or the fixture's installed content when
    /// none is given, as a host's store always binds its content. Each list of the same objects in the same order is
    /// indexed once, so repeated captures do not walk its content again.
    /// </summary>
    private static BoundObjectGraph Bound(object[] content)
    {
        if (content.Length == 0)
            content = SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.BoundContent;
        lock (boundGraphs)
        {
            foreach ((object[] roots, BoundObjectGraph graph) in boundGraphs)
                if (roots.Length == content.Length && roots.Zip(content).All(pair => ReferenceEquals(pair.First, pair.Second)))
                {
                    // Content loaded since the list was indexed (a fixture catalog first used by a later suite in this
                    // process) is indexed now, so a graph names it by path whatever order the suites ran in. An object
                    // keeps the path it was first indexed under.
                    graph.Refresh();
                    return graph;
                }
            var indexed = new BoundObjectGraph(content);
            boundGraphs.Add(([.. content], indexed));
            return indexed;
        }
    }

    /// <summary>Each content list indexed so far, with its graph.</summary>
    private static readonly List<(object[] Roots, BoundObjectGraph Graph)> boundGraphs = [];

    /// <summary>
    /// Write-only stream that feeds serialized graph bytes into an incremental SHA-256 hash. The serializer writes a graph
    /// as many writes of a few bytes each (a marker, a length, a name, one primitive), and every append to the hash is a
    /// call into the platform's hash provider, so the bytes are gathered into a buffer and appended a buffer at a time
    /// (#1532). The hash sees the same bytes in the same order, so the digest is unchanged.
    /// </summary>
    internal sealed class HashingStream(IncrementalHash hash) : Stream
    {
        /// <summary>Bytes written since the last append to the hash.</summary>
        private readonly byte[] pending = new byte[64 * 1024];

        /// <summary>How many bytes of <see cref="pending"/> are written.</summary>
        private int pendingCount;

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
        /// <summary>Appends the buffered bytes to the hash.</summary>
        public override void Flush()
        {
            if (pendingCount == 0)
                return;
            hash.AppendData(pending, 0, pendingCount);
            pendingCount = 0;
        }
        /// <summary>Rejects reads from the write-only digest stream.</summary>
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        /// <summary>Rejects repositioning serialized input within the digest.</summary>
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        /// <summary>Rejects changing a digest stream's length after bytes were hashed.</summary>
        public override void SetLength(long value) => throw new NotSupportedException();
        /// <summary>Appends an array segment to the digest.</summary>
        public override void Write(byte[] buffer, int offset, int count) => Write(new ReadOnlySpan<byte>(buffer, offset, count));
        /// <summary>Appends one byte to the digest.</summary>
        public override void WriteByte(byte value) => Write(new ReadOnlySpan<byte>(in value));
        /// <summary>Appends a span to the digest: gathered while it fits the buffer, appended directly when larger.</summary>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length > pending.Length - pendingCount)
            {
                Flush();
                if (buffer.Length >= pending.Length)
                {
                    hash.AppendData(buffer);
                    return;
                }
            }
            buffer.CopyTo(pending.AsSpan(pendingCount));
            pendingCount += buffer.Length;
        }

        /// <summary>Appends what is still buffered before the stream closes, so the digest covers every byte written.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Flush();
            base.Dispose(disposing);
        }
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

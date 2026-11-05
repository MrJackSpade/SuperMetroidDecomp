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
    /// Whether <paramref name="left"/> and <paramref name="right"/> have the same complete debugger-state graph, both naming
    /// what they reach of <paramref name="content"/> by path: the comparison two equal <see cref="GraphDigest"/>s make,
    /// made on the serialized bytes themselves, so neither graph is hashed.
    /// </summary>
    internal static bool SameGraph(object left, object right, params object[] content) =>
        SameGraph(left, content, right, content);

    /// <summary>
    /// Whether <paramref name="left"/>, naming what it reaches of <paramref name="leftContent"/> by path, and
    /// <paramref name="right"/>, naming what it reaches of <paramref name="rightContent"/>, have the same complete
    /// debugger-state graph: the comparison their two <see cref="GraphDigest"/>s make, made on the serialized bytes. The left
    /// graph is written into this thread's reused buffer and the right one is compared with it as it is written.
    /// </summary>
    internal static bool SameGraph(object left, object[] leftContent, object right, object[] rightContent)
    {
        MemoryStream written = leftGraph ??= new MemoryStream();
        written.SetLength(0);
        BoundObjectGraph leftBound = Bound(leftContent);
        DebuggerObjectGraphSerializer.Serialize(written, left, leftBound);
        if (!written.TryGetBuffer(out ArraySegment<byte> expected))
            throw new InvalidOperationException("The reused graph buffer exposes its bytes.");
        using var comparing = new ComparingStream(expected);
        // Both graphs bound to one list name its content through the one index, refreshed once for the pair.
        DebuggerObjectGraphSerializer.Serialize(comparing, right, ReferenceEquals(leftContent, rightContent) ? leftBound : Bound(rightContent));
        return comparing.Same;
    }

    /// <summary>This thread's buffer for the left graph of a <see cref="SameGraph(object, object[], object, object[])"/>, kept between comparisons.</summary>
    [ThreadStatic] private static MemoryStream? leftGraph;

    /// <summary>Write-only stream that compares the bytes written with an expected graph as they arrive.</summary>
    private sealed class ComparingStream(ArraySegment<byte> expected) : Stream
    {
        /// <summary>How many bytes have been written.</summary>
        private long written;

        /// <summary>Whether a written byte differed from the expected byte at its position, or ran past the expected bytes.</summary>
        private bool differs;

        /// <summary>Whether the bytes written are exactly the expected bytes.</summary>
        internal bool Same => !differs && written == expected.Count;

        /// <summary>Comparing accepts writes only.</summary>
        public override bool CanRead => false;
        /// <summary>Written bytes cannot be repositioned.</summary>
        public override bool CanSeek => false;
        /// <summary>Written bytes are compared in order.</summary>
        public override bool CanWrite => true;
        /// <summary>A comparing stream has no retrievable length.</summary>
        public override long Length => throw new NotSupportedException();
        /// <summary>A comparing stream has no seekable position.</summary>
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        /// <summary>Comparisons are immediate and need no flush.</summary>
        public override void Flush() { }
        /// <summary>Rejects reads from the write-only stream.</summary>
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        /// <summary>Rejects repositioning written bytes.</summary>
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        /// <summary>Rejects changing the stream's length.</summary>
        public override void SetLength(long value) => throw new NotSupportedException();
        /// <summary>Compares an array segment with the expected bytes at the current position.</summary>
        public override void Write(byte[] buffer, int offset, int count) => Write(new ReadOnlySpan<byte>(buffer, offset, count));
        /// <summary>Compares one byte with the expected byte at the current position.</summary>
        public override void WriteByte(byte value) => Write(new ReadOnlySpan<byte>(in value));
        /// <summary>Compares a span with the expected bytes at the current position.</summary>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            if (!differs)
                differs = written + buffer.Length > expected.Count ||
                    !buffer.SequenceEqual(expected.AsSpan((int)written, buffer.Length));
            written += buffer.Length;
        }
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
    /// indexed once, so repeated captures do not walk its content again. A list that ends with a list already indexed, or
    /// with the fixture's installed content, extends that list's index: only what its leading objects reach first is walked,
    /// and the index is the one the whole list would make.
    /// </summary>
    private static BoundObjectGraph Bound(object[] content)
    {
        if (content.Length == 0)
            content = SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.BoundContent;
        lock (boundGraphs)
            return Indexed(content);
    }

    /// <summary>The graph of <paramref name="content"/>, indexed now unless it was before; callers hold the lock.</summary>
    private static BoundObjectGraph Indexed(object[] content)
    {
        foreach ((object[] roots, BoundObjectGraph graph) in boundGraphs)
            if (roots.Length == content.Length && EndsWith(content, roots))
            {
                // Content loaded since the list was indexed (a fixture catalog first used by a later suite in this
                // process) is indexed now, so a graph names it by path whatever order the suites ran in. An object
                // keeps the path it was first indexed under.
                graph.Refresh();
                return graph;
            }
        BoundObjectGraph indexed = IndexedSuffix(content) is { } suffix
            ? new BoundObjectGraph(content[..^suffix.Length], Indexed(suffix))
            : new BoundObjectGraph(content);
        boundGraphs.Add(([.. content], indexed));
        return indexed;
    }

    /// <summary>
    /// The longest list already indexed that <paramref name="content"/> ends with and is longer than, or else the fixture's
    /// installed content when the list ends with it; null when it ends with neither.
    /// </summary>
    private static object[]? IndexedSuffix(object[] content)
    {
        object[]? longest = null;
        foreach ((object[] roots, _) in boundGraphs)
            if (roots.Length < content.Length && roots.Length > (longest?.Length ?? 0) && EndsWith(content, roots))
                longest = roots;
        object[] installed = SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.BoundContent;
        if (longest is null && installed.Length < content.Length && EndsWith(content, installed))
            longest = installed;
        return longest;
    }

    /// <summary>Whether the last objects of <paramref name="content"/> are <paramref name="suffix"/>'s, the same objects in order.</summary>
    private static bool EndsWith(object[] content, object[] suffix)
    {
        int start = content.Length - suffix.Length;
        if (start < 0)
            return false;
        for (int index = 0; index < suffix.Length; index++)
            if (!ReferenceEquals(content[start + index], suffix[index]))
                return false;
        return true;
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
                    else if (!ValueArrayComparer(element)(leftArray, rightArray))
                    {
                        // Value elements (typed blocks, behaviours) compare by their own equality, one by one.
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
    /// <summary>
    /// Compares two arrays of one value element type element by element with that type's own equality, as
    /// <see cref="object.Equals(object?, object?)"/> compares each pair, without boxing every element (#1532): a room's
    /// block arrays hold thousands of elements, and a check restoring a room per case compares them per case.
    /// </summary>
    [ReflectionAudit("structural equality: each value element type's arrays compare through that type's own equality")]
    private static Func<Array, Array, bool> ValueArrayComparer(Type element) => valueArrayComparers.GetOrAdd(element, static type =>
        (typeof(GraphDigestVerification).GetMethod(nameof(ValueArraysEqual), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?? throw new InvalidOperationException("The value-array comparison is missing."))
            .MakeGenericMethod(type).CreateDelegate<Func<Array, Array, bool>>());

    /// <summary>Each value element type's array comparison, made once.</summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, Func<Array, Array, bool>> valueArrayComparers = new();

    /// <summary>Whether two arrays of <typeparamref name="T"/> hold equal elements by <typeparamref name="T"/>'s equality.</summary>
    private static bool ValueArraysEqual<T>(Array left, Array right) =>
        new ReadOnlySpan<T>((T[])left).SequenceEqual((T[])right, EqualityComparer<T>.Default);

    /// <summary>The pixels a packet shows, for a comparison.</summary>
    internal static RgbaPixel[] RenderForComparison(SuperMetroid.Core.Rendering.DisplayPacket frame) =>
        DisplayRendering.Render(frame);

    /// <summary>The pixels a layered scene shows, for a comparison.</summary>
    internal static RgbaPixel[] RenderForComparison(SuperMetroid.Core.Rendering.LayeredScene frame) =>
        DisplayRendering.Render(frame);

    /// <summary>
    /// Whether two packets show the same pixels (#1532). A packet holds everything the renderer draws and nothing else, so
    /// two packets whose descriptions compare equal, or whose scenes' whole graphs digest alike naming
    /// <paramref name="content"/> by path, show the same pixels without drawing either; this answers for a gameplay frame,
    /// whose room layer compares only by instance. Only packets that differ in both are drawn and compared pixel by pixel.
    /// The frame number names the publication, not what it shows, so it is not compared.
    /// </summary>
    internal static bool SamePixels(SuperMetroid.Core.Rendering.DisplayPacket left, SuperMetroid.Core.Rendering.DisplayPacket right,
        params object[] content)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        if (ScenePixels.SameDescription(left, right))
            return true;
        object? leftScene = (object?)left.Layers ?? left.Floor, rightScene = (object?)right.Layers ?? right.Floor;
        if (left.Brightness == right.Brightness && left.Size == right.Size && left.SolidColor == right.SolidColor
            && leftScene is not null && rightScene is not null && leftScene.GetType() == rightScene.GetType()
            && GraphDigest(leftScene, content).AsSpan().SequenceEqual(GraphDigest(rightScene, content)))
            return true;
        return ScenePixels.Same(left, right);
    }
}

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

    /// <summary>
    /// Whether <paramref name="value"/>, naming what it reaches of <paramref name="content"/> by path, has the graph
    /// <paramref name="graph"/>, which <see cref="SerializeGraph"/> wrote of a value bound to the same content: the comparison
    /// <see cref="SameGraph(object, object, object[])"/> makes, with one side written once for many comparisons (#1757).
    /// </summary>
    internal static bool MatchesGraph(object value, byte[] graph, params object[] content)
    {
        using var comparing = new ComparingStream(graph);
        DebuggerObjectGraphSerializer.Serialize(comparing, value, Bound(content));
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
    /// SHA-256 of everything <paramref name="value"/> reaches, content included: no object is named by path, so a change
    /// to content itself, such as a part it loads on first use, changes the digest (#1532).
    /// </summary>
    internal static byte[] UnboundGraphDigest(object value)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        using (var stream = new HashingStream(hash))
            DebuggerObjectGraphSerializer.Serialize(stream, value);
        return hash.GetHashAndReset();
    }

    /// <summary>
    /// The serialized debugger-state graph, naming what it reaches of <paramref name="content"/> by path, content loaded
    /// since the list was first indexed included.
    /// </summary>
    internal static byte[] SerializeGraph(object value, params object[] content)
    {
        using var stream = new ChunkedGraphStream();
        DebuggerObjectGraphSerializer.Serialize(stream, value, Bound(content));
        return stream.ToArray();
    }

    /// <summary>
    /// Captures <paramref name="value"/> and restores it bound to <paramref name="content"/>, as a host's store does. The
    /// capture is read back from the chunks it was written into, so a runtime's graph of a hundred megabytes and more is
    /// neither regrown nor copied (#1680).
    /// </summary>
    internal static T RoundTrip<T>(T value, params object[] content) where T : class
    {
        using var stream = new ChunkedGraphStream();
        DebuggerObjectGraphSerializer.Serialize(stream, value, Bound(content));
        stream.Position = 0;
        return DebuggerObjectGraphSerializer.Deserialize<T>(stream, Bound(content));
    }

    /// <summary>
    /// A graph written into fixed-size chunks and read back from the start. A growing <see cref="MemoryStream"/> reallocates
    /// and copies its whole buffer each time it doubles, so a graph of a hundred megabytes cost several hundred megabytes of
    /// allocation before its final copy (#1680); a chunk, once written, is never moved.
    /// </summary>
    private sealed class ChunkedGraphStream : Stream
    {
        /// <summary>
        /// The bytes each chunk holds: below the large-object threshold, so neither a small graph nor any chunk of a large
        /// one lands on the large-object heap, whose allocations trigger collections of the whole heap.
        /// </summary>
        private const int ChunkSize = 1 << 16;

        /// <summary>The chunks written, each full except the last.</summary>
        private readonly List<byte[]> chunks = [];

        /// <summary>How many bytes have been written.</summary>
        private long length;

        /// <summary>The next byte read or written.</summary>
        private long position;

        /// <summary>A written graph is read back.</summary>
        public override bool CanRead => true;
        /// <summary>Reading returns to the start of the graph.</summary>
        public override bool CanSeek => true;
        /// <summary>A graph is written once, in order.</summary>
        public override bool CanWrite => true;
        /// <summary>The bytes written.</summary>
        public override long Length => length;

        /// <summary>The next byte read or written; writes only append, so a write needs the position at the end.</summary>
        public override long Position
        {
            get => position;
            set => position = value >= 0 && value <= length ? value : throw new ArgumentOutOfRangeException(nameof(value), value, "The position lies outside the written graph.");
        }

        /// <summary>Writes are held in memory and need no flush.</summary>
        public override void Flush() { }

        /// <summary>Moves the position within the written graph.</summary>
        public override long Seek(long offset, SeekOrigin origin) => Position = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => position + offset,
            SeekOrigin.End => length + offset,
            _ => throw new ArgumentOutOfRangeException(nameof(origin), origin, "Unknown seek origin."),
        };

        /// <summary>Rejects changing the graph's length.</summary>
        public override void SetLength(long value) => throw new NotSupportedException();

        /// <summary>Reads into an array segment from the current position.</summary>
        public override int Read(byte[] buffer, int offset, int count) => Read(new Span<byte>(buffer, offset, count));

        /// <summary>Reads one byte from the current position, or -1 at the end.</summary>
        public override int ReadByte()
        {
            if (position == length)
                return -1;
            byte value = chunks[(int)(position / ChunkSize)][(int)(position % ChunkSize)];
            position++;
            return value;
        }

        /// <summary>Reads into a span from the current position, across chunk boundaries.</summary>
        public override int Read(Span<byte> buffer)
        {
            int total = (int)Math.Min(buffer.Length, length - position);
            for (int copied = 0; copied < total;)
            {
                int offset = (int)(position % ChunkSize);
                int count = Math.Min(total - copied, ChunkSize - offset);
                chunks[(int)(position / ChunkSize)].AsSpan(offset, count).CopyTo(buffer[copied..]);
                copied += count;
                position += count;
            }
            return total;
        }

        /// <summary>Appends an array segment.</summary>
        public override void Write(byte[] buffer, int offset, int count) => Write(new ReadOnlySpan<byte>(buffer, offset, count));

        /// <summary>Appends one byte.</summary>
        public override void WriteByte(byte value) => Write(new ReadOnlySpan<byte>(in value));

        /// <summary>Appends a span, starting a new chunk whenever the last one is full.</summary>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            if (position != length)
                throw new NotSupportedException("A graph is written once, in order.");
            while (!buffer.IsEmpty)
            {
                int offset = (int)(length % ChunkSize);
                if (offset == 0 && chunks.Count == length / ChunkSize)
                    chunks.Add(new byte[ChunkSize]);
                int count = Math.Min(buffer.Length, ChunkSize - offset);
                buffer[..count].CopyTo(chunks[^1].AsSpan(offset));
                buffer = buffer[count..];
                length += count;
            }
            position = length;
        }

        /// <summary>The written graph as one array.</summary>
        internal byte[] ToArray()
        {
            var bytes = new byte[length];
            for (int chunk = 0; chunk < chunks.Count; chunk++)
            {
                int count = (int)Math.Min(ChunkSize, length - (long)chunk * ChunkSize);
                chunks[chunk].AsSpan(0, count).CopyTo(bytes.AsSpan((int)((long)chunk * ChunkSize)));
            }
            return bytes;
        }
    }

    /// <summary>Restores a serialized graph bound to <paramref name="content"/>, as a host's store does.</summary>
    internal static T RestoreGraph<T>(byte[] graph, params object[] content) where T : class
    {
        using var stream = new MemoryStream(graph);
        return DebuggerObjectGraphSerializer.Deserialize<T>(stream, Bound(content));
    }

    /// <summary>
    /// The serialized debugger-state graph, naming what it reaches of <paramref name="content"/> by path and what the
    /// game's static members hold through <paramref name="definitions"/> instead of the process's index (#1716).
    /// </summary>
    internal static byte[] SerializeGraphThrough(object value, DefinitionObjects definitions, object[] content)
    {
        using var stream = new ChunkedGraphStream();
        DebuggerObjectGraphSerializer.Serialize(stream, value, Bound(content), definitions);
        return stream.ToArray();
    }

    /// <summary>
    /// Restores a serialized graph bound to <paramref name="content"/>, resolving what the game's static members hold
    /// through <paramref name="definitions"/> instead of the process's index (#1716).
    /// </summary>
    internal static T RestoreGraphThrough<T>(byte[] graph, DefinitionObjects definitions, object[] content) where T : class
    {
        using var stream = new MemoryStream(graph);
        return DebuggerObjectGraphSerializer.Deserialize<T>(stream, Bound(content), definitions);
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
            if (!definitionsDeclared)
                DeclareDefinitions();
            return Indexed(content);
        }
    }

    /// <summary>
    /// Declares the process's index of the objects the game's static members hold (<see cref="DefinitionObjects.Process"/>)
    /// and indexes the catalogs every capture asks, those whose members reach a location any object can be held in, as a
    /// shared fixture (#1757): the declaration reads every type of the game's assembly and every capture of the process
    /// names catalog objects through the one index, so its cost is a shared fixture's, as the installed content index's
    /// is (#1532), not a cost of whichever suite first captures. Catalogs only some objects' types can reach are still
    /// indexed by the capture that first meets one. Callers hold the lock.
    /// </summary>
    private static void DeclareDefinitions()
    {
        SuperMetroid.TestSupport.RepositoryInstallation.SharedFixtureTiming.Load("Fixture: definition index", () =>
            DefinitionObjects.Process.TryGetPath(new object(), out _));
        definitionsDeclared = true;
    }

    /// <summary>
    /// Declares the process's definition index as a shared fixture unless it was before, for a check that captures through
    /// the serializer directly rather than through <see cref="Bound"/>.
    /// </summary>
    internal static void DeclareDefinitionIndex()
    {
        lock (boundGraphs)
        {
            if (!definitionsDeclared)
                DeclareDefinitions();
        }
    }

    /// <summary>Whether the process's definition index was declared as a shared fixture.</summary>
    private static bool definitionsDeclared;

    /// <summary>
    /// The graph of <paramref name="content"/>, indexed now unless it was before; callers hold the lock. The fixture's
    /// installed content is indexed, and re-indexed after another of its catalogs loads, as a shared fixture
    /// (<see cref="IndexInstalledContent"/>).
    /// </summary>
    private static BoundObjectGraph Indexed(object[] content)
    {
        bool installed = IsInstalledContent(content);
        foreach ((object[] roots, BoundObjectGraph graph) in boundGraphs)
            if (roots.Length == content.Length && SameObjects(content, roots))
            {
                // Content loaded since the list was indexed (a fixture catalog first used by a later suite in this
                // process) is indexed now, so a graph names it by path whatever order the suites ran in. An object
                // keeps the path it was first indexed under.
                if (installed && SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.LoadedBoundContent != installedCatalogsIndexed)
                    IndexInstalledContent(() =>
                    {
                        graph.Refresh();
                        return graph;
                    });
                else
                    graph.Refresh();
                return graph;
            }
        BoundObjectGraph indexed = installed ? IndexInstalledContent(() => new BoundObjectGraph(content)) : new BoundObjectGraph(content);
        boundGraphs.Add(([.. content], indexed));
        return indexed;
    }

    /// <summary>Whether <paramref name="content"/> is the fixture's installed content, the same objects in order.</summary>
    private static bool IsInstalledContent(object[] content)
    {
        object[] installed = SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.BoundContent;
        return content.Length == installed.Length && SameObjects(content, installed);
    }

    /// <summary>Whether two lists of equal length hold the same objects in the same order.</summary>
    private static bool SameObjects(object[] content, object[] other)
    {
        for (int index = 0; index < other.Length; index++)
            if (!ReferenceEquals(content[index], other[index]))
                return false;
        return true;
    }

    /// <summary>
    /// Indexes the installed content's catalogs loaded since they were last indexed, as the process's one shared index of
    /// its installed content (#1532): every capture of the process names installed content through it, so its cost is a
    /// shared fixture's, timed as its own entry, not a cost of whichever suite first captures after a catalog loads.
    /// </summary>
    private static BoundObjectGraph IndexInstalledContent(Func<BoundObjectGraph> index)
    {
        installedCatalogsIndexed = SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.LoadedBoundContent;
        return SuperMetroid.TestSupport.RepositoryInstallation.SharedFixtureTiming.Load("Fixture: installed content index", index);
    }

    /// <summary>How many installed catalogs had loaded when the installed content was last indexed.</summary>
    private static int installedCatalogsIndexed = -1;

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
    /// True when two objects of one type hold equal state: every value they reach through the members the compiler gave every
    /// type of the game (#2602) is the same, compared side by side (<see cref="PublishedGraphs"/>), so a restore that forgets
    /// a field added later fails here. Nothing is serialized and no shared index is written, so workers compare at once.
    /// </summary>
    internal static bool InstanceStateEquals<T>(T actual, T expected) where T : class => PublishedGraphs.Same(actual, expected);

    /// <summary>The pixels a packet shows, for a comparison.</summary>
    internal static RgbaPixel[] RenderForComparison(SuperMetroid.Core.Rendering.DisplayPacket frame) =>
        DisplayRendering.Render(frame);

    /// <summary>The pixels a layered scene shows, for a comparison.</summary>
    internal static RgbaPixel[] RenderForComparison(SuperMetroid.Core.Rendering.LayeredScene frame) =>
        DisplayRendering.Render(frame);
}

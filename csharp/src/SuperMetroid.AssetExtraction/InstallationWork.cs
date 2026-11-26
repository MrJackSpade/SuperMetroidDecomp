namespace SuperMetroid.AssetExtraction;

/// <summary>
/// The pieces of work an installation's long loops do, counted for whoever watches the installation. A watched
/// installation's loops say how many pieces each has as it starts and count each piece as it is done; an installation
/// nobody watches counts nothing and runs as it always did.
/// </summary>
internal sealed partial class InstallationWork
{
    /// <summary>
    /// The count of the installation running on this thread, which the threads and tasks it starts share; null when
    /// nobody watches it.
    /// </summary>
    private static readonly AsyncLocal<InstallationWork?> Watched = new();

    /// <summary>Told how many pieces are done, and of how many known so far, each time either changes.</summary>
    private readonly Action<int, int> _changed;
    private int _expected;
    private int _done;

    private InstallationWork(Action<int, int> changed) => _changed = changed;

    /// <summary>
    /// Runs <paramref name="install"/> with its loops' work counted: <paramref name="changed"/> is told how many pieces
    /// are done, and of how many known so far, each time either changes, from whichever thread did the piece. The
    /// number known grows as each loop starts.
    /// </summary>
    internal static void Watching(Action<int, int> changed, Action install)
    {
        ArgumentNullException.ThrowIfNull(changed);
        ArgumentNullException.ThrowIfNull(install);
        InstallationWork? before = Watched.Value;
        Watched.Value = new InstallationWork(changed);
        try
        {
            install();
        }
        finally
        {
            Watched.Value = before;
        }
    }

    /// <summary>Runs <paramref name="body"/> for each index of a range at once, as <c>Parallel.For</c> does, each a piece of work.</summary>
    internal static void For(int from, int to, Action<int> body)
    {
        InstallationWork? work = Watched.Value;
        work?.Expect(to - from);
        Parallel.For(from, to, index =>
        {
            body(index);
            work?.Did();
        });
    }

    /// <summary>
    /// Runs <paramref name="body"/> for each index of a range at once, each thread with what <paramref name="local"/>
    /// makes for it, as <c>Parallel.For</c> does, each index a piece of work.
    /// </summary>
    internal static void For<TLocal>(int from, int to, Func<TLocal> local, Func<int, ParallelLoopState, TLocal, TLocal> body, Action<TLocal> finished)
    {
        InstallationWork? work = Watched.Value;
        work?.Expect(to - from);
        Parallel.For(from, to, local, (index, state, held) =>
        {
            held = body(index, state, held);
            work?.Did();
            return held;
        }, finished);
    }

    /// <summary>Adds a loop's pieces to those known.</summary>
    private void Expect(int pieces)
    {
        int expected = Interlocked.Add(ref _expected, Math.Max(0, pieces));
        _changed(Volatile.Read(ref _done), expected);
    }

    /// <summary>Counts a piece as done.</summary>
    private void Did()
    {
        int done = Interlocked.Increment(ref _done);
        _changed(done, Volatile.Read(ref _expected));
    }
}

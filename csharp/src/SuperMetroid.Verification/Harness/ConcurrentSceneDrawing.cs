using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Verification.Harness;

/// <summary>
/// Draws a batch of independent items at once (#1532) on a fixed set of long-lived companion threads, each drawing with
/// its own renderer: <see cref="DisplayRendering"/> keeps one renderer per thread, so every companion draws with its own
/// WARP device, and the companions live for the process so each keeps its renderer and its uploaded images from batch to
/// batch. The companions and their renderers are made once, as a shared fixture. Every item is drawn exactly as when the
/// items were drawn one by one; <see cref="DrawEach"/> returns only once every item is drawn.
/// </summary>
internal static class ConcurrentSceneDrawing
{
    /// <summary>
    /// How many companions draw at once: a fixed bound, a quarter of the machine's processors, at least two and at most
    /// six, so a long batch never makes more renderers than this.
    /// </summary>
    private static readonly int CompanionCount = Math.Clamp(Environment.ProcessorCount / 4, 2, 6);

    /// <summary>
    /// The companion threads, each with its renderer made, made on first use as one shared fixture: they and their
    /// renderers live for the process, so their making is no cost of the check that first draws through them.
    /// </summary>
    private static readonly Lazy<DrawingCompanion[]> companions =
        new(() => SharedFixtureTiming.Load("Fixture: scene drawing companions", MakeCompanions));

    /// <summary>
    /// What <paramref name="draw"/> makes of each of <paramref name="items"/>, in item order, the items drawn at once on
    /// the companions, each companion taking the next item not yet taken. Every item has been drawn when this returns or
    /// throws; a failing item's own exception is rethrown with its stack, the earliest item's first.
    /// </summary>
    /// <param name="items">The independent items to draw.</param>
    /// <param name="draw">Draws one item on the companion that took it, with that companion's renderer.</param>
    internal static TResult[] DrawEach<TItem, TResult>(IReadOnlyList<TItem> items, Func<TItem, TResult> draw)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(draw);
        var results = new TResult[items.Count];
        if (items.Count == 0)
            return results;
        var failures = new ExceptionDispatchInfo?[items.Count];
        DrawingCompanion[] drawing = companions.Value;
        int taken = -1;
        var running = new Task[Math.Min(drawing.Length, items.Count)];
        for (int companion = 0; companion < running.Length; companion++)
        {
            running[companion] = drawing[companion].Run(() =>
            {
                for (int index = Interlocked.Increment(ref taken); index < items.Count; index = Interlocked.Increment(ref taken))
                {
                    try
                    {
                        results[index] = draw(items[index]);
                    }
                    catch (Exception exception)
                    {
                        // Held, not handled: rethrown below, in item order, once every companion has finished.
                        failures[index] = ExceptionDispatchInfo.Capture(exception);
                    }
                }
            });
        }
        ExceptionDispatchInfo? stopped = null;
        foreach (Task task in running)
        {
            try
            {
                task.GetAwaiter().GetResult();
            }
            catch (Exception exception)
            {
                // Held, not handled: a companion's own failure is rethrown below once every companion has finished.
                stopped ??= ExceptionDispatchInfo.Capture(exception);
            }
        }
        stopped?.Throw();
        foreach (ExceptionDispatchInfo? failure in failures)
            failure?.Throw();
        return results;
    }

    /// <summary>Starts the companions and waits until each has made its renderer; a renderer's failure is rethrown here.</summary>
    private static DrawingCompanion[] MakeCompanions()
    {
        var made = new DrawingCompanion[CompanionCount];
        for (int number = 0; number < made.Length; number++)
            made[number] = new DrawingCompanion(number);
        foreach (DrawingCompanion companion in made)
            companion.Ready.GetAwaiter().GetResult();
        return made;
    }

    /// <summary>One piece of work a companion runs, and the task that completes when it has run or failed.</summary>
    /// <param name="Work">The work to run.</param>
    /// <param name="Done">Completed once the work has run, or with the work's exception.</param>
    private readonly record struct CompanionWork(Action Work, TaskCompletionSource Done);

    /// <summary>
    /// A background thread that makes its renderer, then runs the work given to it in order, for the life of the process.
    /// A failure completes the task of the work that failed, which <see cref="DrawEach"/> rethrows on the calling thread;
    /// none escapes the thread.
    /// </summary>
    private sealed class DrawingCompanion
    {
        /// <summary>The work given to the companion and not yet run, in the order it was given.</summary>
        private readonly BlockingCollection<CompanionWork> pending = new(new ConcurrentQueue<CompanionWork>());

        /// <summary>Completed once the companion's renderer is made, or with the exception that stopped its making.</summary>
        private readonly TaskCompletionSource ready = new(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>Starts the companion's thread, which makes its renderer before it runs any work.</summary>
        /// <param name="number">The companion's place among the companions, which names its thread.</param>
        internal DrawingCompanion(int number)
        {
            var thread = new Thread(RunPending)
            {
                IsBackground = true,
                Name = $"Verification scene drawing {number}",
            };
            thread.Start();
        }

        /// <summary>Completes once the companion's renderer is made.</summary>
        internal Task Ready => ready.Task;

        /// <summary>Gives the companion <paramref name="work"/> to run after the work given before it.</summary>
        internal Task Run(Action work)
        {
            var done = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            pending.Add(new CompanionWork(work, done));
            return done.Task;
        }

        /// <summary>Makes the thread's renderer, then runs each piece of work given to the companion.</summary>
        private void RunPending()
        {
            try
            {
                DisplayRendering.Prepare();
            }
            catch (Exception exception)
            {
                // Passed to the fixture's making, which rethrows it: a companion without a renderer draws nothing.
                ready.SetException(exception);
                return;
            }
            ready.SetResult();
            foreach (CompanionWork work in pending.GetConsumingEnumerable())
            {
                try
                {
                    work.Work();
                }
                catch (Exception exception)
                {
                    // Passed to the caller, which rethrows it: a failing draw fails the check that asked for it.
                    work.Done.SetException(exception);
                    continue;
                }
                work.Done.SetResult();
            }
        }
    }
}

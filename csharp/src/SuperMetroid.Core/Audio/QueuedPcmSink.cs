using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;

namespace SuperMetroid.Core.Audio;

/// <summary>Bounded, ordered PCM delivery that isolates a host's blocking device writes.</summary>
public sealed class QueuedPcmSink : IDisposable
{
    private readonly BlockingCollection<short[]> queue;
    private readonly Task worker;
    private ExceptionDispatchInfo? failure;
    /// <summary>Waiting frames only; excludes the frame currently owned by the device writer.</summary>
    public int PendingFrameCount => queue.Count;

    /// <summary>Starts a long-running worker that delivers submitted PCM buffers in order through a bounded queue.</summary>
    /// <param name="write">Device-writer callback invoked serially on the worker, receiving each submission's copied sample array; the sink does not dispose the callback's device.</param>
    /// <param name="capacity">Positive maximum number of waiting sample arrays, not samples or stereo frames; excludes the buffer currently being written and applies backpressure to <see cref="Submit"/>.</param>
    /// <remarks>Callback exceptions are captured on the worker and rethrown by subsequent submission or disposal. Dispose the sink before the device so accepted buffers can drain.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="write"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is zero or negative.</exception>
    public QueuedPcmSink(Action<short[]> write, int capacity)
    {
        ArgumentNullException.ThrowIfNull(write);
        queue = new(capacity);
        worker = Task.Factory.StartNew(() =>
        {
            try
            {
                foreach (short[] samples in queue.GetConsumingEnumerable()) write(samples);
            }
            catch (Exception error)
            {
                Volatile.Write(ref failure, ExceptionDispatchInfo.Capture(error));
                queue.CompleteAdding();
            }
        }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    /// <summary>Copies the borrowed emulator buffer; applies backpressure when the bounded queue is full.</summary>
    public void Submit(ReadOnlySpan<short> samples)
    {
        Volatile.Read(ref failure)?.Throw();
        try { queue.Add(samples.ToArray()); }
        catch (InvalidOperationException)
        {
            Volatile.Read(ref failure)?.Throw();
            throw;
        }
        Volatile.Read(ref failure)?.Throw();
    }

    /// <summary>Drains accepted samples before the host disposes its device; surfaces worker failures.</summary>
    public void Dispose()
    {
        queue.CompleteAdding();
        worker.GetAwaiter().GetResult();
        queue.Dispose();
        Volatile.Read(ref failure)?.Throw();
    }
}

using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;

namespace SuperMetroid.Core.Audio;

/// <summary>Bounded, ordered PCM delivery that isolates a host's blocking device writes.</summary>
public sealed class QueuedPcmSink : IDisposable
{
    private readonly BlockingCollection<short[]> queue;
    private readonly Task worker;
    private ExceptionDispatchInfo? failure;

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

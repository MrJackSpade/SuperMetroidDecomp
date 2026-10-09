using System.Diagnostics;

namespace SuperMetroid.Android;

/// <summary>
/// Bounded, memory-only startup/resume trace shared by emulation and PCM workers.
/// Flushed at a stopped lifecycle boundary so diagnostic disk I/O cannot create
/// the very startup stalls under investigation. Timestamps share Stopwatch's clock.
/// </summary>
/// <param name="capacity">Maximum event count retained before later events are ignored.</param>
internal sealed class AndroidResumeTrace(int capacity = 300)
{
    /// <summary>Protects the shared trace list across emulation and audio workers.</summary>
    private readonly object gate = new();
    /// <summary>Timestamped event lines retained until a lifecycle-boundary flush.</summary>
    private readonly List<string> events = new();

    /// <summary>Records one event until the configured bounded capacity has been reached.</summary>
    /// <param name="kind">Short event label.</param>
    /// <param name="fields">Diagnostic fields to append to the timestamped line.</param>
    public void Record(string kind, string fields)
    {
        lock (gate)
        {
            if (events.Count >= capacity) return;
            events.Add($"ticks={Stopwatch.GetTimestamp()} {kind} {fields}");
        }
    }

    /// <summary>Whether the event limit has been reached and additional records are being dropped.</summary>
    public bool Full { get { lock (gate) return events.Count >= capacity; } }

    /// <summary>Writes all retained events and clears the in-memory buffer.</summary>
    /// <param name="write">Destination for the formatted trace.</param>
    public void Flush(Action<string> write)
    {
        lock (gate)
        {
            if (events.Count == 0) return;
            write($"resume-trace utc={DateTimeOffset.UtcNow:O} tickFrequency={Stopwatch.Frequency}\n" + string.Join('\n', events) + "\n");
            events.Clear();
        }
    }
}

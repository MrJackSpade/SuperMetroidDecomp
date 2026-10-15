using System.Diagnostics;

namespace SuperMetroid.Diagnostics.Android;

/// <summary>
/// Bounded, memory-only startup/resume trace shared by emulation and PCM workers.
/// Flushed at a stopped lifecycle boundary so diagnostic disk I/O cannot create
/// the very startup stalls under investigation. Timestamps share Stopwatch's clock.
/// </summary>
internal sealed class AndroidResumeTrace(int capacity = 300)
{
    /// <summary>Serializes worker updates and flushes without exposing a partially recorded trace.</summary>
    private readonly Lock gate = new();
    /// <summary>Bounded timestamped entries held in memory until lifecycle shutdown.</summary>
    private readonly List<string> events = new();
    /// <summary>Appends one monotonic-clock event unless the configured capacity has been reached.</summary>
    public void Record(string kind, string fields)
    {
        lock (gate)
        {
            if (events.Count >= capacity) return;
            events.Add($"ticks={Stopwatch.GetTimestamp()} {kind} {fields}");
        }
    }

    /// <summary>Reports whether additional worker events would be discarded.</summary>
    public bool Full { get { lock (gate) return events.Count >= capacity; } }

    /// <summary>Writes a UTC and clock-frequency header with all events, then clears the trace after a successful write.</summary>
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

using System.Diagnostics;

namespace SuperMetroid.Android;

/// <summary>
/// Bounded, memory-only startup/resume trace shared by emulation and PCM workers.
/// Flushed at a stopped lifecycle boundary so diagnostic disk I/O cannot create
/// the very startup stalls under investigation. Timestamps share Stopwatch's clock.
/// </summary>
internal sealed class AndroidResumeTrace(int capacity = 300)
{
    private readonly object gate = new();
    private readonly List<string> events = new();
    public void Record(string kind, string fields)
    {
        lock (gate)
        {
            if (events.Count >= capacity) return;
            events.Add($"ticks={Stopwatch.GetTimestamp()} {kind} {fields}");
        }
    }

    public bool Full { get { lock (gate) return events.Count >= capacity; } }

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

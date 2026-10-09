using System.Diagnostics;
using System.Text;

namespace SuperMetroid.Android;

/// <summary>
/// Allocation-free rolling handoff events. The mailbox gate owns all access.
/// Formatting and disk I/O occur only when the stopped host explicitly flushes.
/// </summary>
/// <param name="capacity">Maximum number of recent events retained in the rolling trace.</param>
internal sealed class AndroidFrameHandoffTrace(int capacity = 2048)
{
    /// <summary>Fixed-capacity ring buffer of recent frame-handoff events.</summary>
    private readonly Entry[] entries = new Entry[capacity > 0 ? capacity : throw new ArgumentOutOfRangeException(nameof(capacity))];
    /// <summary>Next ring position to write and number of valid entries retained.</summary>
    private int next, count;
    /// <summary>Appends an allocation-free event and current garbage-collection counts.</summary>
    /// <param name="kind">Single-character handoff event code.</param>
    /// <param name="sequence">Published frame sequence associated with the event.</param>
    /// <param name="ticks">Monotonic timestamp from <see cref="Stopwatch.GetTimestamp"/>.</param>
    public void Record(char kind, long sequence, long ticks)
    {
        entries[next] = new Entry(ticks, sequence, kind, GC.CollectionCount(0), GC.CollectionCount(2));
        next = (next + 1) % entries.Length;
        count = Math.Min(count + 1, entries.Length);
    }

    /// <summary>Serializes retained events and resets the rolling trace after a successful write.</summary>
    /// <param name="write">Destination for the complete text trace.</param>
    public void Flush(Action<string> write)
    {
        if (count == 0) return;
        var text = new StringBuilder($"handoff utc={DateTimeOffset.UtcNow:O} tickFrequency={Stopwatch.Frequency}\n");
        for (int i = 0; i < count; i++)
        {
            Entry entry = entries[(next - count + i + entries.Length) % entries.Length];
            text.Append(entry.Ticks).Append(' ').Append(entry.Kind).Append(' ').Append(entry.Sequence)
                .Append(' ').Append(entry.Gen0).Append(' ').Append(entry.Gen2).Append('\n');
        }
        write(text.ToString());
        next = count = 0;
    }

    /// <summary>One timestamped handoff event with the GC collection counts observed at capture time.</summary>
    /// <param name="Ticks">Monotonic timestamp.</param>
    /// <param name="Sequence">Frame sequence number.</param>
    /// <param name="Kind">Handoff event code.</param>
    /// <param name="Gen0">Generation-zero collection count.</param>
    /// <param name="Gen2">Generation-two collection count.</param>
    private readonly record struct Entry(long Ticks, long Sequence, char Kind, int Gen0, int Gen2);
}

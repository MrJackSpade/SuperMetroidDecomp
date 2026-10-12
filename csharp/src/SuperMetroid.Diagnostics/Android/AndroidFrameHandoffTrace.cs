using System.Diagnostics;
using System.Text;

namespace SuperMetroid.Android;

/// <summary>
/// Allocation-free rolling handoff events. The mailbox gate owns all access.
/// Formatting and disk I/O occur only when the stopped host explicitly flushes.
/// </summary>
internal sealed class AndroidFrameHandoffTrace(int capacity = 2048)
{
    /// <summary>Stores entries.</summary>
    private readonly Entry[] entries = new Entry[capacity > 0 ? capacity : throw new ArgumentOutOfRangeException(nameof(capacity))];
    /// <summary>Stores next.</summary>
    private int next, count;
    /// <summary>Record.</summary>
    public void Record(char kind, long sequence, long ticks)
    {
        entries[next] = new Entry(ticks, sequence, kind, GC.CollectionCount(0), GC.CollectionCount(2));
        next = (next + 1) % entries.Length;
        count = Math.Min(count + 1, entries.Length);
    }

    /// <summary>Flush.</summary>
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

    /// <summary>Represents entry.</summary>
    private readonly record struct Entry(long Ticks, long Sequence, char Kind, int Gen0, int Gen2);
}

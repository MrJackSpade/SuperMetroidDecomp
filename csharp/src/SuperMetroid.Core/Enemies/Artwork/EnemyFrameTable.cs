using System.Collections;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>A family's frames mapped to the installed frames they show; a frame outside the table fails loudly.</summary>
internal sealed class EnemyFrameTable<TFrame> : IEnumerable<KeyValuePair<TFrame, EnemyFrameId>>
    where TFrame : notnull
{
    /// <summary>The installed frame each family frame shows.</summary>
    private readonly Dictionary<TFrame, EnemyFrameId> _shown = [];

    /// <summary>Records that <paramref name="frame"/> shows <paramref name="shown"/>; a frame is recorded once.</summary>
    internal void Add(TFrame frame, EnemyFrameId shown) => _shown.Add(frame, shown);

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal EnemyFrameId Of(TFrame frame) => _shown.TryGetValue(frame, out EnemyFrameId shown)
        ? shown
        : throw new ArgumentOutOfRangeException(nameof(frame), frame, $"No installed frame is shown by this {typeof(TFrame).Name}.");

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<TFrame, EnemyFrameId>> GetEnumerator() => _shown.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

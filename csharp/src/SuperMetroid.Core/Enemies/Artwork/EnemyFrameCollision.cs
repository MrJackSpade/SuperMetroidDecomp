using System.Collections;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// A family's installed frames mapped to their ordered collision elements (components or hitboxes); a frame outside the
/// table has no compiled collision.
/// </summary>
internal sealed class EnemyFrameCollision<TElement> : IEnumerable<KeyValuePair<EnemyFrameId, TElement[]>>
{
    /// <summary>Each frame's collision elements, in native order.</summary>
    private readonly Dictionary<EnemyFrameId, TElement[]> _elements = [];

    /// <summary>Records <paramref name="frame"/>'s collision elements; a frame is recorded once.</summary>
    internal void Add(EnemyFrameId frame, TElement[] elements) => _elements.Add(frame, elements);

    /// <summary>The number of frames with compiled collision.</summary>
    internal int Count => _elements.Count;

    /// <summary>Whether <paramref name="frame"/> has compiled collision; no frame has none.</summary>
    internal bool Contains(EnemyFrameId? frame) => frame is { } shown && _elements.ContainsKey(shown);

    /// <summary>Finds <paramref name="frame"/>'s collision elements.</summary>
    internal bool TryGet(EnemyFrameId? frame, out TElement[] elements)
    {
        if (frame is { } shown && _elements.TryGetValue(shown, out TElement[]? found))
        {
            elements = found;
            return true;
        }
        elements = [];
        return false;
    }

    /// <summary>The collision elements of <paramref name="frame"/>, which must have compiled collision.</summary>
    internal TElement[] Of(EnemyFrameId? frame) => TryGet(frame, out TElement[] elements)
        ? elements
        : throw new InvalidDataException($"Enemy frame {frame} has no compiled {typeof(TElement).Name} collision.");

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<EnemyFrameId, TElement[]>> GetEnumerator() => _elements.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

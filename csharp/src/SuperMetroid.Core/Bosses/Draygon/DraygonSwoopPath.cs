using System.Collections.Immutable;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// The heights a swoop takes Draygon's body through (#1627): one entry for each update of the climb from the swoop's
/// lowest point up toward the reset height, in climbing order, and after them the height the body started the swoop at,
/// which the descent takes first. The path holds as many entries as that climb takes and nothing else limits its length.
/// </summary>
internal sealed class DraygonSwoopPath
{
    /// <summary>The body's height at each entry, from the swoop's lowest point up.</summary>
    private readonly ImmutableArray<int> _heights;

    /// <summary>Keeps the climb's heights and the height the swoop started at.</summary>
    /// <param name="heights">The body's height at each entry, from the swoop's lowest point up; at least one.</param>
    /// <param name="startHeight">The body's height when the swoop was planned.</param>
    /// <exception cref="ArgumentException">The path has no entry.</exception>
    internal DraygonSwoopPath(ImmutableArray<int> heights, int startHeight)
    {
        if (heights.IsDefaultOrEmpty)
            throw new ArgumentException("A swoop path climbs through at least one height.", nameof(heights));
        _heights = heights;
        StartHeight = startHeight;
    }

    /// <summary>
    /// How many updates the climb takes: the entries before <see cref="StartHeight"/>. The horizontal speed covers a swoop
    /// half in this many updates.
    /// </summary>
    internal int EntryCount => _heights.Length;

    /// <summary>The body's height when the swoop was planned, taken at entry <see cref="EntryCount"/>.</summary>
    internal int StartHeight { get; }

    /// <summary>
    /// The body's height at entry <paramref name="entry"/>: a height of the climb, or <see cref="StartHeight"/> at
    /// <see cref="EntryCount"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="entry"/> lies outside the path.</exception>
    internal int HeightAt(int entry) =>
        Ensure.BetweenInclusive(0, EntryCount, entry) == EntryCount ? StartHeight : _heights[entry];
}

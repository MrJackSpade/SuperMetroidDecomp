using System.Collections.Immutable;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The launch positions of Crocomire's crumbling bridge actors.</summary>
internal static class CrocomireBridgeFragmentDefinitions
{
    /// <summary>
    /// Where each bridge fragment of Crocomire's fall starts, in the order the bridge crumbles (#2303): the bridge's eleven
    /// sixteen-pixel columns from X 1808 to 1968, visited in an authored order no random number chooses. The collapse
    /// launches one fragment an update in this order; each starts at Y 187 with no horizontal speed and its own picture,
    /// and falls at a random speed per update from a quarter of a pixel up to, but not including, half a pixel.
    /// </summary>
    private static readonly ImmutableArray<int> CrumbleXPositions = [1920, 1840, 1936, 1856, 1968, 1888, 1952, 1904, 1808, 1872, 1824];

    /// <summary>
    /// The gravity on a falling bridge fragment, each update. The fragment starts falling
    /// at under half a pixel an update and is removed when its terrain probe finds a block, at the latest as it leaves the
    /// room, at most fifty screens tall (<see cref="Rooms.Scrolling.RoomScrollGrid.KeptCellCount"/>); its fall speed
    /// therefore stays below 50 pixels an update, and the game's restart of a fall reaching 64 pixels an update is never
    /// reached (#1795).
    /// </summary>
    internal static readonly SignedEightEight Gravity = SignedEightEight.FromPixelFraction(3, 32);

    /// <summary>The number of bridge fragments the collapse launches, one per column.</summary>
    internal static int FragmentCount => CrumbleXPositions.Length;

    /// <summary>The row each fragment starts falling from (#2182).</summary>
    internal const int Y = 187;

    /// <summary>The slowest a fragment starts falling, a quarter of a pixel an update, to which its random speed adds (#2182, #2299).</summary>
    internal static readonly SignedEightEight SlowestFall = SignedEightEight.FromPixelFraction(1, 4);

    /// <summary>Where the <paramref name="fragment"/>th fragment the collapse launches starts.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The collapse launches no such fragment.</exception>
    internal static int XPosition(int fragment) => CrumbleXPositions[Ensure.BetweenInclusive(0, FragmentCount - 1, fragment)];
}

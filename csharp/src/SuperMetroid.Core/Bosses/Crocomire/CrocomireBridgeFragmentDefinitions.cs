namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Compiled physical launch positions for Crocomire's crumbling bridge actors.</summary>
internal static class CrocomireBridgeFragmentDefinitions
{
    /// <summary>
    /// The bridge columns of Crocomire's fall, in the order they crumble.
    /// The collapse steps through it one fragment at a time.
    /// </summary>
    /// <remarks>
    /// The eleven equal-width floor columns are visited in this destruction choreography:
    /// 8,3,9,4,11,6,10,7,1,5,2. The falling sequence advances the cursor and passes
    /// each selected X to the same producer. The fragment initializer fixes Y187, X velocity0 and
    /// its graphics; independently, each fragment gets Y velocity(RNG &amp; 63)+64.
    /// No RNG, actor direction or data-dependent choice selects the
    /// order. The permutation is the authored ordering of the visual destruction, not
    /// sampled geometry. Retain only that order; coordinates calculate from the grid.
    /// Issue1165 retains this order as the narrow nonsense exception: replacing the
    /// chosen destruction choreography with an invented shuffle would change the effect.
    /// </remarks>
    private static ReadOnlySpan<int> AuthoredCrumbleColumnOrder => [8, 3, 9, 4, 11, 6, 10, 7, 1, 5, 2];

    /// <summary>The eleven floor columns begin 1808 pixels into the room, sixteen pixels apart.</summary>
    private const int FirstColumnX = 1808;

    /// <summary>
    /// The gravity on a falling bridge fragment, in 256ths of a pixel an update, each update. The fragment starts falling
    /// at under half a pixel an update and is removed when its terrain probe finds a block, at the latest as it leaves the
    /// room, at most fifty screens tall (<see cref="Rooms.Scrolling.RoomScrollGrid.KeptCellCount"/>); its fall speed
    /// therefore stays below 50 pixels an update, and the game's restart of a fall reaching 64 pixels an update is never
    /// reached (#1795).
    /// </summary>
    internal static readonly SignedEightEight Gravity = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(24);

    /// <summary>The number of bridge fragments the collapse launches, one per authored column.</summary>
    internal const int FragmentCount = 11;

    /// <summary>Returns the authored X position of the <paramref name="fragment"/>th fragment the collapse launches.</summary>
    internal static int XPosition(int fragment) =>
        CoordinateAxis.Wrap(FirstColumnX + 16 * (AuthoredCrumbleColumnOrder[Ensure.BetweenInclusive(0, FragmentCount - 1, fragment)] - 1));
}

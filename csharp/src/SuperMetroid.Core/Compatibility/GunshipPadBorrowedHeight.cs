namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Where the gunship's entrance pad starts (#1592): one pixel above the enemy the room lists two places before the pad,
/// whatever that enemy is. Every retail gunship population lists the top hull there, so the pad opens one pixel above
/// the hull's origin; a population listing another enemy there would move the pad to that enemy's height.
/// </summary>
/// <remarks>
/// Consulted once, by the pad's setup as the room's enemies are placed, after the top hull's setup has set its height.
/// Evidence: the bottom piece's original setup reads the vertical position from the enemy record two records before its
/// own, not the hull's by name.
/// </remarks>
[CompatibilityService("the gunship's entrance pad starts one pixel above the enemy listed two places before it")]
public static class GunshipPadBorrowedHeight
{
    /// <summary>How many places before the pad the room lists the enemy whose height it borrows.</summary>
    private const int LenderDistance = 2;

    /// <summary>How far above the borrowed height the pad starts, in pixels.</summary>
    private const int RiseAboveLender = 1;

    /// <summary>The whole vertical position the entrance pad starts at, before wrapping into the room.</summary>
    /// <param name="heightOfEnemyListedBeforePad">The whole vertical position of the enemy the room lists a given number of places, one or more, before the pad.</param>
    public static int Start(Func<int, int> heightOfEnemyListedBeforePad) =>
        Ensure.NotNull(heightOfEnemyListedBeforePad)(LenderDistance) - RiseAboveLender;
}

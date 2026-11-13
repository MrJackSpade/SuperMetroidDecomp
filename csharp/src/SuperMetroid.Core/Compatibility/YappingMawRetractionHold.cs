namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Whether a Yapping Maw that finishes retracting carries the Samus it holds to its mouth on that update (#1620, was
/// <c>YappingMawBehavior</c>'s "parameter-zero early return"): only a maw that reaches down and bit straight right or left
/// does. A maw that reaches up, and one that reaches down and bit in any other direction, leaves her where its previous
/// update held her, though its mouth has moved; its cooldown's first update carries her to the mouth again. Through that
/// one update Samus's own update starts from the earlier hold.
/// </summary>
/// <remarks>
/// Consulted once per retraction, on the update the maw's curl passes below nothing and its cooldown starts, after the
/// cooldown animation is chosen. Evidence: the end of the attack (<c>$A8:A5D2</c>-<c>$A8:A63D</c>) chooses the reaching-up
/// cooldown list and returns (<c>BNE</c> at <c>$A8:A60A</c>) when the maw's second parameter is not zero, the reach up, so
/// it never moves Samus; for a reach down it chooses again, and only the straight-right (<c>$A8:A61C</c>) and straight-left
/// (<c>$A8:A624</c>) branches reach the move (<c>$A8:A634</c>), the shared branch returning (<c>$A8:A632</c>). Retail maws
/// reach both ways and grab Samus as they extend, holding her through the retraction. The Yapping Maw retraction hold
/// check compares every reach and bite direction with those branches through the production retraction.
/// </remarks>
[CompatibilityService("a Yapping Maw that finishes retracting carries the Samus it holds to its mouth on that update only when it reaches down and bit straight right or left")]
internal static class YappingMawRetractionHold
{
    /// <summary>
    /// Whether a maw reaching <paramref name="reach"/> carries the Samus it holds to its mouth on the update it finishes
    /// retracting.
    /// </summary>
    /// <param name="reach">The way the maw reaches.</param>
    /// <param name="bitStraightAcross">Whether its bite went straight right or straight left.</param>
    internal static bool CarriesHeldSamus(VerticalDirection reach, bool bitStraightAcross) =>
        reach == VerticalDirection.Down && bitStraightAcross;
}

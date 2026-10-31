namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The freeze time a thawing enemy is left with (#1472): not zero, but one update while the grapple beam holds it, two
/// while it is still hurt, and three while both hold, so a later read of the enemy's freeze time sees that remainder
/// rather than an expired freeze.
/// </summary>
/// <remarks>
/// Consulted by the frozen-enemy pass on the update an enemy thaws, after its frozen state is cleared. Evidence: the
/// original thaw branch stores the remaining handler state into the freeze timer as well as the handler, as the
/// frozen-pass comment in <c>RoomEnemySystem</c> records; the port kept that value until #1472 named it.
/// </remarks>
[CompatibilityService("a thawing enemy is left one freeze update while grappled and two while hurt instead of none")]
public static class ThawedFreezeLeftover
{
    /// <summary>The freeze updates left while the grapple beam holds the thawing enemy.</summary>
    private const int WhileGrappled = 1;

    /// <summary>The freeze updates left while the thawing enemy is still hurt.</summary>
    private const int WhileHurt = 2;

    /// <summary>What still holds a thawing enemy.</summary>
    public enum Hold
    {
        /// <summary>Nothing holds it.</summary>
        None,

        /// <summary>The grapple beam holds it.</summary>
        Grappled,

        /// <summary>It is still hurt.</summary>
        Hurt,

        /// <summary>The grapple beam holds it and it is still hurt.</summary>
        GrappledAndHurt,
    }

    /// <summary>The freeze updates a thawing enemy <paramref name="hold"/> holds is left with.</summary>
    public static int Of(Hold hold) => hold switch
    {
        Hold.None => 0,
        Hold.Grappled => WhileGrappled,
        Hold.Hurt => WhileHurt,
        Hold.GrappledAndHurt => WhileGrappled + WhileHurt,
        _ => throw new ArgumentOutOfRangeException(nameof(hold), hold, "Undefined thaw hold."),
    };
}

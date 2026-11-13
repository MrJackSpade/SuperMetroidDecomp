namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What a follower reads of its parent after the parent was deleted without being killed (#1620): what the parent left.
/// A Squeept's or a Geruta's flame follows the enemy its room lists directly before it, copying its freeze and position
/// and showing itself while the parent rises or attacks. A deletion that is not a kill, such as the follow-up of a power
/// bomb killing a Dragon listed before the parent (<see cref="FollowUpAfterStruckPart"/>), leaves the parent's place
/// holding its last health, freeze and position, and leaves its last vertical speed or animation, so the flame keeps
/// following the parent as the deletion left it; a parent's kill empties its place, and its flame deletes itself.
/// </summary>
/// <remarks>
/// Consulted on every update of a flame follower, after it tests its parent's health. Evidence: the followers' routines
/// read the parent's health, freeze timer, vertical speed or installed animation and position in the preceding enemy
/// record without testing whether it still holds an enemy; a deletion clears only the record's identity, while a kill
/// clears the whole record.
/// </remarks>
[CompatibilityService("a flame follower whose parent was deleted without being killed keeps following what the parent left")]
public static class DeletedParentLeftovers
{
    /// <summary>
    /// The parent state a follower reads: its parent's current state while one occupies the place, otherwise the state the
    /// parent had when it was deleted, which the follower took when it was placed.
    /// </summary>
    /// <typeparam name="TState">The parent's species state.</typeparam>
    /// <param name="current">The state of the place's current occupant, or null once the parent was deleted.</param>
    /// <param name="left">The parent's state as the follower took it when it was placed, which the parent kept until its deletion.</param>
    public static TState Parent<TState>(TState? current, TState left)
        where TState : class =>
        current ?? Ensure.NotNull(left);
}

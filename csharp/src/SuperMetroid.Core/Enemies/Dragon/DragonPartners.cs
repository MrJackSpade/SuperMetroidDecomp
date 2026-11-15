namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>The partners one Dragon part found when it was placed (#1542).</summary>
/// <param name="Wing">A body's cosmetic wing, which its room lists right after it; null for a wing.</param>
/// <param name="FollowUpRecipient">
/// The enemy the part's follow-up to a hit reaches (<see cref="FollowUpAfterStruckPart"/>): a body's own wing, or the
/// enemy listed after a wing.
/// </param>
internal sealed record DragonPartners(RoomEnemySlot? Wing, RoomEnemySlot FollowUpRecipient);

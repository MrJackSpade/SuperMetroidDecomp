namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// The partners one Magdollite part found when it was placed (#1542). The room lists each Magdollite as three enemies
/// in a row: its head, its rising body, then its tracking overlay.
/// </summary>
/// <param name="Composite">The three places of the part's own Magdollite.</param>
/// <param name="FollowUpRecipients">
/// The two enemies the part's follow-up to a hit reaches (<see cref="FollowUpAfterStruckPart"/>), nearest first; an
/// entry is null where the room has no place that far after the part.
/// </param>
internal sealed record MagdollitePartners(MagdolliteComposite Composite, IReadOnlyList<RoomEnemyPlace?> FollowUpRecipients);

namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>
/// The partners one Evir part found when it was placed (#1542). The room lists each Evir as three enemies in a row:
/// its body, its arms, then its spit.
/// </summary>
/// <param name="Body">The body of an Evir's arms or spit, listed one or two places before it; null for a body.</param>
/// <param name="Spit">A body's spit, listed two places after it; null for the arms and the spit.</param>
/// <param name="FollowUpRecipients">
/// The two enemies a body's or its arms' follow-up to a hit reaches (<see cref="FollowUpAfterStruckPart"/>), nearest
/// first; null for the spit, which has no follow-up.
/// </param>
internal sealed record EvirPartners(RoomEnemySlot? Body, RoomEnemySlot? Spit, IReadOnlyList<RoomEnemySlot>? FollowUpRecipients);

namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>
/// The partners one Evir part found when it was placed (#1542). The room lists each Evir as three enemies in a row:
/// its body, its arms, then its spit.
/// </summary>
/// <param name="Body">The body of an Evir's arms or spit, listed one or two places before it; null for a body.</param>
/// <param name="BodyState">
/// The state of that body as the arms or spit found it; null for a body. The part keeps reaching it after the body has
/// left its place (#1543): the update a hit kills the body and deletes its arms and spit, they run once more on it,
/// reading it facing left (<see cref="KilledEvirBodyFacing"/>); and the arms and spit of a body a power bomb's follow-up
/// deleted (<see cref="FollowUpAfterStruckPart"/>) live on following that body's last facing and animation.
/// </param>
/// <param name="Spit">A body's spit, listed two places after it; null for the arms and the spit.</param>
/// <param name="FollowUpRecipients">
/// The two enemies a body's or its arms' follow-up to a hit reaches (<see cref="FollowUpAfterStruckPart"/>), nearest
/// first; null for the spit, which has no follow-up.
/// </param>
internal sealed record EvirPartners(
    RoomEnemySlot? Body,
    EvirEnemyState? BodyState,
    RoomEnemySlot? Spit,
    IReadOnlyList<RoomEnemySlot>? FollowUpRecipients)
{
    /// <summary>
    /// A spit's own state as it was placed; null for a body and its arms. Its body keeps reaching it after the spit has
    /// left its place (#1543): a power bomb that kills the arms deletes the spit by its follow-up
    /// (<see cref="FollowUpAfterStruckPart"/>), and the living body goes on reading the spit's last state.
    /// </summary>
    internal EvirEnemyState? SpitState { get; init; }
}

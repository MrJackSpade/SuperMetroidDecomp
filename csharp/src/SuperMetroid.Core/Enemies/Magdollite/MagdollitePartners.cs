namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// The partners one Magdollite part found when it was placed (#1542). The room lists each Magdollite as three enemies
/// in a row: its head, its rising body, then its tracking overlay.
/// </summary>
/// <param name="Composite">The three places of the part's own Magdollite.</param>
/// <param name="Head">The part's head (#2558), which hands itself to its body and overlay as it is placed first.</param>
/// <param name="HeadRise">How the head and its arm rise, as the head's spawn settings name, which it hands on with itself (#2474, #2554).</param>
/// <param name="FollowUpRecipients">
/// The enemies, up to two, the part's follow-up to a hit reaches (<see cref="FollowUpAfterStruckPart"/>), nearest first: as
/// many as the room places after the part (#2558).
/// </param>
internal sealed record MagdollitePartners(MagdolliteComposite Composite, RoomEnemyOccupant Head, MagdolliteHeadSpawnSettings HeadRise, IReadOnlyList<RoomEnemyPlace> FollowUpRecipients);

namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>
/// The body a pair of Ki-Hunter wings belongs to (#2558): its place and the enemy holding it, which the body hands the
/// wings its room lists right after it when it is placed, so the wings read their body's settings from the body itself.
/// A body placed again, as a respawn places it, hands its wings its new occupant.
/// </summary>
/// <param name="Place">The body's place, whose position the attached wings follow.</param>
/// <param name="Occupant">The body, whose spawn settings say whether it starts wingless.</param>
/// <param name="Settings">What the body's population entry tells it.</param>
internal sealed record KiHunterWingsBody(RoomEnemyPlace Place, RoomEnemyOccupant Occupant, KiHunterSpawnSettings Settings);

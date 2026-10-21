namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How far loading the next room has progressed when each of its enemies initializes, measured in door-scroll updates.
/// The original program initialized a room's enemies while the door kept scrolling, so which scroll update a write made
/// during loading lands in (an arriving elevator placing Samus, for example) depended on how long loading took. The game
/// loads a room in one step (<see cref="InstantRoomLoad"/>); a replay supplies the progress its recording shows.
/// </summary>
/// <remarks>
/// Consulted during each door-scroll update of a transition, before loading-time writes are applied. Evidence: the door
/// loader (<c>$82:E4A9</c>, <c>Initialise_Enemies</c>) in the SMV recordings' traces.
/// </remarks>
[CompatibilityService("how far room loading has progressed when each enemy initializes")]
public interface IRoomLoadProgress
{
    /// <summary>True once loading has initialized the enemy in slot <paramref name="slot"/>.</summary>
    /// <param name="slot">The enemy's slot in the room's enemy list.</param>
    bool HasInitializedEnemySlot(int slot);
}

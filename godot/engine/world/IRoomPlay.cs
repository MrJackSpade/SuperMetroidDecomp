namespace Engine.World;

/// <summary>
/// Whoever plays a room and has a part of its own in the room's update: play itself. The room asks it at the points
/// of the update that are play's to settle. A room stepped by nothing but itself, as a stand-in's is, has none.
/// </summary>
public interface IRoomPlay
{
    /// <summary>
    /// Called once the view has followed the player, before the room's sequences that play after the view: whatever
    /// lasts beyond the room and runs beside it plays its update here.
    /// </summary>
    void AfterView(Room room);

    /// <summary>
    /// What may follow for a player whose energy is gone on this update, asked once the room's sequences have played:
    /// whether they may begin to die, and whether what they are may save them.
    /// </summary>
    (bool MayDie, bool MayBeSaved) EmptyEnergyTerms(Room room);
}

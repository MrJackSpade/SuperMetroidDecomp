using SuperMetroid.Core.Bosses.Draygon;

namespace SuperMetroid.Core.Bosses;

/// <summary>
/// The boss room-object families a room's room-object owner runs (#1395), which boss room-object animations and frame
/// behaviours reach from their step context. The room composition implements it.
/// </summary>
internal interface IBossRoomObjects
{

    /// <summary>Handles Draygon arena cannon room objects and their block changes.</summary>
    DraygonCannonObjects DraygonCannons { get; }

}

using SuperMetroid.Core.Bosses.Draygon;

namespace SuperMetroid.Core.Bosses;

/// <summary>
/// The boss PLM families a room's PLM owner runs (#1395), which boss PLM programs and pre-instructions reach from their
/// step context. The room composition implements it.
/// </summary>
internal interface IBossRoomObjects
{

    /// <summary>Handles Draygon arena cannon room objects and their block changes.</summary>
    DraygonCannonObjects DraygonCannons { get; }

}

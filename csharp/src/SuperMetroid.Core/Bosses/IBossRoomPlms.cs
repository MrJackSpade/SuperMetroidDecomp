using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Torizo;

namespace SuperMetroid.Core.Bosses;

/// <summary>
/// The boss PLM families a room's PLM owner runs (#1395), which boss PLM programs and pre-instructions reach from their
/// step context. The room composition implements it.
/// </summary>
internal interface IBossRoomPlms
{
    /// <summary>Runs Bomb Torizo's hand and statue room object sequences.</summary>
    BombTorizoHandPlms BombTorizoHand { get; }

    /// <summary>Handles Draygon arena cannon room objects and their block changes.</summary>
    DraygonCannonPlms DraygonCannons { get; }

    /// <summary>Controls the Mother Brain chamber glass barrier and shatter sequence.</summary>
    MotherBrainGlassPlms MotherBrainGlass { get; }
}

using System.Collections.Immutable;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Draygon's wall cannons and matching projectile origins. A destroyed wall cannon's room object writes these
/// values; Draygon's firing selector reads the same values before firing.
/// </summary>
public static class DraygonCannonData
{
    /// <summary>
    /// The cannon each value of the wall-turret firing's two low random bits selects, in that order. The fourth value fires
    /// from none (#1686): the original names a bottom cannon there that no room places and Draygon's setup disables, so the
    /// firing passes with no shot, after the roll, as it does in the original.
    /// </summary>
    internal static ImmutableArray<DraygonFiringCannon?> FiringOrder { get; } =
    [
        DraygonFiringCannon.LowerLeft, DraygonFiringCannon.UpperRight, DraygonFiringCannon.LowerRight, null,
    ];

    /// <summary>The wall-turret firing fires from three cannons; this holds their wall muzzle coordinates.</summary>
    internal static DraygonCannonTarget FiringTarget(DraygonFiringCannon cannon) => cannon switch
    {
        DraygonFiringCannon.LowerLeft => new(DraygonWallCannon.LowerLeft, new(52, 303)),
        DraygonFiringCannon.UpperRight => new(DraygonWallCannon.UpperRight, new(460, 257)),
        DraygonFiringCannon.LowerRight => new(DraygonWallCannon.LowerRight, new(460, 350)),
        _ => throw new InvalidOperationException($"Undefined {nameof(DraygonFiringCannon)} {cannon}."),
    };
}

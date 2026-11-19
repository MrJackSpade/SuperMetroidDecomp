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
    /// The cannon a wall-turret firing fires from, one for each value of <see cref="DraygonRandomDefinitions.TurretChoice"/>
    /// in turn. The fourth value holds fire (#1686): the original names a bottom cannon there that no room places and
    /// Draygon's setup disables, so the firing passes with no shot, after the roll, as it does in the original.
    /// </summary>
    internal static readonly WeightedRandomChoice<DraygonTurretTurn> FiringCannon = WeightedRandomChoice<DraygonTurretTurn>.InTurn(
        DraygonRandomDefinitions.TurretChoice,
        new DraygonTurretTurn.FiresFrom(DraygonWallCannon.LowerLeft), new DraygonTurretTurn.FiresFrom(DraygonWallCannon.UpperRight),
        new DraygonTurretTurn.FiresFrom(DraygonWallCannon.LowerRight), DraygonTurretTurn.HoldsFire);

    /// <summary>The wall muzzle each cannon the wall-turret firing fires from launches its shot at.</summary>
    internal static readonly ImmutableDictionary<DraygonWallCannon, Position16<RoomPixels>> FiringOrigins =
        new Dictionary<DraygonWallCannon, Position16<RoomPixels>>
        {
            [DraygonWallCannon.LowerLeft] = new(52, 303),
            [DraygonWallCannon.UpperRight] = new(460, 257),
            [DraygonWallCannon.LowerRight] = new(460, 350),
        }.ToImmutableDictionary();
}

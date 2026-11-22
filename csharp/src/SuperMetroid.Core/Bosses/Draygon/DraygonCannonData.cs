using System.Collections.Immutable;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Draygon's wall cannons that the wall-turret firing can fire from, and where each launches its shot.
/// </summary>
public static partial class DraygonCannonData
{
    /// <summary>
    /// The cannon a wall-turret firing fires from, one for each value of
    /// <see cref="DraygonRandomDefinitions.TurretChoice"/> in turn. One choice in four holds fire (#1686): that firing
    /// passes with no shot, after its random draw.
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

using System.Collections.Immutable;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Draygon's wall cannons and matching projectile origins. Bank $84 writes these
/// words when a wall cannon is destroyed; bank $A5 reads the same words before firing.
/// </summary>
public static class DraygonCannonData
{
    /// <summary>The cannon each value of HandleFiringWallTurret's two low random bits selects, in that order.</summary>
    internal static ImmutableArray<DraygonFiringCannon> FiringOrder { get; } =
    [
        DraygonFiringCannon.LowerLeft, DraygonFiringCannon.UpperRight, DraygonFiringCannon.LowerRight, DraygonFiringCannon.UnusedBottom,
    ];

    /// <summary>$A5:87AA HandleFiringWallTurret selects four roles; $A5:87E4..87F3 holds their wall muzzle coordinates.</summary>
    internal static DraygonCannonTarget FiringTarget(DraygonFiringCannon cannon) => cannon switch
    {
        DraygonFiringCannon.LowerLeft => new(DraygonWallCannon.LowerLeft, new(52, 303)),
        DraygonFiringCannon.UpperRight => new(DraygonWallCannon.UpperRight, new(460, 257)),
        DraygonFiringCannon.LowerRight => new(DraygonWallCannon.LowerRight, new(460, 350)),
        DraygonFiringCannon.UnusedBottom => new(DraygonWallCannon.UnusedBottom, new(444, 392)),
        _ => throw new InvalidOperationException($"Undefined {nameof(DraygonFiringCannon)} {cannon}."),
    };
}

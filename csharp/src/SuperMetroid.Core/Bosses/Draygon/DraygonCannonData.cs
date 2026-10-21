using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Draygon's cannon-control words and matching projectile origins. Bank $84 writes these
/// words when a wall cannon is destroyed; bank $A5 reads the same words before firing.
/// </summary>
public static class DraygonCannonData
{
    /// <summary>$A5:87AA HandleFiringWallTurret selects four roles; $A5:87E4..87F3 holds their wall muzzle coordinates.</summary>
    internal static DraygonCannonTarget FiringTarget(DraygonFiringCannon cannon) => cannon switch
    {
        DraygonFiringCannon.LowerLeft => new(DraygonCannonControlWord.LowerLeft, new(52, 303)),
        DraygonFiringCannon.UpperRight => new(DraygonCannonControlWord.UpperRight, new(460, 257)),
        DraygonFiringCannon.LowerRight => new(DraygonCannonControlWord.LowerRight, new(460, 350)),
        DraygonFiringCannon.UnusedBottom => new(DraygonCannonControlWord.UnusedBottom, new(444, 392)),
        _ => throw new InvalidOperationException($"Undefined {nameof(DraygonFiringCannon)} {(int)cannon}."),
    };
}

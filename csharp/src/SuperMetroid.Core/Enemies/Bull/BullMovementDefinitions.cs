using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>Immutable authored Bull shot-reaction angles; live timers remain enemy state.</summary>
public static class BullMovementDefinitions
{
    /// <summary>
    /// The compass angle a shot in each direction moves the Bull at: up and down whichever way Samus faces, and each
    /// distinct direction an eighth of a turn (32 of the angle's 256ths) from its neighbours.
    /// </summary>
    internal static readonly SamusProjectileDirectionMap<TrigonometryAngle> ShotAngles = new(
        new(192), new(224), new(0), new(32), new(64), new(64), new(96), new(128), new(160), new(192));
}

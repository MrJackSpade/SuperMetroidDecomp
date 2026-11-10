using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>Immutable authored Bull shot-reaction angles; live timers remain enemy state.</summary>
public static class BullMovementDefinitions
{
    /// <summary>
    /// The compass angle a shot in each direction moves the Bull at: up and down whichever way Samus faces, and each
    /// distinct direction a 32-unit octant from its neighbours.
    /// </summary>
    internal static readonly SamusProjectileDirectionMap<int> ShotAngles = new(192, 224, 0, 32, 64, 64, 96, 128, 160, 192);
}

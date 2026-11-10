using SuperMetroid.Core.Samus.Projectiles;
using static SuperMetroid.Core.Enemies.Bull.BullMovementDefinitions;

namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>A Bull's shot-reaction angle.</summary>
public static class BullMotion
{
    /// <summary>The compass angle an immune shot fired in <paramref name="direction"/> moves the Bull at.</summary>
    internal static int ShotAngle(SamusProjectileDirection direction) => ShotAngles.For(direction);
}

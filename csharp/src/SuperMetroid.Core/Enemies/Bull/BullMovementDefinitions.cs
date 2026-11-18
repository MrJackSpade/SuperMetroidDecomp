using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>How a Bull accelerates, waits, retargets and reacts to a shot (#2279); live timers remain enemy state.</summary>
public static class BullMovementDefinitions
{
    /// <summary>How much each acceleration or deceleration tick changes a Bull's acceleration.</summary>
    internal static readonly SignedEightEight AccelerationStep = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(24);

    /// <summary>The acceleration an immune shot's kick starts the Bull's braking with: a pixel an update.</summary>
    internal static readonly SignedEightEight ShotKickAcceleration = SignedEightEight.FromPixels(1);

    /// <summary>The speed an immune shot kicks the Bull away at: six pixels an update.</summary>
    internal static readonly SignedEightEight ShotKickSpeed = SignedEightEight.FromPixels(6);

    /// <summary>How long a Bull waits before it retargets Samus, in updates.</summary>
    internal const int MovementDelay = 16;

    /// <summary>How long a Bull ignores further shots after a shot kicks it, in updates.</summary>
    internal const int ShotReactionGuardDuration = 48;

    /// <summary>The turn from a Bull's heading to Samus's bearing, in 256ths of a turn, from which it is off target and brakes.</summary>
    internal const int OffTargetTurn = 48;

    /// <summary>
    /// The compass angle a shot in each direction moves the Bull at: up and down whichever way Samus faces, and each
    /// distinct direction an eighth of a turn (32 of the angle's 256ths) from its neighbours.
    /// </summary>
    internal static readonly SamusProjectileDirectionMap<TrigonometryAngle> ShotAngles = new(
        new(192), new(224), new(0), new(32), new(64), new(64), new(96), new(128), new(160), new(192));

    /// <summary>
    /// The compass angle a bomb that leaves the Bull unhurt kicks it at: straight up, as a shot fired straight up does,
    /// whichever side of the Bull the bomb burns on.
    /// </summary>
    internal static readonly TrigonometryAngle BombKickAngle = new(192);
}

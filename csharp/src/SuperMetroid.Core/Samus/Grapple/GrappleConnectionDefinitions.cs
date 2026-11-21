using System.Collections.Frozen;
using static SuperMetroid.Core.Samus.Grapple.GrappleConnectionKind;
using static SuperMetroid.Core.Samus.Poses.SamusPoseId;
using static SuperMetroid.Core.Samus.Projectiles.SamusProjectileDirection;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Grapple connection, cancellation and dropped-pose mechanics, not visual assets.</summary>
internal static class GrappleConnectionDefinitions
{
    /// <summary>
    /// The connection each policy makes for each of the ten firing directions, and the pose it puts Samus in (#2286).
    /// Moving vertically always swings, in the right-swinging pose for the five directions from up (facing right) to
    /// down (facing right) and the left-swinging pose for the other five. Standing and crouching swing from the upward
    /// diagonals and verticals and lock Samus in place otherwise; crouching locks in its own crouching poses except
    /// when aiming straight down, where both policies use the standing down-left pose.
    /// </summary>
    internal static readonly FrozenDictionary<GrappleConnectionPolicy, FrozenDictionary<SamusProjectileDirection, GrappleConnection>> Directional =
        new Dictionary<GrappleConnectionPolicy, FrozenDictionary<SamusProjectileDirection, GrappleConnection>>
        {
            [GrappleConnectionPolicy.Default] = new Dictionary<SamusProjectileDirection, GrappleConnection>
            {
                [UpFacingRight] = new(Swinging, GrappleSwingRightPose),
                [UpRight] = new(Swinging, GrappleSwingRightPose),
                [Right] = new(LockedInPlace, GrappleStandingRightPose),
                [DownRight] = new(LockedInPlace, GrappleStandingDownRightPose),
                [DownFacingRight] = new(LockedInPlace, GrappleStandingDownLeftPose),
                [DownFacingLeft] = new(LockedInPlace, GrappleStandingDownLeftPose),
                [DownLeft] = new(LockedInPlace, GrappleStandingDownLeftPose),
                [Left] = new(LockedInPlace, GrappleStandingLeftPose),
                [UpLeft] = new(Swinging, GrappleSwingLeftPose),
                [UpFacingLeft] = new(Swinging, GrappleSwingLeftPose),
            }.ToFrozenDictionary(),
            [GrappleConnectionPolicy.MovingVertically] = new Dictionary<SamusProjectileDirection, GrappleConnection>
            {
                [UpFacingRight] = new(Swinging, GrappleSwingRightPose),
                [UpRight] = new(Swinging, GrappleSwingRightPose),
                [Right] = new(Swinging, GrappleSwingRightPose),
                [DownRight] = new(Swinging, GrappleSwingRightPose),
                [DownFacingRight] = new(Swinging, GrappleSwingRightPose),
                [DownFacingLeft] = new(Swinging, GrappleSwingLeftPose),
                [DownLeft] = new(Swinging, GrappleSwingLeftPose),
                [Left] = new(Swinging, GrappleSwingLeftPose),
                [UpLeft] = new(Swinging, GrappleSwingLeftPose),
                [UpFacingLeft] = new(Swinging, GrappleSwingLeftPose),
            }.ToFrozenDictionary(),
            [GrappleConnectionPolicy.Crouching] = new Dictionary<SamusProjectileDirection, GrappleConnection>
            {
                [UpFacingRight] = new(Swinging, GrappleSwingRightPose),
                [UpRight] = new(Swinging, GrappleSwingRightPose),
                [Right] = new(LockedInPlace, GrappleCrouchingRightPose),
                [DownRight] = new(LockedInPlace, GrappleCrouchingDownRightPose),
                [DownFacingRight] = new(LockedInPlace, GrappleStandingDownLeftPose),
                [DownFacingLeft] = new(LockedInPlace, GrappleStandingDownLeftPose),
                [DownLeft] = new(LockedInPlace, GrappleCrouchingDownLeftPose),
                [Left] = new(LockedInPlace, GrappleCrouchingLeftPose),
                [UpLeft] = new(Swinging, GrappleSwingLeftPose),
                [UpFacingLeft] = new(Swinging, GrappleSwingLeftPose),
            }.ToFrozenDictionary(),
        }.ToFrozenDictionary();

    /// <summary>The turn a stop's reflection mirrors it about: its reflected angle is this turn less its own, one whole step.</summary>
    internal static readonly RotationAngle ReflectionTurn = RotationAngle.FromSteps(1);

    /// <summary>
    /// The grapple's four authored collision stops. Each has a left/right reflection on the other side, and a swing
    /// compares the eight exact angles: the angle reflects as <see cref="ReflectionTurn"/> less the stop's own, X negates
    /// and the pose swaps facing; Y and the phase are shared.
    /// </summary>
    internal static readonly SpecialConnection[] AuthoredStops =
    [
        new(RotationAngle.FromStepFraction(429, 2), GrappleCrouchingDownRightPose, new(-30, -24), GrapplePhase.ConnectedLocked),
        new(RotationAngle.FromStepFraction(359, 2), GrappleCrouchingDownRightPose, new(-28, -8), GrapplePhase.ConnectedLocked),
        new(RotationAngle.FromStepFraction(213, 2), GrappleWallContactRightPose, new(24, 16), GrapplePhase.WallGrab),
        new(RotationAngle.FromStepFraction(231, 2), GrappleWallContactLeftPose, new(-8, 16), GrapplePhase.WallGrab),
    ];

    /// <summary>One rope angle at which the swing stops, with the pose and place Samus stops in.</summary>
    /// <param name="Angle">The rope angle that triggers the stop.</param>
    /// <param name="Pose">Samus's pose at the stop.</param>
    /// <param name="SamusOffset">Samus's room-pixel position relative to the grapple anchor.</param>
    /// <param name="Phase">The phase the stop holds the grapple in: locked in place, or grabbing the wall (#2553).</param>
    internal readonly record struct SpecialConnection(RotationAngle Angle, SamusPoseId Pose, Offset16<RoomPixels> SamusOffset, GrapplePhase Phase);
}

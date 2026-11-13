using System.Collections.Frozen;
using static SuperMetroid.Core.Samus.Grapple.GrappleConnectionFunction;
using static SuperMetroid.Core.Samus.Grapple.GrappleConnectionHandler;
using static SuperMetroid.Core.Samus.Poses.SamusPoseId;
using static SuperMetroid.Core.Samus.Projectiles.SamusProjectileDirection;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Grapple connection, cancellation and dropped-pose mechanics, not visual assets.</summary>
internal static class GrappleConnectionDefinitions
{
    /// <summary>
    /// The connection each policy installs for each of the ten firing directions. Every direction has its own row in
    /// every policy: firing admits only these ten directions, so no connection reads another policy's rows. Moving
    /// vertically always swings, clockwise for the five directions from up (facing right) to down (facing right) and
    /// anticlockwise for the other five. Standing and crouching swing from the upward diagonals and verticals and lock
    /// Samus in place otherwise; crouching locks in its own crouching poses except when aiming down, where both
    /// policies use the standing down handler.
    /// </summary>
    internal static readonly FrozenDictionary<GrappleConnectionTable, FrozenDictionary<SamusProjectileDirection, GrappleConnection>> Directional =
        new Dictionary<GrappleConnectionTable, FrozenDictionary<SamusProjectileDirection, GrappleConnection>>
        {
            [GrappleConnectionTable.Default] = new Dictionary<SamusProjectileDirection, GrappleConnection>
            {
                [UpFacingRight] = new(Swinging, SwingClockwise),
                [UpRight] = new(Swinging, SwingClockwise),
                [Right] = new(LockedInPlace, StandingUpRight),
                [DownRight] = new(LockedInPlace, StandingRight),
                [DownFacingRight] = new(LockedInPlace, StandingDown),
                [DownFacingLeft] = new(LockedInPlace, StandingDown),
                [DownLeft] = new(LockedInPlace, StandingDown),
                [Left] = new(LockedInPlace, StandingUpLeft),
                [UpLeft] = new(Swinging, SwingAnticlockwise),
                [UpFacingLeft] = new(Swinging, SwingAnticlockwise),
            }.ToFrozenDictionary(),
            [GrappleConnectionTable.MovingVertically] = new Dictionary<SamusProjectileDirection, GrappleConnection>
            {
                [UpFacingRight] = new(Swinging, SwingClockwise),
                [UpRight] = new(Swinging, SwingClockwise),
                [Right] = new(Swinging, SwingClockwise),
                [DownRight] = new(Swinging, SwingClockwise),
                [DownFacingRight] = new(Swinging, SwingClockwise),
                [DownFacingLeft] = new(Swinging, SwingAnticlockwise),
                [DownLeft] = new(Swinging, SwingAnticlockwise),
                [Left] = new(Swinging, SwingAnticlockwise),
                [UpLeft] = new(Swinging, SwingAnticlockwise),
                [UpFacingLeft] = new(Swinging, SwingAnticlockwise),
            }.ToFrozenDictionary(),
            [GrappleConnectionTable.Crouching] = new Dictionary<SamusProjectileDirection, GrappleConnection>
            {
                [UpFacingRight] = new(Swinging, SwingClockwise),
                [UpRight] = new(Swinging, SwingClockwise),
                [Right] = new(LockedInPlace, CrouchingUpRight),
                [DownRight] = new(LockedInPlace, CrouchingRight),
                [DownFacingRight] = new(LockedInPlace, StandingDown),
                [DownFacingLeft] = new(LockedInPlace, StandingDown),
                [DownLeft] = new(LockedInPlace, CrouchingDownLeft),
                [Left] = new(LockedInPlace, CrouchingUpLeft),
                [UpLeft] = new(Swinging, SwingAnticlockwise),
                [UpFacingLeft] = new(Swinging, SwingAnticlockwise),
            }.ToFrozenDictionary(),
        }.ToFrozenDictionary();

    /// <summary>The turn a stop's reflection mirrors it about: its reflected angle is this turn less its own, one whole step.</summary>
    internal static readonly RotationAngle ReflectionTurn = RotationAngle.FromSteps(1);

    /// <summary>
    /// The grapple's four authored collision stops. Each has a left/right reflection on the other side, and a swing
    /// compares the eight exact angles: the angle reflects as <see cref="ReflectionTurn"/> less the stop's own, X negates
    /// and the pose swaps facing; Y and the next function are shared.
    /// </summary>
    internal static readonly SpecialConnection[] AuthoredStops =
    [
        new(RotationAngle.FromSteps(214, 128), GrappleCrouchingDownRightPose, new(-30, -24), GrappleConnectionFunction.LockedInPlace),
        new(RotationAngle.FromSteps(179, 128), GrappleCrouchingDownRightPose, new(-28, -8), GrappleConnectionFunction.LockedInPlace),
        new(RotationAngle.FromSteps(106, 128), GrappleWallContactRightPose, new(24, 16), GrappleConnectionFunction.WallGrab),
        new(RotationAngle.FromSteps(115, 128), GrappleWallContactLeftPose, new(-8, 16), GrappleConnectionFunction.WallGrab),
    ];

    /// <summary>One collision-stop record.</summary>
    /// <param name="Angle">The rope angle that triggers the stop.</param>
    /// <param name="Pose">Samus's pose at the stop.</param>
    /// <param name="SamusOffset">Samus's room-pixel position relative to the grapple anchor.</param>
    /// <param name="Function">The connection function installed at the stop.</param>
    internal readonly record struct SpecialConnection(RotationAngle Angle, SamusPoseId Pose, Offset16<RoomPixels> SamusOffset, GrappleConnectionFunction Function);
}

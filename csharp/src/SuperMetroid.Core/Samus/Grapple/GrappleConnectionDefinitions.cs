using static SuperMetroid.Core.Samus.Poses.SamusPoseId;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Native Grapple connection, cancellation and dropped-pose mechanics, not visual assets.</summary>
internal static class GrappleConnectionDefinitions
{
    /// <summary>The raw angle a stop's reflection mirrors it about: its reflected angle is this minus its own.</summary>
    internal const int ReflectionAngle = 0x0100;

    /// <summary>
    /// $9B:C43E GrappleBeamSpecialAngles stores eight exact collision-stop records. They are four
    /// authored stops, each immediately followed by its left/right reflection: the angle reflects
    /// as $0100 - angle, X negates and the pose swaps facing; Y and the next function are shared.
    /// </summary>
    internal static readonly SpecialConnection[] AuthoredStops =
    [
        new(RotationAngle.FromTableIndex(0xd6, 0x80), GrappleCrouchingDownRightPose, new(-30, -24), GrappleConnectionFunction.LockedInPlace),
        new(RotationAngle.FromTableIndex(0xb3, 0x80), GrappleCrouchingDownRightPose, new(-28, -8), GrappleConnectionFunction.LockedInPlace),
        new(RotationAngle.FromTableIndex(0x6a, 0x80), GrappleWallContactRightPose, new(24, 16), GrappleConnectionFunction.WallGrab),
        new(RotationAngle.FromTableIndex(0x73, 0x80), GrappleWallContactLeftPose, new(-8, 16), GrappleConnectionFunction.WallGrab),
    ];

    /// <summary>One collision-stop record.</summary>
    /// <param name="Angle">The rope angle that triggers the stop.</param>
    /// <param name="Pose">Samus's pose at the stop.</param>
    /// <param name="SamusOffset">Samus's room-pixel position relative to the grapple anchor.</param>
    /// <param name="Function">The connection function installed at the stop.</param>
    internal readonly record struct SpecialConnection(RotationAngle Angle, SamusPoseId Pose, Offset16<RoomPixels> SamusOffset, GrappleConnectionFunction Function);

    /// <summary>Number of native special-angle records, in their native order.</summary>
    internal static int SpecialAngleCount => AuthoredStops.Length * 2;
}

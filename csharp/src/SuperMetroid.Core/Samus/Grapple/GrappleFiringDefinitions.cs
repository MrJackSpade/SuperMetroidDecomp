using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Physical launch and hand-anchor definitions; flare artwork placement is separate.</summary>
internal static class GrappleFiringDefinitions
{
    /// <summary>Launch speed along an axis: twelve pixels an update times the sine of a right angle, 255/256.</summary>
    internal static readonly SignedEightEight CardinalSpeed = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(12 * 255);
    /// <summary>Each component of a diagonal launch: twelve pixels an update times the sine of an eighth turn, 181/256.</summary>
    internal static readonly SignedEightEight DiagonalSpeed = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(12 * 181);

    /// <summary>
    /// Each firing direction's launch velocity and angle (#2280): the axis and diagonal speeds, the sine scaled by twelve
    /// before rounding, never a rounded circle. The angle is a half turn for up and turns on an eighth of a turn for each
    /// eighth of the compass clockwise; both upward and both downward directions share their velocity and angle.
    /// </summary>
    internal static readonly SamusProjectileDirectionMap<GrappleLaunchDefinition> Launches = new(
        UpFacingRight: new(new(SignedEightEight.Zero, -CardinalSpeed), RotationAngle.HalfTurn),
        UpRight: new(new(DiagonalSpeed, -DiagonalSpeed), RotationAngle.HalfTurn + RotationAngle.EighthTurn),
        Right: new(new(CardinalSpeed, SignedEightEight.Zero), RotationAngle.ThreeQuarterTurn),
        DownRight: new(new(DiagonalSpeed, DiagonalSpeed), RotationAngle.ThreeQuarterTurn + RotationAngle.EighthTurn),
        DownFacingRight: new(new(SignedEightEight.Zero, CardinalSpeed), RotationAngle.Zero),
        DownFacingLeft: new(new(SignedEightEight.Zero, CardinalSpeed), RotationAngle.Zero),
        DownLeft: new(new(-DiagonalSpeed, DiagonalSpeed), RotationAngle.EighthTurn),
        Left: new(new(-CardinalSpeed, SignedEightEight.Zero), RotationAngle.QuarterTurn),
        UpLeft: new(new(-DiagonalSpeed, -DiagonalSpeed), RotationAngle.QuarterTurn + RotationAngle.EighthTurn),
        UpFacingLeft: new(new(SignedEightEight.Zero, -CardinalSpeed), RotationAngle.HalfTurn));

    /// <summary>
    /// Where the beam leaves Samus's hand for each firing direction while she is not running (#2280): up and down facings
    /// stay distinct, and straight down sits three pixels right facing right but four left facing left.
    /// </summary>
    internal static readonly SamusProjectileDirectionMap<Offset16<RoomPixels>> StandingOrigins = new(
        UpFacingRight: new(2, -16),
        UpRight: new(10, -12),
        Right: new(2, 2),
        DownRight: new(10, 0),
        DownFacingRight: new(3, 6),
        DownFacingLeft: new(-4, 6),
        DownLeft: new(-10, 0),
        Left: new(-2, 2),
        UpLeft: new(-10, -12),
        UpFacingLeft: new(-2, -16));

    /// <summary>Where the beam leaves Samus's hand while she runs: the horizontal directions' anchors are four pixels higher.</summary>
    internal static readonly SamusProjectileDirectionMap<Offset16<RoomPixels>> RunningOrigins = StandingOrigins with
    {
        Right = new(2, -2),
        Left = new(-2, -2),
    };
}

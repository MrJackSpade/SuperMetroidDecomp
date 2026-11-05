namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>
/// The Ceres elevator shaft's turning floor: how far the floor has turned Samus's surroundings about its centre, which
/// her drawing (and every sprite drawn over the floor) applies to its origin while the shaft turns.
/// </summary>
/// <remarks>
/// This is deliberately a presentation value, not part of Samus's kinematics. The drawing turns a copy of the world
/// point and places the sprite there; collision, camera tracking and the following frame's starting position keep the
/// untransformed point. Keeping the transform immutable makes it difficult for drawing code to turn them by accident.
/// </remarks>
/// <param name="Angle">How far the floor is turned clockwise on the screen, in radians; the sprites over it turn with it.</param>
/// <param name="Center">The room point the shaft's turning floor turns about.</param>
public readonly record struct TurningFloorTransform(double Angle, Position16<RoomPixels> Center)
{
    /// <summary>The floor of a shaft that has not turned.</summary>
    /// <param name="center">The room point the shaft's turning floor turns about.</param>
    public static TurningFloorTransform Level(Position16<RoomPixels> center) => new(0, center);

    /// <summary>
    /// Where <paramref name="world"/> shows once the floor has turned: the point turned clockwise by <see cref="Angle"/>
    /// about <see cref="Center"/>, to the nearest pixel.
    /// </summary>
    public Position16<RoomPixels> Transform(Position16<RoomPixels> world)
    {
        Offset16<RoomPixels> fromCentre = world - Center;
        double cosine = Math.Cos(Angle), sine = Math.Sin(Angle);
        Offset16<RoomPixels> turned = Offset16<RoomPixels>.Wrapped(
            (int)Math.Round(fromCentre.X * cosine - fromCentre.Y * sine),
            (int)Math.Round(fromCentre.X * sine + fromCentre.Y * cosine));
        return Center + turned;
    }
}

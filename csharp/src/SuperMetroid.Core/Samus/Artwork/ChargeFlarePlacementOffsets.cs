using SuperMetroid.Core.Samus.Projectiles;
using static SuperMetroid.Core.Samus.Artwork.ChargeFlarePlacementDefinitions;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Where the charge flare is placed for each beam and grapple pose.</summary>
public static class ChargeFlarePlacementOffsets
{
    /// <summary>Formats the exact JSON identity for one movement-mode/direction placement; direction range validation belongs to catalog loading or lookup, not this formatter.</summary>
    /// <param name="running">True for the running visual-origin table; false for standing.</param>
    /// <param name="direction">One of the ten named aiming directions; formatted as two decimal digits rather than hexadecimal.</param>
    /// <returns>A key such as <c>standing-00</c> or <c>running-09</c>.</returns>
    public static string Key(bool running, int direction) => $"{(running ? "running" : "standing")}-{direction:D2}";

    /// <summary>
    /// The visual muzzle origins of the ten named aiming directions. Physical origins remain separate. The game's tables
    /// carry rows past them, which only a shot direction no flare draw admits would select, so they are not modelled
    /// (#1500).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The direction is not one of the ten named aiming directions.</exception>
    internal static Offset16<RoomPixels> BeamOffset(bool running, int direction)
    {
        if ((uint)direction >= DirectionCount) throw new ArgumentOutOfRangeException(nameof(direction));
        return new SamusFiringStance(running, SamusProjectileDirectionCodec.FromAimIndex(direction)) switch
        {
            (false, SamusProjectileDirection.UpFacingRight) => new Offset16<RoomPixels>(2, -28),
            (false, SamusProjectileDirection.UpRight) => new Offset16<RoomPixels>(18, -19),
            (false, SamusProjectileDirection.Right) => new Offset16<RoomPixels>(15, 1),
            (false, SamusProjectileDirection.DownRight) => new Offset16<RoomPixels>(17, 6),
            (false, SamusProjectileDirection.DownFacingRight) => new Offset16<RoomPixels>(3, 17),
            (false, SamusProjectileDirection.DownFacingLeft) => new Offset16<RoomPixels>(-4, 17),
            (false, SamusProjectileDirection.DownLeft) => new Offset16<RoomPixels>(-17, 6),
            (false, SamusProjectileDirection.Left) => new Offset16<RoomPixels>(-15, 1),
            (false, SamusProjectileDirection.UpLeft) => new Offset16<RoomPixels>(-18, -20),
            (false, SamusProjectileDirection.UpFacingLeft) => new Offset16<RoomPixels>(-2, -28),
            (true, SamusProjectileDirection.UpFacingRight) => new Offset16<RoomPixels>(2, -32),
            (true, SamusProjectileDirection.UpRight) => new Offset16<RoomPixels>(19, -22),
            (true, SamusProjectileDirection.Right) => new Offset16<RoomPixels>(20, -3),
            (true, SamusProjectileDirection.DownRight) => new Offset16<RoomPixels>(18, 6),
            (true, SamusProjectileDirection.DownFacingRight) => new Offset16<RoomPixels>(3, 25),
            (true, SamusProjectileDirection.DownFacingLeft) => new Offset16<RoomPixels>(-4, 25),
            (true, SamusProjectileDirection.DownLeft) => new Offset16<RoomPixels>(-18, 6),
            (true, SamusProjectileDirection.Left) => new Offset16<RoomPixels>(-20, -3),
            (true, SamusProjectileDirection.UpLeft) => new Offset16<RoomPixels>(-19, -20),
            (true, SamusProjectileDirection.UpFacingLeft) => new Offset16<RoomPixels>(-2, -32),
            _ => throw new InvalidOperationException("Beam visual origin requires a named aiming direction."),
        };
    }

    /// <summary>Grapple's ten visual origins, which equal the beam origins.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The direction is not one of the ten named aiming directions.</exception>
    internal static Offset16<RoomPixels> GrappleOffset(bool running, int direction) => BeamOffset(running, direction);
}

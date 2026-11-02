using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Projectiles;
using static SuperMetroid.Core.Samus.Artwork.ChargeFlarePlacementDefinitions;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Where the charge flare is placed for each beam and grapple pose.</summary>
public static class ChargeFlarePlacementOffsets
{
    /// <summary>
    /// The visual muzzle origins of the ten named aiming directions and, standing, of the three turning placements.
    /// Physical origins remain separate. The game's tables carry rows past them, which no pose selects, so they are not
    /// modelled (#1500).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">No pose of the movement mode selects the direction.</exception>
    internal static Offset16<RoomPixels> BeamOffset(bool running, int direction)
    {
        if ((uint)direction >= (running ? DirectionCount : StandingDirectionCount)) throw new ArgumentOutOfRangeException(nameof(direction));
        if (direction >= DirectionCount)
            return TurningOffset(TurningPlacements[direction - DirectionCount]);
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

    /// <summary>The beam flare's placement while Samus turns holding <paramref name="aim"/>.</summary>
    private static Offset16<RoomPixels> TurningOffset(TurningAim aim) => new()
    {
        X = -4,
        Y = aim switch
        {
            TurningAim.Up => -20,
            TurningAim.Neutral => -2,
            TurningAim.Down => 8,
            _ => throw new InvalidOperationException($"Undefined {nameof(TurningAim)} {aim}."),
        },
    };

    /// <summary>The visual origins of Grapple's ten firing directions, which equal the beam origins.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The direction is not one of Grapple's firing directions.</exception>
    internal static Offset16<RoomPixels> GrappleOffset(bool running, int direction) =>
        (uint)direction < GrappleBeamDefinitions.Firing.DirectionCount
            ? BeamOffset(running, direction)
            : throw new ArgumentOutOfRangeException(nameof(direction));
}

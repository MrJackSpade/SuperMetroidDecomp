using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Where the charge flare is placed for each beam and grapple pose.</summary>
public static class ChargeFlarePlacementOffsets
{
    /// <summary>
    /// The visual muzzle origins of the ten aiming directions and, standing, of the three turning placements.
    /// Physical origins remain separate. The game's tables carry rows past them, which no pose selects, so they are not
    /// modelled (#1500).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">No pose of the movement mode places the flare there.</exception>
    internal static Offset16<RoomPixels> BeamOffset(bool running, SamusFlareAim aim)
    {
        if (aim.Turning is { } turning)
            return running
                ? throw new ArgumentOutOfRangeException(nameof(aim), aim, "No running pose places the flare while turning.")
                : TurningOffset(turning);
        SamusProjectileDirection direction = aim.Muzzle
            ?? throw new ArgumentOutOfRangeException(nameof(aim), aim, "A flare is placed at a muzzle or while turning.");
        return new SamusFiringStance(running, direction) switch
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
            _ => throw new ArgumentOutOfRangeException(nameof(aim), aim, "Beam visual origin requires a defined aiming direction."),
        };
    }

    /// <summary>The beam flare's placement while Samus turns holding <paramref name="aim"/>.</summary>
    private static Offset16<RoomPixels> TurningOffset(SamusTurningAim aim) => new()
    {
        X = -4,
        Y = aim switch
        {
            SamusTurningAim.Up => -20,
            SamusTurningAim.Neutral => -2,
            SamusTurningAim.Down => 8,
            _ => throw new InvalidOperationException($"Undefined {nameof(SamusTurningAim)} {aim}."),
        },
    };

    /// <summary>The visual origins of Grapple's ten firing directions, which equal the beam origins.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The flare is placed while turning, when Grapple never fires.</exception>
    internal static Offset16<RoomPixels> GrappleOffset(bool running, SamusFlareAim aim) =>
        aim.Muzzle is not null
            ? BeamOffset(running, aim)
            : throw new ArgumentOutOfRangeException(nameof(aim), aim, "Grapple never fires while turning.");
}

using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Visual flare placement resource contract, including native low-nibble overread selections.</summary>
public static class ChargeFlarePlacementDefinitions
{
    /// <summary>JSON resource filename for beam-charge flare drawing offsets; editing these signed pixel placements does not change physical projectile origins or charge timing.</summary>
    public const string FileName = "charge-flare-placement.json";
    /// <summary>Required revision of the shared beam/Grapple placement schema, containing all sixteen standing and sixteen running direction keys.</summary>
    public const int Version = 1;
    /// <summary>The renderer retains all four direction bits, including values beyond named directions.</summary>
    public const int DirectionCount = 16;
    /// <summary>Formats the exact JSON identity for one movement-mode/direction placement; direction range validation belongs to catalog loading or lookup, not this formatter.</summary>
    /// <param name="running">True for the running visual-origin table; false for standing, including the native turning/adjacent-row selections.</param>
    /// <param name="direction">Full native low-nibble selector 0..15, including bounded overreads beyond the ten named aiming directions; formatted as two decimal digits rather than hexadecimal.</param>
    /// <returns>A key such as <c>standing-00</c> or <c>running-15</c>.</returns>
    public static string Key(bool running, int direction) => $"{(running ? "running" : "standing")}-{direction:D2}";
    /// <summary>$90:C1BC/C1D6: standing turn aiming up or diagonally up.</summary>
    private const int TurningUp = 10;
    /// <summary>$90:C1BE/C1D8: standing turn with neutral aim.</summary>
    private const int TurningNeutral = 11;
    /// <summary>$90:C1C0/C1DA: standing turn aiming down or diagonally down.</summary>
    private const int TurningDown = 12;
    /// <summary>$90:C1A8/C1C2 have thirteen named standing origins; running rows C1DC/C1F0 have ten.</summary>
    private const int StandingDirectionCount = 13;

    /// <summary>
    /// $90:C1A8..C203 named visual muzzle origins, including exact adjacent-row aliases
    /// for the renderer's full low-nibble domain. Physical origins remain separate.
    /// </summary>
    internal static ChargeFlareOffset BeamOffset(bool running, int direction)
    {
        if ((uint)direction >= DirectionCount) throw new ArgumentOutOfRangeException(nameof(direction));
        if (running && direction >= SamusProjectileRomData.Origins.DirectionCount)
        {
            int adjacent = direction - SamusProjectileRomData.Origins.DirectionCount;
            return new() { X = BeamOffset(true, adjacent).Y,
                Y = SamusProjectileOriginDefinitions.Read(false, (ushort)adjacent).X };
        }
        if (!running && direction >= StandingDirectionCount)
        {
            int adjacent = direction - StandingDirectionCount;
            return new() { X = BeamOffset(false, adjacent).Y, Y = BeamOffset(true, adjacent).X };
        }
        if (!running && direction >= SamusProjectileRomData.Origins.DirectionCount)
            return new() { X = -4, Y = direction switch { TurningUp => -20, TurningNeutral => -2, TurningDown => 8, _ => throw new InvalidOperationException() } };
        (int x, int y) = (running, (SamusProjectileDirection)direction) switch
        {
            (false, SamusProjectileDirection.UpFacingRight) => (2, -28),
            (false, SamusProjectileDirection.UpRight) => (18, -19),
            (false, SamusProjectileDirection.Right) => (15, 1),
            (false, SamusProjectileDirection.DownRight) => (17, 6),
            (false, SamusProjectileDirection.DownFacingRight) => (3, 17),
            (false, SamusProjectileDirection.DownFacingLeft) => (-4, 17),
            (false, SamusProjectileDirection.DownLeft) => (-17, 6),
            (false, SamusProjectileDirection.Left) => (-15, 1),
            (false, SamusProjectileDirection.UpLeft) => (-18, -20),
            (false, SamusProjectileDirection.UpFacingLeft) => (-2, -28),
            (true, SamusProjectileDirection.UpFacingRight) => (2, -32),
            (true, SamusProjectileDirection.UpRight) => (19, -22),
            (true, SamusProjectileDirection.Right) => (20, -3),
            (true, SamusProjectileDirection.DownRight) => (18, 6),
            (true, SamusProjectileDirection.DownFacingRight) => (3, 25),
            (true, SamusProjectileDirection.DownFacingLeft) => (-4, 25),
            (true, SamusProjectileDirection.DownLeft) => (-18, 6),
            (true, SamusProjectileDirection.Left) => (-20, -3),
            (true, SamusProjectileDirection.UpLeft) => (-19, -20),
            (true, SamusProjectileDirection.UpFacingLeft) => (-2, -32),
            _ => throw new InvalidOperationException("Beam visual origin requires a named native direction."),
        };
        return new() { X = (short)x, Y = (short)y };
    }
    /// <summary>
    /// $9B:C14A/C15E and C19A/C1AE: Grapple's ten visual origins equal the beam origins.
    /// Overread directions use adjacent physical-origin and swing-selector row owners.
    /// </summary>
    internal static ChargeFlareOffset GrappleOffset(bool running, int direction)
    {
        if ((uint)direction >= DirectionCount) throw new ArgumentOutOfRangeException(nameof(direction));
        if (direction < SamusGrappleRomData.Firing.DirectionCount) return BeamOffset(running, direction);
        int adjacent = direction - SamusGrappleRomData.Firing.DirectionCount;
        short y = running ? unchecked((short)(SwingSelectorByte(adjacent * 2) | SwingSelectorByte(adjacent * 2 + 1) << 8))
            : GrappleFiringDefinitions.Origin((byte)adjacent, true).X;
        return new() { X = BeamOffset(running, adjacent).Y, Y = y };
    }

    /// <summary>$9B:C1C2 GrappleSwingSamusXYOffsets: nearest eight-angle bucket, modulo32; its first twelve bytes bound the running-flare Y alias.</summary>
    private static int SwingSelectorByte(int angle) => ((angle + 4) / 8) % 32;
}

using System.Buffers.Binary;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Where each firing direction's projectile leaves the arm cannon.</summary>
internal static class SamusMuzzleOrigins
{
    /// <summary>$90:C204..C253 ProjectileOriginOffsetsByDirection: physical muzzle coordinates for each named firing direction and running/Moonwalk mode.</summary>
    private static Offset16<RoomPixels> Muzzle(bool running, SamusProjectileDirection direction) => new SamusFiringStance(running, direction) switch
    {
        (false, SamusProjectileDirection.UpFacingRight) => new Offset16<RoomPixels>(2, -8),
        (false, SamusProjectileDirection.UpRight) => new Offset16<RoomPixels>(13, -13),
        (false, SamusProjectileDirection.Right) => new Offset16<RoomPixels>(11, 1),
        (false, SamusProjectileDirection.DownRight) => new Offset16<RoomPixels>(13, 4),
        (false, SamusProjectileDirection.DownFacingRight) => new Offset16<RoomPixels>(2, 13),
        (false, SamusProjectileDirection.DownFacingLeft) => new Offset16<RoomPixels>(-5, 13),
        (false, SamusProjectileDirection.DownLeft) => new Offset16<RoomPixels>(-14, 4),
        (false, SamusProjectileDirection.Left) => new Offset16<RoomPixels>(-11, 1),
        (false, SamusProjectileDirection.UpLeft) => new Offset16<RoomPixels>(-19, -19),
        (false, SamusProjectileDirection.UpFacingLeft) => new Offset16<RoomPixels>(-2, -8),
        (true, SamusProjectileDirection.UpFacingRight) => new Offset16<RoomPixels>(2, -8),
        (true, SamusProjectileDirection.UpRight) => new Offset16<RoomPixels>(15, -16),
        (true, SamusProjectileDirection.Right) => new Offset16<RoomPixels>(15, -2),
        (true, SamusProjectileDirection.DownRight) => new Offset16<RoomPixels>(13, 1),
        (true, SamusProjectileDirection.DownFacingRight) => new Offset16<RoomPixels>(2, 13),
        (true, SamusProjectileDirection.DownFacingLeft) => new Offset16<RoomPixels>(-5, 13),
        (true, SamusProjectileDirection.DownLeft) => new Offset16<RoomPixels>(-13, 1),
        (true, SamusProjectileDirection.Left) => new Offset16<RoomPixels>(-13, -2),
        (true, SamusProjectileDirection.UpLeft) => new Offset16<RoomPixels>(-15, -16),
        (true, SamusProjectileDirection.UpFacingLeft) => new Offset16<RoomPixels>(-2, -8),
        _ => throw new InvalidOperationException("Muzzle source owner must contain a named direction."),
    };

    /// <summary>Reads the selected X/Y muzzle offsets using the native low-nibble direction and running table layout.</summary>
    internal static Offset16<RoomPixels> Read(bool running, SamusProjectileDirectionWord direction)
    {
        int offset = direction.DirectionIndex * sizeof(ushort);
        int x = running ? SamusFiringTableDefinitions.RunningXOrigins : SamusFiringTableDefinitions.DefaultXOrigins;
        int y = running ? SamusFiringTableDefinitions.RunningYOrigins : SamusFiringTableDefinitions.DefaultYOrigins;
        return new Offset16<RoomPixels>(ReadWord(x + offset), ReadWord(y + offset));
    }

    /// <summary>Resolves authored muzzle words first, then reproduces adjacent cooldown bytes reached by out-of-range low-nibble directions.</summary>
    private static short ReadWord(int offset)
    {
        int originOffset = offset - SamusFiringTableDefinitions.DefaultXOrigins;
        if (originOffset >= 0 && originOffset < 40 * sizeof(ushort) && (originOffset & 1) == 0)
        {
            int word = originOffset / sizeof(ushort);
            var muzzle = Muzzle(word >= 20, SamusProjectileDirections.At(word % 10));
            return word % 20 < 10 ? muzzle.X : muzzle.Y;
        }
        return unchecked((short)BinaryPrimitives.ReadUInt16LittleEndian([SamusProjectileCooldowns.ReadByte(offset), SamusProjectileCooldowns.ReadByte(offset + 1)]));
    }
}
